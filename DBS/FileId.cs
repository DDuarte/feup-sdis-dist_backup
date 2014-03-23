using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using DBS.Utilities;

namespace DBS
{
    public class FileId
    {
        public static FileId FromFile(string fileName)
        {
            return new FileId(Build(fileName).ToArray());
        }

        public static FileId FromFile(SystemWrapper.IO.IFileInfoWrap file)
        {
            return new FileId(Build(file).ToArray());
        }

        public FileId(string fileIdStr)
        {
            if (fileIdStr.Length != 64)
                throw new ArgumentException("Invalid fileId string", "fileIdStr");

            _bytes = new byte[32];
            for (int i = 0, index = 31; i < fileIdStr.Length; i += 2, index--)
                _bytes[index] = Convert.ToByte(string.Format("{0}{1}", fileIdStr[i], fileIdStr[i + 1]), 16);
        }

        public FileId(byte[] fileId)
        {
            _bytes = fileId;
        }

        private readonly byte[] _bytes;
        public byte[] GetBytes()
        {
            return _bytes;
        }

        private string _fileIdStr; // cached string, it is assumed that _bytes is immutable
        public override string ToString()
        {
            if (_fileIdStr == null)
                _fileIdStr = _bytes.Reverse().Aggregate(string.Empty, (current, b) => current + b.ToString("X2"));
            return _fileIdStr;
        }

        public string ToStringSmall()
        {
            return ToString().Substring(0, 4) + "..";
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            var fileId = obj as FileId;
            if (fileId == null)
                return false;

            return Equals(fileId);
        }

        public bool Equals(FileId fileId)
        {
            if (fileId == null)
                return false;

            if (_bytes == null && fileId._bytes == null)
                return true;
            if (_bytes == null || fileId._bytes == null)
                return false;
            if (_bytes.Length != fileId._bytes.Length)
                return false;
// ReSharper disable once LoopCanBeConvertedToQuery
            for (var i = 0; i < _bytes.Length; ++i)
                if (_bytes[i] != fileId._bytes[i])
                    return false;

            return true;
        }

        public override int GetHashCode()
        {
            if (_bytes == null || _bytes.Length == 0)
                return 0;
            var hashCode = 0;
// ReSharper disable once LoopCanBeConvertedToQuery
// ReSharper disable once ForCanBeConvertedToForeach
            for (var i = 0; i < _bytes.Length; i++)
                // Rotate by 3 bits and XOR the new value.
                hashCode = (hashCode << 3) | (hashCode >> (29)) ^ _bytes[i];
            return hashCode;
        }

        public static bool operator ==(FileId a, FileId b)
        {
            if (ReferenceEquals(a, b))
                return true;

            if ((object)a == null || (object)b == null)
                return false;

            return a.Equals(b);
        }

        public static bool operator !=(FileId a, FileId b)
        {
            return !(a == b);
        }

        private static IEnumerable<byte> Build(string fileName)
        {
            return Build(new SystemWrapper.IO.FileInfoWrap(new FileInfo(fileName)));
        }

        private static IEnumerable<byte> Build(SystemWrapper.IO.IFileInfoWrap file)
        {
            var sha = new SHA256Managed();

            var createDate = Encoding.ASCII.GetBytes(file.CreationTime.ToString(CultureInfo.InvariantCulture));
            var createDateChecksum = sha.ComputeHash(createDate);
            var modifyDate = Encoding.ASCII.GetBytes(file.LastWriteTime.ToString(CultureInfo.InvariantCulture));
            var modifyDateChecksum = sha.ComputeHash(modifyDate);
            var fileName = Encoding.ASCII.GetBytes(file.Name);
            var fileNameChecksum = sha.ComputeHash(fileName);

            // TODO: Verifiy if including file data (expensive operation) is necessary
            byte[] fileChecksum;
            using (var fileStream = file.Open(FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                using (var bufferedStream = new BufferedStream(fileStream.StreamInstance, 64 * 1000))
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
    }
}
