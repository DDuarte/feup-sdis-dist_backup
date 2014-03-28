using System;
using System.IO;
using System.Text;
using DBS.Utilities;

namespace DBS.Messages.Enhancements
{
    class ACKMessage : Message // <Version> <FileId> <ChunkNo> <ChunkOwnerIP> <ChunkOwnerPort> <CRLF> <CRLF>
    {
        public ACKMessage(FileChunk fileChunk)
            : this(Core.Instance.Config.VersionM, Core.Instance.Config.VersionN,
            fileChunk.FileId, fileChunk.ChunkNo)
        { }

        public ACKMessage(int versionM, int versionN, FileId fileId, int chunkNo)
            : base(MessageType.ACK)
        {
            ValidateVersionPart(versionM);
            ValidateVersionPart(versionN);
            ValidateFileId(fileId);
            ValidateChunkNo(chunkNo);

            VersionM = versionM;
            VersionN = versionN;
            FileId = fileId;
            ChunkNo = chunkNo;
        }

        public int VersionM { get; private set; }
        public int VersionN { get; private set; }
        public FileId FileId { get; private set; }
        public int ChunkNo { get; private set; }

        public override string ToString()
        {
            return string.Format("{0} {1} {2}#{3}", MessageType, GetVersion(),
                FileId.ToStringSmall(), ChunkNo);
        }

        public string GetVersion()
        {
            return string.Format("{0}.{1}", VersionM, VersionN);
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
                if (fields.Length != 4)
                    return null;

                int versionM, versionN, chunkNo;
                FileId fileId;

                if (!ParseVersion(fields[1], out versionM, out versionN)) return null;
                if (!ParseFileId(fields[2], out fileId)) return null;
                if (!ParseInt(fields[3], out chunkNo)) return null;

                try
                {
                    return new ACKMessage(versionM, versionN, fileId, chunkNo);
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
