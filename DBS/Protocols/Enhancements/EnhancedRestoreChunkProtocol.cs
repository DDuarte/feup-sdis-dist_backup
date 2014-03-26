using System;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using DBS.Messages;
using DBS.Messages.Enhancements;

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
                Core.Instance.MCChannel.Send(new GetChunkMessage(_fileChunk));

                try
                {
                    var src1 = Core.Instance.MDRChannel.Received
                        .Where(message => message.MessageType == MessageType.Chunk)
                        .Cast<ChunkMessage>()
                        .Where(message => message.ChunkNo == _fileChunk.ChunkNo &&
                                          message.FileId == _fileChunk.FileId)
                        .Cast<Message>();

                    var src2 = Core.Instance.MDRChannel.Received
                        .Where(message => message.MessageType == MessageType.ACK)
                        .Cast<ACKMessage>()
                        .Where(message => message.ChunkNo == _fileChunk.ChunkNo &&
                                          message.FileId == _fileChunk.FileId)
                        .Cast<Message>();

                    // wait for response
                    var msg = src1.Merge(src2).Timeout(TimeSpan.FromMilliseconds(Timeout))
                        .Next().First();

                    if (msg.MessageType == MessageType.Chunk) // same behaviour as the regular protocol
                    {
                        ChunkMessage = msg;
                        return;
                    }

                    // TODO
                }
                catch (TimeoutException)
                {
                    Console.WriteLine("RestoreChunkSubprotocol: Could not fetch {0} from the network.", _fileChunk);
                }
            });
        }
    }
}
