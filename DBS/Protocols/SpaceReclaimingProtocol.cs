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
            Core.Instance.Log.Info("Starting SpaceReclaimingProtocol");
            return Task.Factory.StartNew(() =>
            {
                var chunks = Core.Instance.ChunkPeers.Select(peer => peer.Chunk).Distinct();

                foreach (var c in chunks)
                {
                    int wantedDegree, actualDegree;
                    if (!Core.Instance.ChunkPeers.TryGetDegrees(c, out wantedDegree, out actualDegree))
                    {
                        Core.Instance.Log.ErrorFormat("SpaceReclaimingProtocol: Could not get degrees for {0}", c);
                        continue;
                    }

                    if (actualDegree > wantedDegree)
                    {
                        var fc = new FileChunk(c);
                        Core.Instance.Log.InfoFormat(@"SpaceReclaimingProtocol: Deleting chunk {0} because
                                                   degree {1} is higher than the desired {2}", fc,
                                                   actualDegree, wantedDegree);
                        DeleteChunk(fc);
                    }
                }
            });
        }
    }
}
