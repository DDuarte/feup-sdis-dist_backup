using System;

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

        public void Run()
        {
            if (!_fileChunk.Delete())
            {
                Console.WriteLine("SpaceReclaimingProtocol: Could not delete file {0}", _fileChunk);
                return;
            }

            var msg = Message.BuildRemovedMessage(_fileChunk);
            Core.Instance.MCChannel.Send(msg);
        }
    }
}
