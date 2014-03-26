using System;
using System.IO;
using System.Text;
using DBS.Utilities;

namespace DBS.Messages
{
    public class DeleteMessage : Message // <FileId> <CRLF> <CRLF>
    {
        public DeleteMessage(FileId fileId)
            : base(MessageType.Delete)
        {
            ValidateFileId(fileId);

            FileId = fileId;
        }

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
                if (fields.Length != 2)
                    return null;

                FileId fileId;

                if (!ParseFileId(fields[1], out fileId)) return null;

                try
                {
                    return new DeleteMessage(fileId);
                }
                catch (Exception ex)
                {
                    Core.Instance.Log.Error("Could not create DeleteMessage", ex);
                    return null;
                }
            }
        }
    }
}
