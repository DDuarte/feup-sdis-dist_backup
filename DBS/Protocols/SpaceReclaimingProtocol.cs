using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DBS.Messages;
using Util = DBS.Utilities.Utilities;

namespace DBS.Protocols
{
    /// <summary>
    /// Launches the space reclaiming protocol
    /// </summary>
    public class SpaceReclaimingProtocol : IProtocol
    {
        private readonly bool _force;

        public SpaceReclaimingProtocol(bool force)
        {
            _force = force;
        }

        private static bool DeleteChunk(FileChunk chunk)
        {
            if (!chunk.Delete())
            {
                Core.Instance.Log.ErrorFormat("SpaceReclaimingProtocol: Could not delete file {0}", chunk);
                return false;
            }

            // Not updating actualDegree here. Will be done when we receive our own REMOVED.

            var msg = new RemovedMessage(chunk);
            Core.Instance.MCChannel.Send(msg);
            return true;
        }

        /// <summary>
        /// Space reclaiming "algorithm": find all chunks whose actual replication degree is
        /// higher than the required application degree and delete them
        /// </summary>
        private void SimpleReclaimer()
        {
            var chunks = Util.GetLocalFileChunks();
            foreach (var fc in chunks)
            {
                int wantedDegree, actualDegree;
                if (!Core.Instance.ChunkPeers.TryGetDegrees(fc, out wantedDegree, out actualDegree))
                {
                    Core.Instance.Log.ErrorFormat("SpaceReclaimingProtocol: Could not get degrees for {0}", fc);
                    continue;
                }

                if (actualDegree > wantedDegree)
                {
                    Core.Instance.Log.InfoFormat(@"SpaceReclaimingProtocol: Deleting chunk {0} because
                                                   degree {1} is higher than the desired {2}", fc,
                                               actualDegree, wantedDegree);
                    DeleteChunk(fc);
                }
            }
        }

        private void ForcedReclaimer()
        {
            var directorySize = Utilities.Utilities.GetDirectorySize(Core.Instance.Config.BackupDirectory);
            var sizeLimit = Core.Instance.Config.MaxBackupSize;

            if (directorySize < sizeLimit)
            {
                Core.Instance.Log.Info("ForcedReclaimer: directory size is less than limit, no cleaning up to do.");
                return;
            }

            var chunks = Util.GetLocalFileChunks()
                .Select(fc =>
                {
                    long size;
                    try
                    {
                        size = new FileInfo(fc.FullFileName).Length;
                    }
                    catch (Exception ex)
                    {
                        size = 0;
                        Core.Instance.Log.Error("ForcedReclaimer", ex);
                    }

                    int wantedDegree, actualDegree;
                    return !Core.Instance.ChunkPeers.TryGetDegrees(fc, out wantedDegree, out actualDegree)
                        ? null
                        : Tuple.Create(fc, Tuple.Create(wantedDegree, actualDegree, size));
                }).ToList();

            if (chunks.Count == 0)
            {
                Core.Instance.Log.Info("SpaceReclaimingProtocol:ForcedReclaimer: empty dir, finished");
                return;
            }

            chunks.Sort((pair1, pair2) =>
            {
                if (pair1 == null || pair2 == null)
                    return 0;

                var delta1 = Math.Abs(pair1.Item2.Item1 - pair1.Item2.Item2); // 1 - wanted, 2 - actual
                var delta2 = Math.Abs(pair2.Item2.Item1 - pair2.Item2.Item2);

                if (delta1 > delta2)
                    return -1;
                if (delta1 < delta2)
                    return 1;
                return 0;
            });

            var queue = new Queue<Tuple<FileChunk, Tuple<int, int, long>>>(chunks);

            while (directorySize > sizeLimit && queue.Count > 0)
            {
                var pair = queue.Dequeue();
                if (pair == null) continue;
                if (!DeleteChunk(pair.Item1)) continue;
                directorySize -= pair.Item2.Item3;
                Core.Instance.Log.InfoFormat("ForcedReclaimer: decreased dir size by {0} to {1}. limit is {2}",
                    pair.Item2.Item3, directorySize, sizeLimit);
            }
        }

        public Task Run()
        {
            Core.Instance.Log.Info("Starting SpaceReclaimingProtocol");
            return Task.Factory.StartNew(() =>
            {
                if (_force)
                    ForcedReclaimer();
                else
                    SimpleReclaimer();
            });
        }
    }
}
