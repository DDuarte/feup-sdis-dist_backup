using System;
using System.Linq;
using System.Net.Sockets;
using System.Reactive.Linq;
using DBS.Messages.Enhancements;
using DBS.Utilities;

namespace DBS.Protocols.Enhancements
{
    class EnhancedRestoreChunkConnInfoService : IServiceObserver<ConnInfoMessage>
    {
        public void OnNext(ConnInfoMessage msg)
        {
            var fileChunk = new FileChunk(msg.FileId, msg.ChunkNo);
            if (!fileChunk.Exists()) // we don't have this chunk, do nothing
                return;

            try
            {
                using (var client = new TcpClient())
                {
                    client.Connect(msg.RemoteEndPoint.Address, msg.InitiatorPort);
                    var stream = client.GetStream();

                    var bytes = fileChunk.GetData();
                    stream.Write(bytes, 0, bytes.Length);
                }
            }
            catch (Exception ex)
            {
                Core.Instance.Log.ErrorFormat("EnhancedRestoreChunkConnInfoService: {0}", ex);
            }

            Core.Instance.Log.InfoFormat("EnhancedRestoreChunkConnInfoService: {0} sent", msg);
        }

        public void OnError(Exception error)
        {
            Core.Instance.Log.Error("EnhancedRestoreChunkConnInfoService:OnError", error);
        }

        public void OnCompleted()
        {
            Core.Instance.Log.Info("EnhancedRestoreChunkConnInfoService:OnCompleted");
        }

        public void Start()
        {
            Core.Instance.Log.Info("Starting EnhancedRestoreChunkConnInfoService");
            Core.Instance.MCChannel.Received
                .Where(message => message.MessageType == Messages.MessageType.ConnInfo)
                .Cast<ConnInfoMessage>().Where(message => NetworkUtilities.GetLocalIPAddresses().Contains(message.PassiveIP))
                .Subscribe(this);
        }

        public void Stop()
        {
            Core.Instance.Log.Info("EnhancedRestoreChunkConnInfoService:Stop");
        }
    }
}
