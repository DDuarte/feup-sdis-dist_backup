using System;
using System.Threading.Tasks;

namespace DBS.Protocols
{
    /// <summary>
    /// Launches the space reclaiming protocol (deletes a file and sends a REMOVED)
    /// </summary>
    public class SpaceReclaimingProtocol : IProtocol
    {
        private readonly FileChunk _fileChunk;

        public SpaceReclaimingProtocol(FileChunk fileChunk)
        {
            _fileChunk = fileChunk;
        }

        public Task Run()
        {
            return Task.Factory.StartNew(() =>
            {
                if (!_fileChunk.Delete())
                {
                    Console.WriteLine("SpaceReclaimingProtocol: Could not delete file {0}", _fileChunk);
                    return;
                }

                // Not updating actualDegree here. Will be done when we receive our own REMOVED.

                var msg = Message.BuildRemovedMessage(_fileChunk);
                Core.Instance.MCChannel.Send(msg);
            });
        }
    }
}
