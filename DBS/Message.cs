using System;
using System.IO;
using System.Text;

namespace DBS
{
    public enum MessageType
    {
        // Chunk backup subprotocol
        [StringValue("PUTCHUNK")] // <Version> <FileId> <ChunkNo> <ReplicationDeg> <CRLF> <CRLF> <Body>
        PutChunk,
        [StringValue("STORED")] // <Version> <FileId> <ChunkNo> <CRLF> <CRLF>
        Stored,

        // Chunk restore protocol
        [StringValue("GETCHUNK")] // <Version> <FileId> <ChunkNo> <CRLF> <CRLF>
        GetChunk,
        [StringValue("CHUNK")] // <Version> <FileId> <ChunkNo> <CRLF> <CRLF> <Body>
        Chunk,

        // File deletion subprotocol
        [StringValue("DELETE")] // <FileId> <CRLF> <CRLF>
        Delete,

        // Space reclaiming subprotocol
        [StringValue("REMOVED")] // <Version> <FileId> <ChunkNo> <CRLF> <CRLF>
        Removed
    }

    public struct Message
    {
        public MessageType MessageType { get; set; }

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

        public byte[] Body { get; set; }

        public byte[] Serialize()
        {
            // <MessageType> <Version> <FileId> <ChunkNo> <ReplicationDeg> <CRLF> <Body>
            using (var stream = new MemoryStream())
            {
                stream.Write(Encoding.ASCII.GetBytes(StringValueAttribute.GetStringValue(MessageType)));
                if (!IsLastField("MessageType"))
                    stream.WriteASCII(' ');

                if (VersionM.HasValue && VersionN.HasValue)
                {
                    stream.WriteASCII(VersionM.Value.ToString("D"));
                    stream.WriteASCII('.');
                    stream.WriteASCII(VersionN.Value.ToString("D"));
                    if (!IsLastField("VersionM"))
                        stream.WriteASCII(' ');
                }

                if (FileId.HasValue)
                {
                    stream.WriteASCII(FileId.Value.ToString("X").PadLeft(64, '0'));
                    if (!IsLastField("FileId"))
                        stream.WriteASCII(' ');
                }

                if (ChunkNo.HasValue)
                {
                    stream.WriteASCII(ChunkNo.Value.ToString("D"));
                    if (!IsLastField("ChunkNo"))
                        stream.WriteASCII(' ');
                }

                if (ReplicationDeg.HasValue)
                {
                    stream.WriteASCII(ReplicationDeg.Value.ToString("D"));
                    if (!IsLastField("ReplicationDeg"))
                        stream.WriteASCII(' ');
                }

                stream.WriteASCII("\r\n\r\n");

                if (Body != null)
                    stream.Write(Body);

                return stream.ToArray();
            }
        }

        private bool IsLastField(string field)
        {
            switch (field)
            {
                case "MessageType":
                    return !(VersionM.HasValue || FileId.HasValue || ChunkNo.HasValue || ReplicationDeg.HasValue);
                case "VersionM":
                    return !(FileId.HasValue || ChunkNo.HasValue || ReplicationDeg.HasValue);
                case "FileId":
                    return !(ChunkNo.HasValue || ReplicationDeg.HasValue);
                case "ChunkNo":
                    return !ReplicationDeg.HasValue;
                case "ReplicationDeg":
                    return true;
                default:
                    throw new ArgumentException("field needs to be one of MessageType, VersionM, FileId, ChunkNo or ReplicationDeg", "field");
            }
        }

        public static Message BuildPutChunkMessage(int versionM, int versionN, int fileId, int chunkNo,
            int replicationDeg, byte[] body)
        {
            return new Message
            {
                MessageType = MessageType.PutChunk,
                VersionM = versionM,
                VersionN = versionN,
                FileId = fileId,
                ChunkNo = chunkNo,
                ReplicationDeg = replicationDeg,
                Body = body
            };
        }

        public static Message BuildStoredMessage(int versionM, int versionN, int fileId, int chunkNo)
        {
            return new Message
            {
                MessageType = MessageType.Stored,
                VersionM = versionM,
                VersionN = versionN,
                FileId = fileId,
                ChunkNo = chunkNo
            };
        }

        public static Message BuildGetChunkMessage(int versionM, int versionN, int fileId, int chunkNo)
        {
            return new Message
            {
                MessageType = MessageType.GetChunk,
                VersionM = versionM,
                VersionN = versionN,
                FileId = fileId,
                ChunkNo = chunkNo
            };
        }

        public static Message BuildChunkMessage(int versionM, int versionN, int fileId, int chunkNo,
            byte[] body)
        {
            return new Message
            {
                MessageType = MessageType.Chunk,
                VersionM = versionM,
                VersionN = versionN,
                FileId = fileId,
                ChunkNo = chunkNo,
                Body = body
            };
        }

        public static Message BuildDeleteMessage(int fileId)
        {
            return new Message
            {
                MessageType = MessageType.Delete,
                FileId = fileId
            };
        }

        public static Message BuildRemovedMessage(int versionM, int versionN, int fileId, int chunkNo)
        {
            return new Message
            {
                MessageType = MessageType.Removed,
                VersionM = versionM,
                VersionN = versionN,
                FileId = fileId,
                ChunkNo = chunkNo
            };
        }
    }
}
