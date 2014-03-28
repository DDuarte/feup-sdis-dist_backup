using System;
using System.Reactive.Linq;
using System.Threading.Tasks;
using DBS.Messages;
using DBS.Messages.Enhancements;

namespace DBS.Protocols.Enhancements
{
    class EnhancedRestoreChunkACKService : IService<GetChunkMessage>
    {
        public void OnNext(GetChunkMessage msg)
        {
            var fileChunk = new FileChunk(msg.FileId, msg.ChunkNo);
            if (!fileChunk.Exists()) // we don't have this chunk, do nothing
                return;
            
            var ackReceived = false;
            var disposable = Core.Instance.MDRChannel.Received
                .Where(message => message.MessageType == MessageType.ACK)
                .Cast<ACKMessage>()
                .Where(message => message.ChunkNo == msg.ChunkNo &&
                    message.FileId == msg.FileId)
                .Subscribe(_ => ackReceived = true);

            Task.Delay(Core.Instance.RandomDelay).Wait(); // random delay uniformly distributed
            disposable.Dispose();

            if (!ackReceived)
            {
                var ackMessage = new ACKMessage(fileChunk);
                Core.Instance.MDRChannel.Send(ackMessage);
            }
        }

        public void OnError(Exception error)
        {
            Core.Instance.Log.Error("EnhancedRestoreChunkACKService:OnError", error);
        }

        public void OnCompleted()
        {
            Core.Instance.Log.Info("EnhancedRestoreChunkACKService:OnCompleted");
        }

        public void Start()
        {
            Core.Instance.Log.Info("Starting EnhancedRestoreChunkACKService");
            Core.Instance.MCChannel.Received
                 .Where(message => message.MessageType == MessageType.GetChunk)
                 .Cast<GetChunkMessage>()
                 .Subscribe(this);
        }

        public void Stop()
        {
            Core.Instance.Log.Info("EnhancedRestoreChunkAckService:Stop");
        }
    }
}
