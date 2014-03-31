using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using DBS.Utilities;

namespace DBS.Persistence
{
    public class PersistentChunkPeers : IEnumerable<ChunkPeer>, IDisposable
    {
        public ObservableCollection<ChunkPeer> ObservableCollection { get; private set; }

        private class Nothing // FIXME: we need this placeholder because PersistentHashSet was not implemented
        {                     // so we are using a dictionary from keys to this placeholder
// ReSharper disable once UnusedMember.Local
            public byte N { get; set; }
        }

        // FIXME: using two different dbs because PersistentDictionary is not thread safe per tables
        private readonly PersistentDictionary<ChunkPeer, Nothing> _chunkPeers; // ((FileId, ChunkNo), IP) -> nothing

        private readonly PersistentDictionary<string, int> _wantedRepDegs; // (FileId, ChunkNo) -> wanted rep deg

        public PersistentChunkPeers()
        {
            _chunkPeers = new PersistentDictionary<ChunkPeer, Nothing>("chunkpeer", "chunkpeer");
            _wantedRepDegs = new PersistentDictionary<string, int>("wanteddeg", "wanteddeg");
            ObservableCollection = new ObservableCollection<ChunkPeer>(_chunkPeers.Select(pair => pair.Key));
        }

        public int CountChunkPeer(FileChunk chunk)
        {
            return _chunkPeers.Count(chunkPeer => chunkPeer.Key.Chunk == chunk.FileName);
        }

        public int CountChunkPeer(string fileName)
        {
            return _chunkPeers.Count(chunkPeer => chunkPeer.Key.Chunk == fileName);
        }

        public bool HasChunkPeer(FileChunk chunk, bool any)
        {
            if (any)
                return _chunkPeers.Any(chunkPeer => chunkPeer.Key.Chunk == chunk.FileName);

            var localIps = NetworkUtilities.GetLocalIPAddresses();
            return localIps.Any(address =>
                _chunkPeers.Any(chunkPeer => chunkPeer.Key.Chunk == chunk.FileName &&
                                             chunkPeer.Key.IP == address.GetHashCode()));
        }

        public bool HasChunkPeer(FileChunk chunk, IPAddress ip)
        {
            return _chunkPeers.Any(chunkPeer => chunkPeer.Key.Chunk == chunk.FileName && chunkPeer.Key.IP == ip.GetHashCode());
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
            if (!HasChunkPeer(chunk, true))
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

            ObservableCollection.Add(chunkPeer);
        }

        public bool RemoveChunkPeer(FileChunk chunk, IPAddress ip)
        {
            var toRemove = _chunkPeers.Where(pair => pair.Key.Chunk == chunk.FileName && pair.Key.IP == ip.GetHashCode())
                .Select(pair => pair.Key).ToList();

            var any = false;
            foreach (var key in toRemove)
            {
                any = any || _chunkPeers.Remove(key);
                ObservableCollection.Remove(key);
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
                ObservableCollection.Remove(key);
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
            {
                ObservableCollection.Clear();
                _chunkPeers.Dispose();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
