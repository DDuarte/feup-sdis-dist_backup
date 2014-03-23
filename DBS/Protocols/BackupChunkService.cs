using System;
using System.Reactive.Linq;
using System.Threading;

namespace DBS.Protocols
{
    /// <summary>
    /// Listens to PUTCHUNK messages on MDB
    /// </summary>
    public class BackupChunkService : IService
    {
        public void Start()
        {
            Core.Instance.MDBChannel.Received
                .Where(message => message.MessageType == MessageType.PutChunk)
                .Subscribe(this);
        }

        public void Stop()
        {
            Console.WriteLine("BackupChunkService:Stop");
        }

        public void OnNext(Message msg)
        {
            if (msg.FileId == null)
            {
                Console.WriteLine("BackupChunkService: bad msg, ChunkNo has no value.");
                return;
            }

            if (!msg.ChunkNo.HasValue)
            {
                Console.WriteLine("BackupChunkService: bad msg, ChunkNo has no value.");
                return;
            }

            if (!msg.ReplicationDeg.HasValue)
            {
                Console.WriteLine("BackupChunkService: bad msg, ReplicationDeg has no value.");
                return;
            }

            var fileChunk = new FileChunk(msg.FileId, msg.ChunkNo.Value);
            var stored = fileChunk.SetData(msg.Body);
            if (stored.HasValue && stored.Value)
                Core.Instance.Store.IncrementActualDegree(fileChunk.FileName, msg.ReplicationDeg.Value);
            else if (!stored.HasValue)
            {
                Console.WriteLine("BackupChunkService: Could not store file {0}", fileChunk);
                return;
            }
            // otherwise file is already created: send Stored but do not increment degrees

            Thread.Sleep(Core.Instance.Rnd.Next(0, 401)); // random delay uniformly distributed between 0 and 400 ms
            Core.Instance.MCChannel.Send(Message.BuildStoredMessage(fileChunk));
        }

        public void OnError(Exception error)
        {
            Console.WriteLine("BackupChunkService:OnError: {0}", error);
        }

        public void OnCompleted()
        {
            Console.WriteLine("BackupChunkService:OnCompleted");
        }
    }
}