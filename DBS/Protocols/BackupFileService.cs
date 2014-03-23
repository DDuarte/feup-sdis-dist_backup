using System;
using System.IO;
using System.Reactive.Linq;
using System.Threading;

namespace DBS.Protocols
{
    /// <summary>
    /// Listens to PUTCHUNK messages on MDB
    /// </summary>
    public class BackupFileService : IService
    {
        public void Start()
        {
            Core.Instance.MDBChannel.Received
                .Where(message => message.MessageType == MessageType.PutChunk)
                .Subscribe(this);
        }

        public void Stop()
        {
            Console.WriteLine("BackupFileService:Stop");
        }

        public void OnNext(Message msg)
        {
            if (!msg.ChunkNo.HasValue)
            {
                Console.WriteLine("BackupFileService: bad msg, ChunkNo has no value.");
                return;
            }

            if (!msg.ReplicationDeg.HasValue)
            {
                Console.WriteLine("BackupFileService: bad msg, ReplicationDeg has no value.");
                return;
            }

            var fileChunk = new FileChunk(msg.FileId, msg.ChunkNo.Value);
            if (fileChunk.Exists())
            {
                try
                {
                    File.WriteAllBytes(fileChunk.FullFileName, msg.Body);
                    Core.Instance.Store.IncrementActualDegree(fileChunk.FileName, msg.ReplicationDeg.Value);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("BackupFileService: " + ex);
                    return;
                }
            }

            Thread.Sleep(Core.Instance.Rnd.Next(0, 401)); // random delay uniformly distributed between 0 and 400 ms
            Core.Instance.MCChannel.Send(Message.BuildStoredMessage(fileChunk));
        }

        public void OnError(Exception error)
        {
            Console.WriteLine("BackupFileService:OnError: {0}", error);
        }

        public void OnCompleted()
        {
            Console.WriteLine("BackupFileService:OnCompleted");
        }
    }
}