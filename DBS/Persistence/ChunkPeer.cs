using System;

namespace DBS.Persistence
{
    public class ChunkPeer
    {
        public string Chunk { get; set; } // FileId_ChunkNo
        public long IP { get; set; }

        public override bool Equals(Object obj)
        {
            var cp = obj as ChunkPeer;
            if ((object) cp == null)
                return false;

            return Equals(cp);
        }

        public bool Equals(ChunkPeer cp)
        {
            return Chunk == cp.Chunk && IP == cp.IP;
        }

        public override int GetHashCode()
        {
            return Chunk.GetHashCode() ^ IP.GetHashCode();
        }

        public static bool operator ==(ChunkPeer a, ChunkPeer b)
        {
            if (ReferenceEquals(a, b))
                return true;

            if (((object) a == null) || ((object) b == null))
                return false;

            return a.Equals(b);
        }

        public static bool operator !=(ChunkPeer a, ChunkPeer b)
        {
            return !(a == b);
        }
    }
}