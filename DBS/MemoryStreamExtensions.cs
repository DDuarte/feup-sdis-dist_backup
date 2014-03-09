using System.IO;
using System.Text;

namespace DBS
{
    public static class MemoryStreamExtensions
    {
        public static void Write(this MemoryStream stream, byte[] bytes)
        {
            stream.Write(bytes, 0, bytes.Length);
        }

        public static void WriteASCII(this MemoryStream stream, char c)
        {
            stream.WriteByte((byte) c);
            // Masochist way: stream.WriteByte(Encoding.ASCII.GetBytes(c.ToString(CultureInfo.InvariantCulture))[0]);
        }

        public static void WriteASCII(this MemoryStream stream, string str)
        {
            stream.Write(Encoding.ASCII.GetBytes(str));
        }
    }
}
