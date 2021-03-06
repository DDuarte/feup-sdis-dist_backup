﻿using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using DBS.Messages;
using Util = DBS.Utilities.Utilities;

namespace DBS.Protocols.Enhancements
{
    class EnhancedSpaceReclaimingWatcher : IService
    {
        private const int SecondsTimeSpan = 30;
        private IDisposable _intervalSubs;

        public void Start()
        {
            Core.Instance.Log.Info("Starting EnhancedSpaceReclaimingWatcher");
            _intervalSubs = Observable.Interval(TimeSpan.FromSeconds(SecondsTimeSpan)).Subscribe(_ =>
            {
                new LookUpProtocol().Run().Wait();
                CheckChunks();
            });
        }

        public void Stop()
        {
            if (_intervalSubs != null)
                _intervalSubs.Dispose();
        }

        private static void CheckChunks()
        {
            var chunks = Util.GetLocalFileChunks();
            var chunkList = new List<KeyValuePair<FileChunk, Tuple<int, int>>>();
            foreach (var c in chunks)
            {
                int wantedDegree, actualDegree;
                if (!Core.Instance.ChunkPeers.TryGetDegrees(c, out wantedDegree, out actualDegree))
                {
                    Core.Instance.Log.ErrorFormat("SpaceReclaimingProtocol: Could not get degrees for {0}", c);
                    continue;
                }

                if (actualDegree < wantedDegree)
                    chunkList.Add(new KeyValuePair<FileChunk, Tuple<int, int>>(c, Tuple.Create(wantedDegree, actualDegree)));
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
                try
                {
                    if (!chunk.Key.Exists())
                    {
                        Core.Instance.Log.ErrorFormat(
                                "EnhancedSpaceReclaimingWatcher: Could not start BackupChunkProtocol" +
                                " for {0} because it no longer exists here.", chunk.Key);
                        Core.Instance.MCChannel.Send(new RemovedMessage(chunk.Key)); // we were supposed to have the file chunk
                        continue;
                    }

                    new BackupChunkSubprotocol(chunk.Key, chunk.Value.Item1, chunk.Key.GetData()).Run().Wait();
                }
                catch (Exception ex)
                {
                    Core.Instance.Log.Error("CheckChunks", ex);
                }
            }
        }
    }
}
