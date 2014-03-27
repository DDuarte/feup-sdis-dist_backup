using System;
using System.IO;
using System.Net;
using System.Text;
using DBS.Utilities;

namespace DBS.Messages.Enhancements
{
    class ConnInfoMessage : Message
    {
        public ConnInfoMessage(FileChunk fileChunk,int initiatorPort, IPAddress selectedPassivePeerIp)
            : this(Core.Instance.Config.VersionM, Core.Instance.Config.VersionN, fileChunk.FileId, fileChunk.ChunkNo, initiatorPort, selectedPassivePeerIp)
        {
            
        }

        public ConnInfoMessage(int versionM, int versionN, FileId fileId, int chunkNo, int initiatorPort, IPAddress selectedPassiveIp) 
        : base(MessageType.ConnInfo)
        {
            ValidateVersionPart(versionM);
            ValidateVersionPart(versionN);
            ValidateFileId(fileId);
            ValidateChunkNo(chunkNo);
            ValidatePort(initiatorPort);
            ValidateIP(selectedPassiveIp);

            VersionM = versionM;
            VersionN = versionN;
            FileId = fileId;
            ChunkNo = chunkNo;

            InitiatorPort = initiatorPort;
            PassiveIP = selectedPassiveIp;
        }

        public int VersionM { get; private set; }
        public int VersionN { get; private set; }
        public FileId FileId { get; private set; }
        public int ChunkNo { get; private set; }
        public int InitiatorPort { get; private set; }
        public IPAddress PassiveIP { get; private set; }
        public override string ToString()
        {
            return string.Format("{0} {1}#{2} {3}:{4} [{5}]", MessageType,
                FileId.ToStringSmall(), ChunkNo, RemoteEndPoint.Address, InitiatorPort, PassiveIP);
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
                stream.WriteASCII(InitiatorPort.ToString("D"));
                stream.WriteASCII(' ');
                stream.WriteASCII(PassiveIP.ToString());
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

                int versionM, versionN, chunkNo;
                FileId fileId;
                int port;
                IPAddress passiveIP;

                if (!ParseVersion(fields[1], out versionM, out versionN)) return null;
                if (!ParseFileId(fields[2], out fileId)) return null;
                if (!ParseInt(fields[3], out chunkNo)) return null;
                if (!ParseInt(fields[4], out port)) return null;
                if (!ParseIP(fields[5], out passiveIP)) return null;

                try
                {
                    return new ConnInfoMessage(versionM, versionN, fileId, chunkNo, port, passiveIP);
                }
                catch (Exception ex)
                {
                    Core.Instance.Log.Error("Could not create ConnInfoMessage", ex);
                    return null;
                }
            }
        }
    }
}
