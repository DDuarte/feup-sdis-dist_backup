using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
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

            var data = fileChunk.GetData();
            var ackMessage = new ACKMessage(fileChunk);
            Core.Instance.MDRChannel.Send(ackMessage);
        }

        public void OnError(Exception error)
        {
            
        }

        public void OnCompleted()
        {
            
        }

        public void Start()
        {
            Core.Instance.MCChannel.Received
                 .Where(message => message.MessageType == MessageType.GetChunk)
                 .Cast<GetChunkMessage>()
                 .Subscribe(this);
        }

        public void Stop()
        {

        }
    }
}
