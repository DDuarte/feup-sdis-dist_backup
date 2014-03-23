using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DBS.Persistence;

namespace DBS.Protocols
{
    /// <summary>
    /// Listens to REMOVED messages on MC
    /// </summary>
    public class SpaceReclaimingService : IService
    {
        public void Start()
        {
            Core.Instance.MCChannel.Received
                .Where(message => message.MessageType == MessageType.Removed)
                .Subscribe(this);
        }

        public void Stop()
        {
            throw new NotImplementedException();
        }

        public void OnNext(Message msg)
        {
            if (msg.FileId == null)
            {
                Console.WriteLine("SpaceReclaimingService: bad msg, ChunkNo has no value.");
                return;
            }

            if (!msg.ChunkNo.HasValue)
            {
                Console.WriteLine("BackupChunkService: bad msg, ChunkNo has no value.");
                return;
            }

            var fileChunk = new FileChunk(msg.FileId, msg.ChunkNo.Value);

            Core.Instance.Store.DecrementActualDegree(fileChunk.FileName);
            ReplicationDegrees rd;
            Core.Instance.Store.TryGetDegrees(fileChunk.FileName, out rd);
            if (rd.ActualDegree < rd.WantedDegree)
            {
                try
                {
                    var putChunkReceived = false;
                    var disposable = Core.Instance.MDBChannel.Received.Where(message =>
                        message.MessageType == MessageType.PutChunk &&
                        message.ChunkNo == fileChunk.ChunkNo &&
                        message.FileId == fileChunk.FileId).Subscribe(_ => putChunkReceived = true);

                    Thread.Sleep(Core.Instance.Rnd.Next(0, 401)); // random delay uniformly distributed between 0 and 400 ms

                    if (!putChunkReceived)
                        new BackupChunkSubprotocol(fileChunk, rd.WantedDegree, fileChunk.GetData()).Run();

                    disposable.Dispose();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("RestoreChunkService: " + ex);
                }
            }
        }

        public void OnError(Exception error)
        {
            throw new NotImplementedException();
        }

        public void OnCompleted()
        {
            throw new NotImplementedException();
        }
    }
}
