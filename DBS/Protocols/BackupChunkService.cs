using System;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using DBS.Messages;
using Util = DBS.Utilities.Utilities;

namespace DBS.Protocols
{
    /// <summary>
    /// Listens to PUTCHUNK messages on MDB
    /// </summary>
    public class BackupChunkService : IService<PutChunkMessage>
    {
        public void Start()
        {
            Core.Instance.Log.Info("Starting BackupChunkService");
            Core.Instance.MDBChannel.Received
                .Where(message => message.MessageType == MessageType.PutChunk)
                .Cast<PutChunkMessage>()
                .Subscribe(this);
        }

        public void Stop()
        {
            Core.Instance.Log.Info("BackupChunkService:Stop");
        }

        public void OnNext(PutChunkMessage msg)
        {
            var fileChunk = new FileChunk(msg.FileId, msg.ChunkNo);

            var dirSize = Util.GetDirectorySize(Core.Instance.Config.BackupDirectory);
            if (dirSize + msg.Body.Length > Core.Instance.Config.MaxBackupSize)
            {
                Core.Instance.Log.InfoFormat(
                    "BackupChunkService:OnNext: Got no space to store {0}, trying to evict some other chunks", fileChunk);
                new SpaceReclaimingProtocol().Run().Wait();

                dirSize = Util.GetDirectorySize(Core.Instance.Config.BackupDirectory);
                if (dirSize + msg.Body.Length > Core.Instance.Config.MaxBackupSize)
                {
                    Core.Instance.Log.InfoFormat(
                        "BackupChunkService:OnNext: Really have no space to store any file. Giving up on storing {0}",
                        fileChunk);
                    return;
                }
            }

            var stored = fileChunk.SetData(msg.Body);
            if (stored.HasValue && stored.Value)
                Core.Instance.Store.IncrementActualDegree(fileChunk.FileName, msg.ReplicationDeg);
            else if (!stored.HasValue)
            {
                Core.Instance.Log.ErrorFormat("BackupChunkService: Could not store file {0}", fileChunk);
                return;
            }
            // otherwise file is already created: send Stored but do not increment degrees

            Task.Delay(Core.Instance.RandomDelay).Wait();
            Core.Instance.MCChannel.Send(new StoredMessage(fileChunk));
        }

        public void OnError(Exception error)
        {
            Core.Instance.Log.Error("BackupChunkService:OnError", error);
        }

        public void OnCompleted()
        {
            Core.Instance.Log.Info("BackupChunkService:OnCompleted");
        }
    }
}
