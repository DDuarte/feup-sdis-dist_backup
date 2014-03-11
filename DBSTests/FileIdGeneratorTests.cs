using System;
using System.Linq;
using System.Text.RegularExpressions;
using SystemWrapper;
using DBS;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DBSTests
{
    [TestClass]
    public class FileIdGeneratorTests
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
            var hash1 = FileIdGenerator.Build(File1A).ToArray();
            var hash2 = FileIdGenerator.Build(File1B).ToArray();

            CollectionAssert.AreEqual(hash1, hash2);
        }

        [TestMethod]
        public void TestBuildDifferentHash()
        {
            var hash1 = FileIdGenerator.Build(File1A).ToArray();
            var hash2 = FileIdGenerator.Build(File1C).ToArray();
            var hash3 = FileIdGenerator.Build(File1D).ToArray();
            var hash4 = FileIdGenerator.Build(File1E).ToArray();
            var hash5 = FileIdGenerator.Build(File1F).ToArray();

            CollectionAssert.AreNotEqual(hash1, hash2); // diff data
            CollectionAssert.AreNotEqual(hash1, hash3); // diff creation time
            CollectionAssert.AreNotEqual(hash1, hash4); // diff modify time
            CollectionAssert.AreNotEqual(hash1, hash5); // diff file name
            CollectionAssert.AreNotEqual(hash2, hash3);
            CollectionAssert.AreNotEqual(hash2, hash4);
            CollectionAssert.AreNotEqual(hash2, hash5);
            CollectionAssert.AreNotEqual(hash3, hash4);
            CollectionAssert.AreNotEqual(hash3, hash5);
            CollectionAssert.AreNotEqual(hash4, hash5);
        }

        [TestMethod]
        public void TestFileIdToStringEquality()
        {
            var hash1 = FileIdGenerator.FileIdToString(FileIdGenerator.Build(File1A));
            var hash2 = FileIdGenerator.FileIdToString(FileIdGenerator.Build(File1B));
            var hash3 = FileIdGenerator.FileIdToString(FileIdGenerator.Build(File1C));
            var hash4 = FileIdGenerator.FileIdToString(FileIdGenerator.Build(File1D));
            var hash5 = FileIdGenerator.FileIdToString(FileIdGenerator.Build(File1E));
            var hash6 = FileIdGenerator.FileIdToString(FileIdGenerator.Build(File1F));

            Assert.AreEqual(hash1, hash2);
            Assert.AreNotEqual(hash1, hash3);
            Assert.AreNotEqual(hash1, hash4);
            Assert.AreNotEqual(hash1, hash5);
            Assert.AreNotEqual(hash1, hash6);
        }

        [TestMethod]
        public void TestFileIdToStringFormat()
        {
            var hashArr = FileIdGenerator.Build(File1A).ToArray();
            var hashStr = FileIdGenerator.FileIdToString(hashArr);

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
