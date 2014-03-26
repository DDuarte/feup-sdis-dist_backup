using System;
using System.IO;
using System.Net;
using System.Text;
using DBS.Utilities;

namespace DBS.Messages.Enhancements
{
    class ACKMessage : Message // <Version> <FileId> <ChunkNo> <ChunkOwnerIP> <ChunkOwnerPort> <CRLF> <CRLF>
    {
        public ACKMessage(FileChunk fileChunk, IPAddress chunkOwnerIp, int chunkOwnerPort)
            : this(Core.Instance.Config.VersionM, Core.Instance.Config.VersionN,
            fileChunk.FileId, fileChunk.ChunkNo, chunkOwnerIp, chunkOwnerPort)
        { }

        public ACKMessage(int versionM, int versionN, FileId fileId, int chunkNo,
            IPAddress chunkOwnerIp, int chunkOwnerPort)
            : base(MessageType.ACK)
        {
            ValidateVersionPart(versionM);
            ValidateVersionPart(versionN);
            ValidateFileId(fileId);
            ValidateChunkNo(chunkNo);
            ValidateIP(chunkOwnerIp);
            ValidatePort(chunkOwnerPort);

            VersionM = versionM;
            VersionN = versionN;
            FileId = fileId;
            ChunkNo = chunkNo;
            ChunkOwnerIP = chunkOwnerIp;
            ChunkOwnerPort = chunkOwnerPort;
        }

        public int VersionM { get; private set; }
        public int VersionN { get; private set; }
        public FileId FileId { get; private set; }
        public int ChunkNo { get; private set; }
        public IPAddress ChunkOwnerIP { get; private set; }
        public int ChunkOwnerPort { get; private set; }

        public override string ToString()
        {
            return string.Format("{0} {1}#{2} {3}:{4}", MessageType,
                FileId.ToStringSmall(), ChunkNo, ChunkOwnerIP, ChunkOwnerPort);
        }

        public override byte[] Serialize()
        {
            using (var stream = new MemoryStream())
            {
                stream.WriteASCII(StringValueAttribute.Get(MessageType));
                stream.WriteASCII(' ');
                stream.WriteASCII(VersionM.ToString("D"));
                stream.WriteASCII('.');
                stream.WriteASCII(VersionN.ToString("D"));
                stream.WriteASCII(' ');
                stream.WriteASCII(FileId.ToString());
                stream.WriteASCII(' ');
                stream.WriteASCII(ChunkNo.ToString("D"));
                stream.WriteASCII(' ');
                stream.WriteASCII(ChunkOwnerIP.ToString());
                stream.WriteASCII(' ');
                stream.WriteASCII(ChunkOwnerPort.ToString("D"));
                stream.WriteASCII("\r\n\r\n");
                return stream.ToArray();
            }
        }

        public new static Message Deserialize(byte[] data)
        {
            using (var stream = new MemoryStream(data))
            {
                var reader = new StreamReader(stream, Encoding.ASCII);
                var header = reader.ReadLine(); // until crlf
                if (header == null) return null;
                var fields = header.Split(' '); // fields[0] is type
                if (fields.Length != 6)
                    return null;

                int versionM, versionN, chunkNo, port;
                FileId fileId;
                IPAddress ip;

                if (!ParseVersion(fields[1], out versionM, out versionN)) return null;
                if (!ParseFileId(fields[2], out fileId)) return null;
                if (!ParseInt(fields[3], out chunkNo)) return null;
                if (!ParseIP(fields[4], out ip)) return null;
                if (!ParseInt(fields[5], out port)) return null;

                try
                {
                    return new ACKMessage(versionM, versionN, fileId, chunkNo, ip, port);
                }
                catch (Exception ex)
                {
                    Core.Instance.Log.Error("Could not create ACKMessage", ex);
                    return null;
                }
            }
        }
    }
}
