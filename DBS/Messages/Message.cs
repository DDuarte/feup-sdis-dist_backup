using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Text;
using DBS.Messages.Enhancements;
using DBS.Utilities;

namespace DBS.Messages
{
    public enum MessageType
    {
        [StringValue("NONE")] None,
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
        Removed,

        // File lookup subprotocol enhancement
        [StringValue("LOOKUP")] // <Version> <FileId> <CRLF> <CRLF>
        Lookup,

        // File lookup reply
        [StringValue("GOT")] // <Version> <FileId> <CRLF> <CRLF>
        Got,

        // Ack chunk message reply
        [StringValue("ACK")] // <Version> <FileId> <ChunkNo> <CRLF> <CRLF>
        ACK,

        // Connection Information message
        [StringValue("CONNINFO")] // <Version> <FileId> <ChunkNo> <InitiatorPeerIP> <InitiatorPeerPort> <SelectedPeerIP> <CRLF> <CRLF>
        ConnInfo
    }

    // 8 + 1 + 3 + 1 + 64 + 1 + 6 + 1 + 1 + 2 + 2 = 90 # max header size
    // 0x10000 (MaxUDPSize) - 90 = 65446 # max body size
    public abstract class Message
    {
        protected Message(MessageType messageType)
        {
            MessageType = messageType;
        }

        public MessageType MessageType { get; private set; }

        public IPEndPoint RemoteEndPoint { get; set; }

        public abstract byte[] Serialize();

        private static readonly Dictionary<MessageType, Func<byte[], Message>> _deserializeDict =
            new Dictionary<MessageType, Func<byte[], Message>>
            {
                {MessageType.PutChunk, PutChunkMessage.Deserialize},
                {MessageType.Stored, StoredMessage.Deserialize},
                {MessageType.GetChunk, GetChunkMessage.Deserialize},
                {MessageType.Chunk, ChunkMessage.Deserialize},
                {MessageType.Delete, DeleteMessage.Deserialize},
                {MessageType.Removed, RemovedMessage.Deserialize},
                {MessageType.Lookup, LookupMessage.Deserialize},
                {MessageType.Got, GotMessage.Deserialize},
                {MessageType.ACK, ACKMessage.Deserialize},
                {MessageType.ConnInfo, ConnInfoMessage.Deserialize}
            };

        public static Message Deserialize(byte[] data)
        {
            if (data == null)
                return null;

            var minLength = Math.Min(15, data.Length); // assuming max type length is 15
            string typeStr = null;
            for (var i = 0; i < minLength; ++i)
            {
                if (data[i] == ' ')
                {
                    typeStr = Encoding.ASCII.GetString(data, 0, i);
                    break;
                }
            }

            if (string.IsNullOrWhiteSpace(typeStr))
                return null;

            var type = StringValueAttribute.Get<MessageType>(typeStr);
            if (type == MessageType.None)
                return null;

            Func<byte[], Message> deserializeMethod;
            if (_deserializeDict.TryGetValue(type, out deserializeMethod))
                return deserializeMethod(data);
            return null;
        }

        protected static bool ParseVersion(string versionStr, out int m, out int n) // expected format: "M.N"
        {
            if (versionStr.Length != 3)
            {
                m = 0;
                n = 0;
                return false;
            }

            if (!int.TryParse(versionStr[0].ToString(CultureInfo.InvariantCulture), out m))
            {
                n = 0;
                return false;
            }

            return int.TryParse(versionStr[2].ToString(CultureInfo.InvariantCulture), out n);
        }

        protected static bool ParseFileId(string fileIdStr, out FileId fileId)
        {
            try
            {
                fileId = new FileId(fileIdStr);
                return true;
            }
            catch (Exception)
            {
                fileId = null;
                return false;
            }
        }

        protected static bool ParseInt(string valueStr, out int value)
        {
            return int.TryParse(valueStr, out value);
        }

        protected static bool ParseIP(string ipStr, out IPAddress ip)
        {
            return IPAddress.TryParse(ipStr, out ip);
        }

        protected static void ValidateVersionPart(int v)
        {
            if (v < 0 || v > 9) // 1 digit max
                throw new ArgumentOutOfRangeException("v", v, "Version part must be between 0 and 9");
        }

        protected static void ValidateChunkNo(int v)
        {
            if (v < 0 || v > 999999) // 6 digits max
                throw new ArgumentOutOfRangeException("v", v, "Chunk number must be between 0 and 999999");
        }

        protected static void ValidateReplicationDeg(int v)
        {
            if (v < 0 || v > 9) // 1 digit max
                throw new ArgumentOutOfRangeException("v", v, "Replication degree must be between 0 and 9");
        }

        protected static void ValidateFileId(FileId fileId)
        {
            if (fileId == null)
                throw new ArgumentNullException("fileId");
        }

        protected static void ValidateBody(IEnumerable<byte> body)
        {
            if (body == null)
                throw new ArgumentNullException("body");
        }

        protected static void ValidateIP(IPAddress ip)
        {
            if (ip == null)
                throw new ArgumentNullException("ip");
        }

        protected static void ValidatePort(int port)
        {
            if (port < 0 || port > 65535) // 0x0 - 0xFFFF
                throw new ArgumentOutOfRangeException("port", port, "Port number must be between 0 and 65535");
        }
    }
}
