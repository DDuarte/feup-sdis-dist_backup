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
            var fc = new FileChunk(msg.FileId, msg.ChunkNo);
            if (!Core.Instance.ChunkPeers.HasChunkPeer(fc, msg.RemoteEndPoint.Address))
                if (Core.Instance.ChunkPeers.GotWantedReplicationDegree(fc))
                    Core.Instance.ChunkPeers.AddChunkPeer(fc, msg.RemoteEndPoint.Address);
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
