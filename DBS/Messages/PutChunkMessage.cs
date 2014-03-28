using System;
using System.IO;
using System.Text;
using DBS.Utilities;

namespace DBS.Messages
{
    public class PutChunkMessage : Message // <Version> <FileId> <ChunkNo> <ReplicationDeg> <CRLF> <CRLF> <Body>
    {
        public PutChunkMessage(FileChunk fileChunk, int replicationDeg, byte[] body)
            : this(Core.Instance.Config.VersionM, Core.Instance.Config.VersionN,
            fileChunk.FileId, fileChunk.ChunkNo, replicationDeg, body)
        { }

        public PutChunkMessage(int versionM, int versionN, FileId fileId, int chunkNo,
            int replicationDeg, byte[] body)
            : base(MessageType.PutChunk)
        {
            ValidateVersionPart(versionM);
            ValidateVersionPart(versionN);
            ValidateFileId(fileId);
            ValidateChunkNo(chunkNo);
            ValidateReplicationDeg(replicationDeg);
            ValidateBody(body);

            VersionM = versionM;
            VersionN = versionN;
            FileId = fileId;
            ChunkNo = chunkNo;
            ReplicationDeg = replicationDeg;
            Body = body;
        }

        public int VersionM { get; private set; }
        public int VersionN { get; private set; }
        public FileId FileId { get; private set; }
        public int ChunkNo { get; private set; }
        public int ReplicationDeg { get; private set; }
        public byte[] Body { get; private set; }

        public override string ToString()
        {
            return string.Format("{0} {1} {2}#{3} {4} |{5}|", MessageType,
                GetVersion(), FileId.ToStringSmall(), ChunkNo, ReplicationDeg, Body.Length);
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
                stream.WriteASCII(' ');
                stream.WriteASCII(ReplicationDeg.ToString("D"));
                stream.WriteASCII("\r\n\r\n");
                stream.Write(Body);
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
                if (fields.Length != 5)
                    return null;

                int versionM, versionN, chunkNo, replicationDeg;
                FileId fileId;

                if (!ParseVersion(fields[1], out versionM, out versionN)) return null;
                if (!ParseFileId(fields[2], out fileId)) return null;
                if (!ParseInt(fields[3], out chunkNo)) return null;
                if (!ParseInt(fields[4], out replicationDeg)) return null;

                stream.Position = header.Length + 4; // 2x CRLF;
                var bodySize = stream.Length - stream.Position;
                var body = new byte[bodySize];
                if (bodySize != 0) // body can be 0 bytes if the size of the file to be sent is multiple of 64KB
                    stream.Read(body, 0, (int)bodySize);

                try
                {
                    return new PutChunkMessage(versionM, versionN, fileId, chunkNo, replicationDeg, body);
                }
                catch (Exception ex)
                {
                    Core.Instance.Log.Error("Could not create PutChunkMessage", ex);
                    return null;
                }
            }
        }
    }
}
