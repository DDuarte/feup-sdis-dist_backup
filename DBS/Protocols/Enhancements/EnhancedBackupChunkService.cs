using System;
using System.Reactive.Linq;
using System.Threading.Tasks;
using DBS.Messages;

namespace DBS.Protocols.Enhancements
{
    /// <summary>
    /// Listens to PUTCHUNK messages on MDB
    /// </summary>
    public class EnhancedBackupChunkService : IServiceObserver<PutChunkMessage>
    {
        public void Start()
        {
            Core.Instance.Log.Info("Starting EnhancedBackupChunkService");
            Core.Instance.MDBChannel.Received
                .Where(message => message.MessageType == MessageType.PutChunk)
                .Cast<PutChunkMessage>()
                .Subscribe(this);
        }

        public void Stop()
        {
            Core.Instance.Log.Info("EnhancedBackupChunkService:Stop");
        }

        public void OnNext(PutChunkMessage msg)
        {
            var fileChunk = new FileChunk(msg.FileId, msg.ChunkNo);
            Core.Instance.ChunkPeers.SetWantedReplicationDegree(fileChunk, msg.ReplicationDeg);

            var dirSize = Utilities.Utilities.GetDirectorySize(Core.Instance.Config.BackupDirectory);
            if (dirSize + msg.Body.Length > Core.Instance.Config.MaxBackupSize)
            {
                Core.Instance.Log.InfoFormat(
                    "EnhancedBackupChunkService:OnNext: Got no space to store {0}, trying to evict some other chunks", fileChunk);
                new SpaceReclaimingProtocol(false).Run().Wait();

                dirSize = Utilities.Utilities.GetDirectorySize(Core.Instance.Config.BackupDirectory);
                if (dirSize + msg.Body.Length > Core.Instance.Config.MaxBackupSize)
                {
                    Core.Instance.Log.InfoFormat(
                        "EnhancedBackupChunkService:OnNext: Really have no space to store any file. Giving up on storing {0}",
                        fileChunk);
                    return;
                }
            }


            var count = 0;
            var disp = Core.Instance.MCChannel.Received
                .Where(message => message.MessageType == MessageType.Stored)
                .Cast<StoredMessage>()
                .Where(message => message.ChunkNo == msg.ChunkNo && message.FileId == msg.FileId)
                .Subscribe(message => count++);

            Task.Delay(Core.Instance.RandomDelay).Wait();

            disp.Dispose();

            if (count >= msg.ReplicationDeg)
            {
                Core.Instance.Log.InfoFormat("EnhancedBackupChunkService: Not storing {0}#{1} because replication degree " +
                                             "has been ensured by other peers (got {2}, wanted {3}",
                                             msg.FileId.ToStringSmall(), msg.ChunkNo, count, msg.ReplicationDeg);
                return;
            }

            var stored = fileChunk.SetData(msg.Body);
            if (!stored.HasValue)
            {
                Core.Instance.Log.ErrorFormat("EnhancedBackupChunkService: Could not store file {0}", fileChunk);
                return;
            }
            // otherwise file is already created

            Core.Instance.MCChannel.Send(new StoredMessage(fileChunk));
        }

        public void OnError(Exception error)
        {
            Core.Instance.Log.Error("EnhancedBackupChunkService:OnError", error);
        }

        public void OnCompleted()
        {
            Core.Instance.Log.Info("EnhancedBackupChunkService:OnCompleted");
        }
    }
}
