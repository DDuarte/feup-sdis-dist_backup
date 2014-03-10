using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace DBS
{
    public enum MessageType
    {
        [StringValue("NONE")]
        None,
        // Chunk backup subprotocol
        [StringValue("PUTCHUNK")] // <Version> <FileId> <ChunkNo> <ReplicationDeg> <CRLF> <CRLF> <Body> 8 + 1 + 3 + 1 + 64 + 1 + 6 + 1 + 1 + 2 + 2 + 64000
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

        internal void SetMessageType(string type)
        {
            var mt = StringValueAttribute.Get<MessageType>(type);
            if (mt == MessageType.None)
                throw new ArgumentException("Invalid MessageType", "type");
            MessageType = mt;
        }

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

        internal void SetVersion(string version)
        {
            if (version.Length != 3)
                throw new ArgumentException("Invalid version string", "version");

            VersionM = int.Parse(version[0].ToString(CultureInfo.InvariantCulture));
            VersionN = int.Parse(version[2].ToString(CultureInfo.InvariantCulture));
        }

        private byte[] _fileId;
        public byte[] FileId
        {
            get { return _fileId; }
            set
            {
                if (value != null && value.Length != 32)
                    throw new ArgumentException("File identifier must be a 32 byte array", "value");
                _fileId = value;
            }
        }

        internal void SetFileId(string fileId)
        {
            if (fileId.Length != 64)
                throw new ArgumentException("Invalid fileId string", "fileId");

            FileId = new byte[32];
            for (int i = 0, index = 31; i < fileId.Length; i += 2, index--)
            {
                FileId[index] = Convert.ToByte(string.Format("{0}{1}", fileId[i], fileId[i + 1]), 16);
            }
        }

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

        internal void SetChunkNo(string chunkNo)
        {
            _chunkNo = int.Parse(chunkNo);
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

        internal void SetReplicationDeg(string replicationDeg)
        {
            ReplicationDeg = int.Parse(replicationDeg);
        }

        public byte[] Body { get; set; }

        public static Message Deserialize(byte[] data)
        {
            var message = new Message();

            using (var stream = new MemoryStream(data))
            {
                //using (var reader = new StreamReader(stream, Encoding.ASCII))
                var reader = new StreamReader(stream, Encoding.ASCII);
                var pos = 0L;
                {
                    var header = reader.ReadLine(); // until crlf
                    if (header != null)
                    {
                        pos += header.Length;
                        var fields = header.Split(' ');
                        var messageType = fields[0];
                        message.SetMessageType(messageType);
                        switch (message.MessageType)
                        {
                            case MessageType.PutChunk:
                            {
                                message.SetVersion(fields[1]);
                                message.SetFileId(fields[2]);
                                message.SetChunkNo(fields[3]);
                                message.SetReplicationDeg(fields[4]);
                                break;
                            }
                            case MessageType.Stored:
                            case MessageType.GetChunk:
                            case MessageType.Chunk:
                            case MessageType.Removed:
                            {
                                message.SetVersion(fields[1]);
                                message.SetFileId(fields[2]);
                                message.SetChunkNo(fields[3]);
                                break;
                            }
                            case MessageType.Delete:
                            {
                                message.SetFileId(fields[1]);
                                break;
                            }
                        }
                    }

                    pos += 4; // 2x CRLF
                }

                if (stream.Position != pos) // PUTCHUNK or CHUNK
                {
                    stream.Position = pos;
                    var bodySize = stream.Length - stream.Position;
                    message.Body = new byte[bodySize];
                    stream.Read(message.Body, 0, (int)bodySize);
                }
            }

            return message;
        }

        public byte[] Serialize()
        {
            // <MessageType> <Version> <FileId> <ChunkNo> <ReplicationDeg> <CRLF> <Body>
            using (var stream = new MemoryStream())
            {
                stream.Write(Encoding.ASCII.GetBytes(StringValueAttribute.Get(MessageType)));
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

                if (FileId != null)
                {
                    stream.WriteASCII(FileIdGenerator.FileIdToString(FileId));
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
                    return !(VersionM.HasValue || FileId != null || ChunkNo.HasValue || ReplicationDeg.HasValue);
                case "VersionM":
                    return !(FileId != null || ChunkNo.HasValue || ReplicationDeg.HasValue);
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

        public static Message BuildPutChunkMessage(int versionM, int versionN, byte[] fileId, int chunkNo,
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

        public static Message BuildStoredMessage(int versionM, int versionN, byte[] fileId, int chunkNo)
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

        public static Message BuildGetChunkMessage(int versionM, int versionN, byte[] fileId, int chunkNo)
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

        public static Message BuildChunkMessage(int versionM, int versionN, byte[] fileId, int chunkNo,
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

        public static Message BuildDeleteMessage(byte[] fileId)
        {
            return new Message
            {
                MessageType = MessageType.Delete,
                FileId = fileId
            };
        }

        public static Message BuildRemovedMessage(int versionM, int versionN, byte[] fileId, int chunkNo)
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
