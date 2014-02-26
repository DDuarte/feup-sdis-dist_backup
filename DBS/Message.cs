using System;

namespace DBS
{
    public enum MessageType
    {
        // Chunk backup subprotocol
        PutChunk, // PUTCHUNK <Version> <FileId> <ChunkNo> <ReplicationDeg> <CRLF> <CRLF> <Body>
        Stored, // STORED <Version> <FileId> <ChunkNo> <CRLF> <CRLF>

        // Chunk restore protocol
        GetChunk, // GETCHUNK <Version> <FileId> <ChunkNo> <CRLF> <CRLF>
        Chunk, // CHUNK <Version> <FileId> <ChunkNo> <CRLF> <CRLF> <Body>

        // File deletion subprotocol
        Delete, // DELETE <FileId> <CRLF> <CRLF>

        // Space reclaiming subprotocol
        Removed // REMOVED <Version> <FileId> <ChunkNo> <CRLF> <CRLF>
    }

    public struct Message
    {
        public MessageType Type { get; set; }

        private int? _versionN;
        public int? VersionN
        {
            get { return _versionN; }
            set
            {
                if (value.HasValue && (value < 0 || value > 9)) // 1 digit max
                    throw new ArgumentOutOfRangeException("value", value, "Version (N) must be between 0 and 9");
                _versionN = value;
            }
        }

        private int? _versionM;
        public int? VersionM
        {
            get { return _versionM; }
            set
            {
                if (value.HasValue && (value < 0 || value > 9)) // 1 digit max
                    throw new ArgumentOutOfRangeException("value", value, "Version (M) must be between 0 and 9");
                _versionM = value;
            }
        }

        public int? FileId { get; set; }

        private int? _chunkNo;
        public int? ChunkNo
        {
            get { return _chunkNo; }
            set
            {
                if (value.HasValue && (value < 0 || value > 999999)) // 6 digits max
                    throw new ArgumentOutOfRangeException("value", value, "Chunk number must be between 0 and 999999");
                _chunkNo = value;
            }
        }

        private int? _replicationDeg;
        public int? ReplicationDeg
        {
            get { return _replicationDeg; }
            set
            {
                if (value.HasValue && (value < 0 || value > 9)) // 1 digit max
                    throw new ArgumentOutOfRangeException("value", value, "Replication degree must be between 0 and 9");
                _replicationDeg = value;
            }
        }
    }
}
