using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBS.Protocols.Enhancements
{
    class EnhancedRestoreChunkProtocol : IProtocol
    {
        private readonly FileChunk _fileChunk;
        private const int Timeout = 5000;
        public Message ChunkMessage { get; private set; }
        public EnhancedRestoreChunkProtocol(FileChunk fileChunk)
        {
            _fileChunk = fileChunk;
        }

        public Task Run()
        {
            return Task.Factory.StartNew(() =>
            {
                Core.Instance.MCChannel.Send(Message.BuildGetChunkMessage(_fileChunk));

                try
                {
                    // wait for response
                    Message msg = Core.Instance.MDRChannel.Received.Where(message =>
                        (message.MessageType == MessageType.Chunk &&
                        message.ChunkNo == _fileChunk.ChunkNo &&
                        message.FileId == _fileChunk.FileId) ||
                        message.MessageType == MessageType.Ack)
                        .Timeout(TimeSpan.FromMilliseconds(Timeout))
                        .Next().First();

                    if (msg.MessageType == MessageType.Chunk) // same behaviour as the regular protocol
                    {
                        ChunkMessage = msg;
                        return;
                    }

                    
                }
                catch (TimeoutException)
                {
                    Console.WriteLine("RestoreChunkSubprotocol: Could not fetch {0} from the network.", _fileChunk);
                }
            });
        }
    }
}
