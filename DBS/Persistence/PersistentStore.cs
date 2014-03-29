using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace DBS.Persistence
{
    public class PersistentChunkPeers : IEnumerable<ChunkPeer>, IDisposable
    {
        private class Nothing
        {
            public byte N { get; set; }
        }

        private readonly PersistentDictionary<ChunkPeer, Nothing> _chunkPeers = // ((FileId, ChunkNo), IP) -> nothing
            new PersistentDictionary<ChunkPeer, Nothing>("store", "chunkpeer");

        private readonly PersistentDictionary<string, int> _wantedRepDegs = // (FileId, ChunkNo) -> wanted rep deg
            new PersistentDictionary<string, int>("store", "wanteddeg");

        public int CountChunkPeer(FileChunk chunk)
        {
            return _chunkPeers.Count(chunkPeer => chunkPeer.Key.Chunk == chunk.FileName);
        }

        public int CountChunkPeer(string fileName)
        {
            return _chunkPeers.Count(chunkPeer => chunkPeer.Key.Chunk == fileName);
        }

        public int CountChunkPeer(ChunkPeer chunkPeer)
        {
            return _chunkPeers.Count(cp => cp.Key == chunkPeer);
        }

        public bool HasChunkPeer(FileChunk chunk)
        {
            return _chunkPeers.Any(chunkPeer => chunkPeer.Key.Chunk == chunk.FileName);
        }

        public bool HasChunkPeer(string fileName)
        {
            return _chunkPeers.Any(chunkPeer => chunkPeer.Key.Chunk == fileName);
        }

        public bool TryGetDegrees(string fileName, out int wantedDeg, out int actualDeg)
        {
            if (!HasChunkPeer(fileName))
            {
                wantedDeg = -1;
                actualDeg = -1;
                return false;
            }

            if (!_wantedRepDegs.TryGetValue(fileName, out wantedDeg))
            {
                actualDeg = -1;
                return false;
            }

            actualDeg = CountChunkPeer(fileName);
            return true;
        }

        public bool TryGetDegrees(FileChunk chunk, out int wantedDeg, out int actualDeg)
        {
            if (!HasChunkPeer(chunk))
            {
                wantedDeg = -1;
                actualDeg = -1;
                return false;
            }

            if (!_wantedRepDegs.TryGetValue(chunk.FileName, out wantedDeg))
            {
                actualDeg = -1;
                return false;
            }

            actualDeg = CountChunkPeer(chunk);
            return true;
        }

        public void SetWantedReplicationDegree(FileChunk chunk, int wantedRepDeg)
        {
            if (_wantedRepDegs.ContainsKey(chunk.FileName))
            {
                _wantedRepDegs[chunk.FileName] = wantedRepDeg; // override
                return;
            }

            _wantedRepDegs.Add(chunk.FileName, wantedRepDeg);
        }

        public void AddChunkPeer(FileChunk chunk, IPAddress address)
        {
            var chunkPeer = new ChunkPeer { Chunk = chunk.FileName, IP = address.GetHashCode() };
            _chunkPeers.Add(chunkPeer, new Nothing());
        }

        public bool RemoveChunkPeer(FileChunk chunk, IPAddress ip)
        {
            var toRemove = _chunkPeers.Where(pair => pair.Key.Chunk == chunk.FileName && pair.Key.IP == ip.GetHashCode())
                .Select(pair => pair.Key).ToList();

            var any = false;
            foreach (var key in toRemove)
            {
                any = any || _chunkPeers.Remove(key);
            }

            return any;
        }

        public bool RemoveAllChunkPeer(FileChunk chunk)
        {
            var toRemove = _chunkPeers.Where(pair => pair.Key.Chunk == chunk.FileName)
                .Select(pair => pair.Key).ToList();

            var any = false;
            foreach (var key in toRemove)
            {
                any = true;
                _chunkPeers.Remove(key);
            }

            return any;
        }

        public bool RemoveAllChunkPeer(FileId fileId)
        {
            var toRemove = _chunkPeers.Where(pair => pair.Key.Chunk.StartsWith(fileId.ToString()))
                .Select(pair => pair.Key).ToList();

            var any = false;
            foreach (var key in toRemove)
            {
                any = true;
                _chunkPeers.Remove(key);
            }

            return any;
        }

        public bool RemoveAllChunkPeer(IPAddress ip)
        {
            var toRemove = _chunkPeers.Where(pair => pair.Key.IP == ip.GetHashCode())
                .Select(pair => pair.Key).ToList();

            var any = false;
            foreach (var key in toRemove)
            {
                any = true;
                _chunkPeers.Remove(key);
            }

            return any;
        }

        public IEnumerator<ChunkPeer> GetEnumerator()
        {
            return _chunkPeers.Select(pair => pair.Key).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)_chunkPeers).GetEnumerator();
        }

        protected virtual void Dispose(bool d)
        {
            if (d)
                _chunkPeers.Dispose();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }

    /*
    public class PersistentStore : IEnumerable<KeyValuePair<string, ReplicationDegrees>>, IDisposable
    {
        private readonly PersistentDictionary<string, ReplicationDegrees> _dict =
            new PersistentDictionary<string, ReplicationDegrees>("store", "repdegrees");

        public bool ContainsFile(string fileName)
        {
            return _dict.ContainsKey(fileName);
        }

        public int IncrementActualDegree(string fileName, int wantedDegree)
        {
            return IncrementActualDegree(fileName, 1, wantedDegree);
        }

        public int DecrementActualDegree(string fileName)
        {
            return IncrementActualDegree(fileName, -1, 1);
        }

        public void UpdateDegrees(string fileName, int actualDegree, int wantedDegree)
        {
            ReplicationDegrees t;
            if (_dict.TryGetValue(fileName, out t))
            {
                t.ActualDegree = actualDegree;
                t.WantedDegree = wantedDegree;
            }
            else
                _dict[fileName] = new ReplicationDegrees {ActualDegree = actualDegree, WantedDegree = wantedDegree};
        }

        public bool TryGetDegrees(string fileName, out ReplicationDegrees rd)
        {
            return _dict.TryGetValue(fileName, out rd);
        }

        public bool RemoveDegrees(string fileName)
        {
            return _dict.Remove(fileName);
        }

        private int IncrementActualDegree(string fileName, int add, int wantedDegree)
        {
            ReplicationDegrees t;
            if (_dict.TryGetValue(fileName, out t))
            {
                t.ActualDegree += add;
                if (t.ActualDegree < 0)
                    t.ActualDegree = 0;
                return t.ActualDegree;
            }

            var actualDegree = Math.Max(add, 0);
            _dict[fileName] = new ReplicationDegrees { ActualDegree = actualDegree, WantedDegree = wantedDegree };
            return actualDegree;
        }

        public IEnumerator<KeyValuePair<string, ReplicationDegrees>> GetEnumerator()
        {
            return _dict.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable) _dict).GetEnumerator();
        }

        protected virtual void Dispose(bool d)
        {
            if (d)
                _dict.Dispose();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }*/
}
