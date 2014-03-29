using System;
using System.Reactive.Linq;
using DBS.Messages;

namespace DBS.Protocols
{
    /// <summary>
    /// Listens to STORED messages on MC
    /// </summary>
    public class BackupChunkStoredService : IServiceObserver<StoredMessage>
    {
        public void Start()
        {
            Core.Instance.Log.Info("Starting BackupChunkSubprotocolStored");
            Core.Instance.MCChannel.Received
                .Where(message => message.MessageType == MessageType.Stored)
                .Cast<StoredMessage>()
                .Subscribe(this);
        }

        public void Stop()
        {
            Core.Instance.Log.Info("BackupChunkSubprotocolStored:Stop");
        }

        public void OnNext(StoredMessage msg)
        {
            Core.Instance.ChunkPeers.AddChunkPeer(new FileChunk(msg.FileId, msg.ChunkNo), msg.RemoteEndPoint.Address);
        }

        public void OnError(Exception error)
        {
            Core.Instance.Log.Error("BackupChunkSubprotocolStored:OnError", error);
        }

        public void OnCompleted()
        {
            Core.Instance.Log.Info("BackupChunkSubprotocolStored:OnCompleted");
        }
    }
}
