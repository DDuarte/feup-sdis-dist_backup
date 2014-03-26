using System;
using System.IO;
using System.Text;
using DBS.Utilities;

namespace DBS.Messages.Enhancements
{
    class LookupMessage : Message // <Version> <FileId> <CRLF> <CRLF>
    {
        public LookupMessage(FileId fileId)
            : this(Core.Instance.Config.VersionM, Core.Instance.Config.VersionN, fileId)
        { }

        public LookupMessage(int versionM, int versionN, FileId fileId)
            : base(MessageType.Lookup)
        {
            ValidateVersionPart(versionM);
            ValidateVersionPart(versionN);
            ValidateFileId(fileId);

            VersionM = versionM;
            VersionN = versionN;
            FileId = fileId;
        }

        public int VersionM { get; private set; }
        public int VersionN { get; private set; }
        public FileId FileId { get; private set; }

        public override string ToString()
        {
            return string.Format("{0} {1}", MessageType, FileId.ToStringSmall());
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
                if (fields.Length != 3)
                    return null;

                int versionM, versionN;
                FileId fileId;

                if (!ParseVersion(fields[1], out versionM, out versionN)) return null;
                if (!ParseFileId(fields[2], out fileId)) return null;

                try
                {
                    return new LookupMessage(versionM, versionN, fileId);
                }
                catch (Exception ex)
                {
                    Core.Instance.Log.Error("Could not create LookupMessage", ex);
                    return null;
                }
            }
        }
    }
}
