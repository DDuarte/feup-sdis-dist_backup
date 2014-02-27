using System;
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
        public static IEnumerable<byte> Build(FileInfo file)
        {
            var sha = new SHA256Managed();

            var createDate = Encoding.ASCII.GetBytes(file.CreationTime.ToString(CultureInfo.InvariantCulture));
            var createDateChecksum = sha.ComputeHash(createDate);
            var modifyDate = Encoding.ASCII.GetBytes(file.LastWriteTime.ToString(CultureInfo.InvariantCulture));
            var modifyDateChecksum = sha.ComputeHash(modifyDate);
            var fileName = Encoding.ASCII.GetBytes(file.Name.ToCharArray());
            var fileNameChecksum = sha.ComputeHash(fileName);

            byte[] fileChecksum = null;
            using (var fileStream = file.Open(FileMode.Open, FileAccess.Read))
            {
                using (var bufferedStream = new BufferedStream(fileStream, 64*1000))
                {
                    fileChecksum = sha.ComputeHash(bufferedStream);
                }
            }

            var checksumSum = createDateChecksum.Concat(modifyDateChecksum).Concat(fileNameChecksum).Concat(fileChecksum);
            var checksum = sha.ComputeHash(checksumSum.ToArray());
            return checksum;
        }
    }
}
