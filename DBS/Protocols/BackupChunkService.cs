using System;
using System.Linq;
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

            var dirSize = SpaceReclaimingWatcher.GetDirectorySize(Core.Instance.BackupDirectory);

            if (dirSize + msg.Body.Length > Core.Instance.MaxBackupSize)
            {
                Console.WriteLine(
                    "BackupChunkService:OnNext: Got no space to store {0}, trying to evict some other chunks", fileChunk);
                foreach (var fileChunkf in Core.Instance.Store.Where(f => f.Value.ActualDegree > f.Value.WantedDegree)
                        .Select(f => f.Key.Split('_'))
                        .Select(keyParts => new {keyParts, fileIdStr = keyParts[0]})
                        .Select(@t => new {@t, chunkNo = int.Parse(@t.keyParts[1])})
                        .Select(@t => new FileChunk(new FileId(@t.@t.fileIdStr), @t.chunkNo))) // lol.
                {
                    Console.WriteLine("BackupChunkService:OnNext: Starting SpaceReclaimingProtocol for {0}",
                        fileChunkf);
                    new SpaceReclaimingProtocol(fileChunk).Run();
                }
            }

            if (dirSize + msg.Body.Length > Core.Instance.MaxBackupSize)
            {
                Console.WriteLine(
                    "BackupChunkService:OnNext: Really have no space to store any file. Giving up on storing {0}",
                    fileChunk);
                return;
            }

            var stored = fileChunk.SetData(msg.Body);
            if (stored.HasValue && stored.Value)
                Core.Instance.Store.IncrementActualDegree(fileChunk.FileName, msg.ReplicationDeg.Value);
            else if (!stored.HasValue)
            {
                Console.WriteLine("BackupChunkService: Could not store file {0}", fileChunk);
                return;
            }
            // otherwise file is already created: send Stored but do not increment degrees

            Thread.Sleep(Core.Instance.RandomDelay); // random delay uniformly distributed
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
