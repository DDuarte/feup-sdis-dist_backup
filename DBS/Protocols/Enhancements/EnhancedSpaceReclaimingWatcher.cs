using System;
using System.Linq;
using System.Reactive.Linq;

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
            var chunkList =
                Core.Instance.Store.Where(pair => pair.Value.ActualDegree < pair.Value.WantedDegree).ToList();

            chunkList.Sort(
                (pair1, pair2) =>
                {
                    var delta1 = Math.Abs(pair1.Value.WantedDegree - pair1.Value.ActualDegree);
                    var delta2 = Math.Abs(pair2.Value.WantedDegree - pair2.Value.ActualDegree);

                    if (delta1 > delta2)
                        return -1;
                    else if (delta1 < delta2)
                        return 1;
                    else
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
                        continue;
                    }

                    new BackupChunkSubprotocol(fileChunk, chunk1.Value.WantedDegree, fileChunk.GetData()).Run().Wait();
                }
                catch (Exception ex)
                {
                    Core.Instance.Log.Error("CheckChunks", ex);
                }
            }
        }
    }
}
