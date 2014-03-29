using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using DBS.Messages;

namespace DBS.Protocols.Enhancements
{
    class EnhancedSpaceReclaimingWatcher : IService
    {
        private const int MinutesTimeSpan = 1;
        private IDisposable _intervalSubs;

        public void Start()
        {
            Core.Instance.Log.Info("Starting EnhancedSpaceReclaimingWatcher");
            _intervalSubs = Observable.Interval(TimeSpan.FromMinutes(MinutesTimeSpan)).Subscribe(_ => CheckChunks());
        }

        public void Stop()
        {
            if (_intervalSubs != null)
                _intervalSubs.Dispose();
        }

        private static void CheckChunks()
        {
            //var chunkList =
            //    Core.Instance.Store.Where(pair => pair.Value.ActualDegree < pair.Value.WantedDegree).ToList();

            var chunks = Core.Instance.ChunkPeers.Select(peer => peer.Chunk).Distinct();
            var chunkList = new List<KeyValuePair<string, Tuple<int, int>>>();
            foreach (var c in chunks)
            {
                int wantedDegree, actualDegree;
                if (!Core.Instance.ChunkPeers.TryGetDegrees(c, out wantedDegree, out actualDegree))
                {
                    Core.Instance.Log.ErrorFormat("SpaceReclaimingProtocol: Could not get degrees for {0}", c);
                    continue;
                }

                chunkList.Add(new KeyValuePair<string, Tuple<int, int>>(c, Tuple.Create(wantedDegree, actualDegree)));
            }

            chunkList.Sort((pair1, pair2) =>
            {
                var delta1 = Math.Abs(pair1.Value.Item1 - pair1.Value.Item2);
                var delta2 = Math.Abs(pair2.Value.Item1 - pair2.Value.Item2);

                if (delta1 > delta2)
                    return -1;
                if (delta1 < delta2)
                    return 1;
                return 0;
            });

            foreach (var chunk in chunkList)
            {
                var chunk1 = chunk;
                try
                {
                    var fileChunk = new FileChunk(chunk1.Key);
                    if (!fileChunk.Exists())
                    {
                        Core.Instance.Log.ErrorFormat(
                                "EnhancedSpaceReclaimingWatcher: Could not start BackupChunkProtocol" +
                                " for {0} because it no longer exists here.", fileChunk);
                        Core.Instance.MCChannel.Send(new RemovedMessage(fileChunk)); // we were supposed to have the file chunk
                        continue;
                    }

                    new BackupChunkSubprotocol(fileChunk, chunk1.Value.Item1, fileChunk.GetData()).Run().Wait();
                }
                catch (Exception ex)
                {
                    Core.Instance.Log.Error("CheckChunks", ex);
                }
            }
        }
    }
}
