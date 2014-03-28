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
            foreach (var chunk in Core.Instance.Store.Where(pair => pair.Value.ActualDegree < pair.Value.WantedDegree))
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
