using System;
using System.Reactive.Linq;
using System.Threading.Tasks;
using DBS.Messages;

namespace DBS.Protocols
{
    /// <summary>
    /// Listens to REMOVED messages on MC
    /// </summary>
    public class SpaceReclaimingService : IServiceObserver<RemovedMessage>
    {
        public void Start()
        {
            Core.Instance.Log.Info("Starting SpaceReclaimingService");
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

            if (!Core.Instance.ChunkPeers.RemoveChunkPeer(fileChunk, msg.RemoteEndPoint.Address))
                return; // we don't have this file, ignore

            int wantedDegree, actualDegree;
            Core.Instance.ChunkPeers.TryGetDegrees(fileChunk, out wantedDegree, out actualDegree);
            if (actualDegree >= wantedDegree) return; // can't delete because 
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
                {
                    var data = fileChunk.GetData();
                    if (data == null)
                        Core.Instance.Log.ErrorFormat(
                            "SpaceReclaimingService: Could not start BackupChunkProtocol" +
                            " for {0} because it no longer exists here.", fileChunk);
                    else
                        new BackupChunkSubprotocol(fileChunk, wantedDegree, data).Run();
                }

                disposable.Dispose();
            }
            catch (Exception ex)
            {
                Core.Instance.Log.Error("SpaceReclaimingService", ex);
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
