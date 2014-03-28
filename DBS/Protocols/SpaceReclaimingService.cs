using System;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using DBS.Messages;
using DBS.Persistence;

namespace DBS.Protocols
{
    /// <summary>
    /// Listens to REMOVED messages on MC
    /// </summary>
    public class SpaceReclaimingService : IService<RemovedMessage>
    {
        public void Start()
        {
            Core.Instance.MCChannel.Received
                .Where(message => message.MessageType == MessageType.Removed)
                .Cast<RemovedMessage>()
                .Subscribe(this);
        }

        public void Stop()
        {
            Core.Instance.Log.Info("SpaceReclaimingService:Stop");
        }

        public void OnNext(RemovedMessage msg)
        {
            var fileChunk = new FileChunk(msg.FileId, msg.ChunkNo);

            Core.Instance.Store.DecrementActualDegree(fileChunk.FileName);
            ReplicationDegrees rd;
            Core.Instance.Store.TryGetDegrees(fileChunk.FileName, out rd);
            if (rd.ActualDegree < rd.WantedDegree)
            {
                try
                {
                    var putChunkReceived = false;
                    var disposable = Core.Instance.MDBChannel.Received
                        .Where(message => message.MessageType == MessageType.PutChunk)
                        .Cast<PutChunkMessage>()
                        .Where(message => message.ChunkNo == fileChunk.ChunkNo &&
                            message.FileId == fileChunk.FileId)
                        .Subscribe(_ => putChunkReceived = true);

                    Task.Delay(Core.Instance.RandomDelay).Wait(); // random delay uniformly distributed

                    if (!putChunkReceived)
                        new BackupChunkSubprotocol(fileChunk, rd.WantedDegree, fileChunk.GetData()).Run();

                    disposable.Dispose();
                }
                catch (Exception ex)
                {
                    Core.Instance.Log.Error("SpaceReclaimingService", ex);
                }
            }
        }

        public void OnError(Exception error)
        {
            Core.Instance.Log.Error("SpaceReclaimingService:OnError", error);
        }

        public void OnCompleted()
        {
            Core.Instance.Log.Info("SpaceReclaimingService:OnCompleted");
        }
    }
}
