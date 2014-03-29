using System.Threading.Tasks;
using DBS.Messages;

namespace DBS.Protocols
{
    /// <summary>
    /// Launches the chunk backup subprotocol
    /// </summary>
    public class BackupChunkSubprotocol : IProtocol
    {
        private readonly FileChunk _fileChunk;
        private readonly int _replicationDegree;
        private readonly byte[] _data;

        public BackupChunkSubprotocol(FileChunk fileChunk, int replicationDegree, byte[] data)
        {
            _fileChunk = fileChunk;
            _replicationDegree = replicationDegree;
            _data = data;
        }

        private void SendChunk()
        {
            var timeout = Core.Instance.Config.BackupChunkTimeout;
            var maxRetries = Core.Instance.Config.BackupChunkRetries;
            var multi = Core.Instance.Config.BackupChunkTimeoutMultiplier;

            int retryCount;
            var success = false;
            var count = 0;
            for (retryCount = 0; retryCount < maxRetries; retryCount++)
            {
                var msg = new PutChunkMessage(_fileChunk, _replicationDegree, _data);
                Core.Instance.MDBChannel.Send(msg);

                Task.Delay(timeout).Wait();

                count = Core.Instance.ChunkPeers.CountChunkPeer(_fileChunk);
                if (count >= _replicationDegree)
                {
                    success = true;
                    break;
                }

                timeout = (int) (timeout * multi);
                if (retryCount != maxRetries - 1) // not last iter
                    Core.Instance.Log.InfoFormat("{0}: ChunkReplication degree is {1} but wanted {2}. Timeout increased to {3}",
                        _fileChunk, count, _replicationDegree, timeout);
            }

            Core.Instance.Log.InfoFormat(
                success ? "{0}: Stored: retries {1}, rep degree {2}" : "{0}: Giving up: retries {1}, rep degree {2}",
                _fileChunk, retryCount, count);
        }

        public Task Run()
        {
            Core.Instance.Log.InfoFormat("Starting BackupChunkSubprotocol: {0}, {1}", _fileChunk, _replicationDegree);
            return Task.Factory.StartNew(SendChunk);
        }
    }
}
