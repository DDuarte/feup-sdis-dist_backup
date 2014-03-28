using System;
using System.Linq;
using System.Reactive.Linq;

namespace DBS.Protocols.Enhancements
{
    class EnhancedSpaceReclaimingWatcher
    {
        private const int MinutesTimeSpan = 1;
        public void Start()
        {
            Core.Instance.Log.Info("Starting EnhancedSpaceReclaimingWatcher");
            Observable.Interval(TimeSpan.FromMinutes(MinutesTimeSpan)).Subscribe(_ => CheckChunks());
        }

        private static void CheckChunks()
        {
            foreach (var chunk in Core.Instance.Store.Where(pair => pair.Value.ActualDegree < pair.Value.WantedDegree))
            {
                var chunk1 = chunk;
                try
                {
                    var fileChunk = new FileChunk(chunk1.Key);
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
