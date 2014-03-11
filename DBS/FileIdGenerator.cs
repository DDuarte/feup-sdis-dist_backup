using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace DBS
{
    public static class FileIdGenerator
    {
        public static IEnumerable<byte> Build(string fileName)
        {
            return Build(new SystemWrapper.IO.FileInfoWrap(new FileInfo(fileName)));
        }

        public static IEnumerable<byte> Build(SystemWrapper.IO.IFileInfoWrap file)
        {
            var sha = new SHA256Managed();

            var createDate = Encoding.ASCII.GetBytes(file.CreationTime.ToString(CultureInfo.InvariantCulture));
            var createDateChecksum = sha.ComputeHash(createDate);
            var modifyDate = Encoding.ASCII.GetBytes(file.LastWriteTime.ToString(CultureInfo.InvariantCulture));
            var modifyDateChecksum = sha.ComputeHash(modifyDate);
            var fileName = Encoding.ASCII.GetBytes(file.Name);
            var fileNameChecksum = sha.ComputeHash(fileName);

            byte[] fileChecksum;
            using (var fileStream = file.Open(FileMode.Open, FileAccess.Read))
            {
                using (var bufferedStream = new BufferedStream(fileStream.StreamInstance, 64*1000))
                {
                    fileChecksum = sha.ComputeHash(bufferedStream);
                }
            }

            var networkId = Encoding.ASCII.GetBytes(NetworkUtilities.GetNetworkId());
            var networkIdChecksum = sha.ComputeHash(networkId);

            var checksumSum = createDateChecksum.Concat(modifyDateChecksum).Concat(fileNameChecksum).Concat(fileChecksum).Concat(networkIdChecksum);
            var checksum = sha.ComputeHash(checksumSum.ToArray());
            return checksum;
        }

        public static string FileIdToString(IEnumerable<byte> bytes)
        {
            return bytes.Reverse().Aggregate("", (current, b) => current + b.ToString("X2"));
        }
    }
}
