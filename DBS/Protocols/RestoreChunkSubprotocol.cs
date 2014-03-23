using System;
using System.Linq;
using System.Reactive.Linq;

namespace DBS.Protocols
{
    /// <summary>
    /// Launches the initiator-peer Chunk Restore Protocol
    /// </summary>
    class RestoreChunkSubprotocol : IProtocol
    {
        private readonly FileChunk _fileChunk;
        public Message ChunkMessage { get; private set; }

        public RestoreChunkSubprotocol(FileChunk fileChunk)
        {
            _fileChunk = fileChunk;
        }

        private void SendGetChunk()
        {
            Core.Instance.MCChannel.Send(Message.BuildGetChunkMessage(_fileChunk));

            try
            {
                // wait for response
                ChunkMessage = Core.Instance.MDRChannel.Received.Where(message =>
                    message.MessageType == MessageType.Chunk &&
                    message.ChunkNo == _fileChunk.ChunkNo &&
                    message.FileId == _fileChunk.FileId)
                    .Timeout(TimeSpan.FromMilliseconds(5000))
                    .Next().First();
            }
            catch (TimeoutException)
            {
                Console.WriteLine("RestoreChunkSubprotocol: Could not fetch {0} from the network.", _fileChunk);
            }
        }

        public void Run()
        {
            SendGetChunk();
        }
    }
}
