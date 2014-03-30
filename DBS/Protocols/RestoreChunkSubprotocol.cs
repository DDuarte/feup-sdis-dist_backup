using System;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using DBS.Messages;

namespace DBS.Protocols
{
    /// <summary>
    /// Launches the initiator-peer Chunk Restore Protocol
    /// </summary>
    class RestoreChunkSubprotocol : IProtocol
    {
        private readonly FileChunk _fileChunk;
        private const int MaxRetries = 5;

        public ChunkMessage Message { get; private set; }

        public RestoreChunkSubprotocol(FileChunk fileChunk)
        {
            _fileChunk = fileChunk;
        }

        private void SendGetChunk()
        {
            int timeout = 1000;
            bool timedOut;
            int retry = 0;
            do
            {
                timedOut = false;
                Core.Instance.MCChannel.Send(new GetChunkMessage(_fileChunk));

                try
                {
                    // wait for response
                    Message = Core.Instance.MDRChannel.Received
                        .Where(message => message.MessageType == MessageType.Chunk)
                        .Cast<ChunkMessage>()
                        .Where(message => message.ChunkNo == _fileChunk.ChunkNo &&
                                          message.FileId == _fileChunk.FileId)
                        .Timeout(TimeSpan.FromMilliseconds(timeout))
                        .Next().First();
                }
                catch (TimeoutException)
                {
                    timedOut = true;
                    ++retry;
                    timeout *= 2;
                    Core.Instance.Log.ErrorFormat(
                        "RestoreChunkSubprotocol: Could not fetch {0} from the network. Retrying.",
                        _fileChunk);
                }

            } while (timedOut && retry < MaxRetries);

            if (timedOut)
                Core.Instance.Log.ErrorFormat(
                    "RestoreChunkSubprotocol: Could not fetch {0} from the network. Giving up.",
                    _fileChunk);
        }

        public Task Run()
        {
            Core.Instance.Log.InfoFormat("Starting RestoreChunkSubprotocol: {0}", _fileChunk);
            return Task.Factory.StartNew(SendGetChunk);
        }
    }
}
