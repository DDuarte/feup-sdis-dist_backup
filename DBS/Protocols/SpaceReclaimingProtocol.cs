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
                foreach (var fc in Core.Instance.Store
                    .Where(f => f.Value.ActualDegree > f.Value.WantedDegree)
                    .Select(d => new FileChunk(d.Key)))
                {
                    Core.Instance.Log.InfoFormat("BackupChunkService:OnNext: Starting SpaceReclaimingProtocol for {0}", fc);
                    DeleteChunk(fc);
                }
            });
        }
    }
}
