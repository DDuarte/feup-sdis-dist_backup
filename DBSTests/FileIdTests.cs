using System;
using System.Text.RegularExpressions;
using SystemWrapper;
using DBS;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DBSTests
{
    [TestClass]
    public class FileIdTests
    {
        /* This is the file identifier for the backup service.
         * As stated above, it is supposed to be obtained by using
         * the SHA256 cryptographic hash function. As its name indicates
         * its length is 256 bit, i.e. 32 bytes, and should be encoded
         * as a 64 ASCII character sequence. The encoding is as follows:
         * each byte of the hash value is encoded by the two ASCII characters
         * corresponding to the hexadecimal representation of that byte. E.g.,
         * a byte with value 0xB2 should be represented by the two char sequence
         * 'B''2' (or 'b''2', it does not matter). The entire hash is represented
         * in big-endian order, i.e. from the MSB (byte 31) to the LSB (byte 0). */

        private static readonly MyFileInfo File1A = new MyFileInfo(new DateTimeWrap(DateTime.Now), new DateTimeWrap(DateTime.Now), "File1", new byte[] {1, 2, 3});
        private static readonly MyFileInfo File1B = new MyFileInfo(new DateTimeWrap(DateTime.Now), new DateTimeWrap(DateTime.Now), "File1", new byte[] {1, 2, 3});
        private static readonly MyFileInfo File1C = new MyFileInfo(new DateTimeWrap(DateTime.Now), new DateTimeWrap(DateTime.Now), "File1", new byte[] {3, 2, 1});
        private static readonly MyFileInfo File1D = new MyFileInfo(new DateTimeWrap(DateTime.Now).AddSeconds(1.0), new DateTimeWrap(DateTime.Now), "File1", new byte[] {1, 2, 3});
        private static readonly MyFileInfo File1E = new MyFileInfo(new DateTimeWrap(DateTime.Now), new DateTimeWrap(DateTime.Now).AddSeconds(1.0), "File1", new byte[] { 1, 2, 3 });
        private static readonly MyFileInfo File1F = new MyFileInfo(new DateTimeWrap(DateTime.Now), new DateTimeWrap(DateTime.Now), "File2", new byte[] { 1, 2, 3 });

        [TestMethod]
        public void TestBuildEqualHash()
        {
            // File1a and File1b have same attributes
            var fileId1 = FileId.FromFile(File1A);
            var fileId2 = FileId.FromFile(File1B);

            Assert.AreEqual(fileId1, fileId2);
        }

        [TestMethod]
        public void TestBuildDifferentHash()
        {
            var fileId1 = FileId.FromFile(File1A);
            var fileId2 = FileId.FromFile(File1C);
            var fileId3 = FileId.FromFile(File1D);
            var fileId4 = FileId.FromFile(File1E);
            var fileId5 = FileId.FromFile(File1F);

            Assert.AreNotEqual(fileId1, fileId2); // diff data
            Assert.AreNotEqual(fileId1, fileId3); // diff creation time
            Assert.AreNotEqual(fileId1, fileId4); // diff modify time
            Assert.AreNotEqual(fileId1, fileId5); // diff file name
            Assert.AreNotEqual(fileId2, fileId3);
            Assert.AreNotEqual(fileId2, fileId4);
            Assert.AreNotEqual(fileId2, fileId5);
            Assert.AreNotEqual(fileId3, fileId4);
            Assert.AreNotEqual(fileId3, fileId5);
            Assert.AreNotEqual(fileId4, fileId5);
        }

        [TestMethod]
        public void TestFileIdToStringEquality()
        {
            var hash1 = FileId.FromFile(File1A).ToString();
            var hash2 = FileId.FromFile(File1B).ToString();
            var hash3 = FileId.FromFile(File1C).ToString();
            var hash4 = FileId.FromFile(File1D).ToString();
            var hash5 = FileId.FromFile(File1E).ToString();
            var hash6 = FileId.FromFile(File1F).ToString();

            Assert.AreEqual(hash1, hash2);
            Assert.AreNotEqual(hash1, hash3);
            Assert.AreNotEqual(hash1, hash4);
            Assert.AreNotEqual(hash1, hash5);
            Assert.AreNotEqual(hash1, hash6);
        }

        [TestMethod]
        public void TestFileIdToStringFormat()
        {
            var fileId = FileId.FromFile(File1A);
            var hashArr = fileId.GetBytes();
            var hashStr = fileId.ToString();

            // sizes
            Assert.AreEqual(32, hashArr.Length);
            Assert.AreEqual(64, hashStr.Length);

            // format
            var regex = new Regex(@"[A-Fa-f0-9]+"); // hex values
            Assert.IsTrue(regex.IsMatch(hashStr));

            // big endian
            var startStr = string.Format("{0}{1}", hashStr[0], hashStr[1]);
            var endArr = hashArr[31].ToString("X2");
            Assert.AreEqual(endArr, startStr);

            var endStr = string.Format("{0}{1}", hashStr[62], hashStr[63]);
            var startArr = hashArr[0].ToString("X2");
            Assert.AreEqual(startArr, endStr);
        }
    }
}
