using System;
using DBS;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DBSTests
{
    [TestClass]
    public class MessageTests
    {
        private static readonly byte[] FileId1 = { 1, 2, 3, 4, 5, 6, 7, 8,
                                                   9, 10, 11, 12, 13, 14, 15, 16,
                                                  17, 18, 19, 20, 21, 22, 23, 24,
                                                  25, 26, 27, 28, 29, 30, 31, 32 };

        private static readonly byte[] Body1 = {1, 2, 3};

        private static readonly byte[] FileId2 = { 11, 189, 180, 206, 8, 14, 253, 168,
                                                  232, 87, 78, 191, 115, 33, 47, 11,
                                                  165, 129, 245, 225, 22, 146, 233, 6,
                                                  108, 212, 233, 31, 99, 150, 106, 201 };

        private const string FileIdStr2 = "0BBDB4CE080EFDA8E8574EBF73212F0BA581F5E11692E9066CD4E91F63966AC9";

        [TestMethod]
        public void TestBuildPutChunkMessage()
        {
            var msg = Message.BuildPutChunkMessage(1, 0, FileId1, 50, 5, Body1);

            Assert.AreEqual(MessageType.PutChunk, msg.MessageType);
            Assert.AreEqual(1, msg.VersionM);
            Assert.AreEqual(0, msg.VersionN);
            CollectionAssert.AreEqual(FileId1, msg.FileId);
            Assert.AreEqual(50, msg.ChunkNo);
            Assert.AreEqual(5, msg.ReplicationDeg);
            Assert.AreEqual(Body1, msg.Body);
        }

        [TestMethod]
        public void TestSetters()
        {
            var msg = new Message();
            msg.SetMessageType("PUTCHUNK");
            msg.SetVersion("5.7");
            msg.SetFileId(FileIdStr2);
            msg.SetChunkNo("100");
            msg.SetReplicationDeg("2");
            
            Assert.AreEqual(MessageType.PutChunk, msg.MessageType);
            Assert.AreEqual(5, msg.VersionM);
            Assert.AreEqual(7, msg.VersionN);
            CollectionAssert.AreEqual(FileId2, msg.FileId);
            Assert.AreEqual(100, msg.ChunkNo);
            Assert.AreEqual(2, msg.ReplicationDeg);
            Assert.AreEqual(null, msg.Body);
        }

        [TestMethod]
        public void TestSerializeDeserialize()
        {
            var body = new byte[255];
            for (var i = 0; i < body.Length; ++i)
                body[i] = (byte)i;

            var msg = Message.BuildPutChunkMessage(5, 7, FileId1, 100, 2, body);

            var bytes = msg.Serialize();
            var msg2 = Message.Deserialize(bytes);

            Assert.AreEqual(msg.MessageType, msg2.MessageType);
            Assert.AreEqual(msg.VersionM, msg2.VersionM);
            Assert.AreEqual(msg.VersionN, msg2.VersionN);
            CollectionAssert.AreEqual(msg.FileId, msg2.FileId);
            Assert.AreEqual(msg.ChunkNo, msg2.ChunkNo);
            Assert.AreEqual(msg.ReplicationDeg, msg2.ReplicationDeg);
            CollectionAssert.AreEqual(msg.Body, msg2.Body);
        }

        [TestMethod]
        public void TestSerializeDeserialize2()
        {
            var msg = Message.BuildDeleteMessage(FileId1);

            var bytes = msg.Serialize();
            var msg2 = Message.Deserialize(bytes);

            Assert.AreEqual(msg.MessageType, msg2.MessageType);
            Assert.AreEqual(msg.VersionM, msg2.VersionM);
            Assert.AreEqual(msg.VersionN, msg2.VersionN);
            CollectionAssert.AreEqual(msg.FileId, msg2.FileId);
            Assert.AreEqual(msg.ChunkNo, msg2.ChunkNo);
            Assert.AreEqual(msg.ReplicationDeg, msg2.ReplicationDeg);
            CollectionAssert.AreEqual(msg.Body, msg2.Body);
        }
    }
}
