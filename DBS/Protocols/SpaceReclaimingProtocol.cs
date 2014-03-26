using System.Linq;
using System.Threading.Tasks;
using DBS.Messages;

namespace DBS.Protocols
{
    /// <summary>
    /// Launches the space reclaiming protocol
    /// </summary>
    /// <remarks>
    /// Space reclaiming "algorithm": find all chunks whose actual replication degree is
    /// higher than the required application degree and delete them
    /// </remarks>
    public class SpaceReclaimingProtocol : IProtocol
    {
        private static void DeleteChunk(FileChunk chunk)
        {
            if (!chunk.Delete())
            {
                Core.Instance.Log.ErrorFormat("SpaceReclaimingProtocol: Could not delete file {0}", chunk);
                return;
            }

            // Not updating actualDegree here. Will be done when we receive our own REMOVED.

            var msg = new RemovedMessage(chunk);
            Core.Instance.MCChannel.Send(msg);
        }

        public Task Run()
        {
            return Task.Factory.StartNew(() =>
            {
                foreach (var fileChunk in Core.Instance.Store.Where(f => f.Value.ActualDegree > f.Value.WantedDegree)
                        .Select(f => f.Key.Split('_'))
                        .Select(keyParts => new { keyParts, fileIdStr = keyParts[0] })
                        .Select(@t => new { @t, chunkNo = int.Parse(@t.keyParts[1]) })
                        .Select(@t => new FileChunk(new FileId(@t.@t.fileIdStr), @t.chunkNo))) // lol.
                {
                    Core.Instance.Log.InfoFormat("BackupChunkService:OnNext: Starting SpaceReclaimingProtocol for {0}",
                        fileChunk);
                    DeleteChunk(fileChunk);
                }
            });
        }
    }
}
