using System;
using DBS;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DBSTests
{
    [TestClass]
    public class MessageTests
    {
        private static readonly byte[] FileId1Bytes = { 1, 2, 3, 4, 5, 6, 7, 8,
                                                        9, 10, 11, 12, 13, 14, 15, 16,
                                                       17, 18, 19, 20, 21, 22, 23, 24,
                                                       25, 26, 27, 28, 29, 30, 31, 32 };

        private static readonly FileId FileId1 = new FileId(FileId1Bytes);

        private static readonly byte[] Body1 = {1, 2, 3};

        private static readonly byte[] FileId2Bytes = { 11, 189, 180, 206, 8, 14, 253, 168,
                                                       232, 87, 78, 191, 115, 33, 47, 11,
                                                       165, 129, 245, 225, 22, 146, 233, 6,
                                                       108, 212, 233, 31, 99, 150, 106, 201 };

        private static readonly FileId FileId2 = new FileId(FileId2Bytes);

        private const string FileIdStr2 = "C96A96631FE9D46C06E99216E1F581A50B2F2173BF4E57E8A8FD0E08CEB4BD0B";

        [TestMethod]
        public void TestBuildPutChunkMessage()
        {
            var msg = Message.BuildPutChunkMessage(1, 0, FileId1, 50, 5, Body1);

            Assert.AreEqual(MessageType.PutChunk, msg.MessageType);
            Assert.AreEqual(1, msg.VersionM);
            Assert.AreEqual(0, msg.VersionN);
            Assert.AreEqual(FileId1, msg.FileId);
            Assert.AreEqual(50, msg.ChunkNo);
            Assert.AreEqual(5, msg.ReplicationDeg);
            CollectionAssert.AreEqual(Body1, msg.Body);
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
            Assert.AreEqual(FileId2, msg.FileId);
            Assert.AreEqual(100, msg.ChunkNo);
            Assert.AreEqual(2, msg.ReplicationDeg);
            CollectionAssert.AreEqual(null, msg.Body);
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
            Assert.AreEqual(msg.FileId, msg2.FileId);
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
            Assert.AreEqual(msg.FileId, msg2.FileId);
            Assert.AreEqual(msg.ChunkNo, msg2.ChunkNo);
            Assert.AreEqual(msg.ReplicationDeg, msg2.ReplicationDeg);
            CollectionAssert.AreEqual(msg.Body, msg2.Body);
        }

        [TestMethod]
        public void TestMessageFieldUpperBounds()
        {
            var body = new byte[255];
            for (var i = 0; i < body.Length; ++i)
                    body[i] = (byte)i;

            var getChunkMsg = Message.BuildGetChunkMessage(9, 9, FileId1, 999999);
            var bytes = getChunkMsg.Serialize();
            var getChunkMsg2 = Message.Deserialize(bytes);
            Assert.AreEqual(getChunkMsg.MessageType, getChunkMsg2.MessageType);
            Assert.AreEqual(getChunkMsg.VersionM, getChunkMsg2.VersionM);
            Assert.AreEqual(getChunkMsg.VersionN, getChunkMsg2.VersionN);
            Assert.AreEqual(getChunkMsg.FileId, getChunkMsg2.FileId);
            Assert.AreEqual(getChunkMsg.ChunkNo, getChunkMsg2.ChunkNo);
            Assert.AreEqual(getChunkMsg.ReplicationDeg, getChunkMsg2.ReplicationDeg);
            CollectionAssert.AreEqual(getChunkMsg.Body, getChunkMsg2.Body);

            var chunkMsg = Message.BuildChunkMessage(9, 9, FileId1, 999999, body);
            bytes = chunkMsg.Serialize();
            var chunkMsg2 = Message.Deserialize(bytes);
            Assert.AreEqual(chunkMsg.MessageType, chunkMsg2.MessageType);
            Assert.AreEqual(chunkMsg.VersionM, chunkMsg2.VersionM);
            Assert.AreEqual(chunkMsg.VersionN, chunkMsg2.VersionN);
            Assert.AreEqual(chunkMsg.FileId, chunkMsg2.FileId);
            Assert.AreEqual(chunkMsg.ChunkNo, chunkMsg2.ChunkNo);
            Assert.AreEqual(chunkMsg.ReplicationDeg, chunkMsg2.ReplicationDeg);
            CollectionAssert.AreEqual(chunkMsg.Body, chunkMsg2.Body);

            var putChunkMsg = Message.BuildPutChunkMessage(9, 9, FileId1, 999999, 9, body);
            bytes = putChunkMsg.Serialize();
            var putChunkMsg2 = Message.Deserialize(bytes);
            Assert.AreEqual(putChunkMsg.MessageType, putChunkMsg2.MessageType);
            Assert.AreEqual(putChunkMsg.VersionM, putChunkMsg2.VersionM);
            Assert.AreEqual(putChunkMsg.VersionN, putChunkMsg2.VersionN);
            Assert.AreEqual(putChunkMsg.FileId, putChunkMsg2.FileId);
            Assert.AreEqual(putChunkMsg.ChunkNo, putChunkMsg2.ChunkNo);
            Assert.AreEqual(putChunkMsg.ReplicationDeg, putChunkMsg2.ReplicationDeg);
            CollectionAssert.AreEqual(putChunkMsg.Body, putChunkMsg2.Body);

            var storedMsg = Message.BuildStoredMessage(9, 9, FileId1, 999999);
            bytes = storedMsg.Serialize();
            var storedMsg2 = Message.Deserialize(bytes);
            Assert.AreEqual(storedMsg.MessageType, storedMsg2.MessageType);
            Assert.AreEqual(storedMsg.VersionM, storedMsg2.VersionM);
            Assert.AreEqual(storedMsg.VersionN, storedMsg2.VersionN);
            Assert.AreEqual(storedMsg.FileId, storedMsg2.FileId);
            Assert.AreEqual(storedMsg.ChunkNo, storedMsg2.ChunkNo);
            Assert.AreEqual(storedMsg.ReplicationDeg, storedMsg2.ReplicationDeg);
            CollectionAssert.AreEqual(storedMsg.Body, storedMsg2.Body);

            var deleteMsg = Message.BuildDeleteMessage(FileId1);
            bytes = deleteMsg.Serialize();
            var deleteMsg2 = Message.Deserialize(bytes);
            Assert.AreEqual(deleteMsg.MessageType, deleteMsg2.MessageType);
            Assert.AreEqual(deleteMsg.VersionM, deleteMsg2.VersionM);
            Assert.AreEqual(deleteMsg.VersionN, deleteMsg2.VersionN);
            Assert.AreEqual(deleteMsg.FileId, deleteMsg2.FileId);
            Assert.AreEqual(deleteMsg.ChunkNo, deleteMsg2.ChunkNo);
            Assert.AreEqual(deleteMsg.ReplicationDeg, deleteMsg2.ReplicationDeg);
            CollectionAssert.AreEqual(deleteMsg.Body, deleteMsg2.Body);

            var removedMsg = Message.BuildRemovedMessage(9, 9, FileId1, 999999);
            bytes = removedMsg.Serialize();
            var removedMsg2 = Message.Deserialize(bytes);
            Assert.AreEqual(removedMsg.MessageType, removedMsg2.MessageType);
            Assert.AreEqual(removedMsg.VersionM, removedMsg2.VersionM);
            Assert.AreEqual(removedMsg.VersionN, removedMsg2.VersionN);
            Assert.AreEqual(removedMsg.FileId, removedMsg2.FileId);
            Assert.AreEqual(removedMsg.ChunkNo, removedMsg2.ChunkNo);
            Assert.AreEqual(removedMsg.ReplicationDeg, removedMsg2.ReplicationDeg);
            CollectionAssert.AreEqual(removedMsg.Body, removedMsg2.Body);
        }

        [TestMethod]
        public void TestMessageFieldLowerBounds()
        {
            var body = new byte[255];
            for (var i = 0; i < body.Length; ++i)
                body[i] = (byte)i;

            var getChunkMsg = Message.BuildGetChunkMessage(0, 0, FileId1, 0);
            var bytes = getChunkMsg.Serialize();
            var getChunkMsg2 = Message.Deserialize(bytes);
            Assert.AreEqual(getChunkMsg.MessageType, getChunkMsg2.MessageType);
            Assert.AreEqual(getChunkMsg.VersionM, getChunkMsg2.VersionM);
            Assert.AreEqual(getChunkMsg.VersionN, getChunkMsg2.VersionN);
            Assert.AreEqual(getChunkMsg.FileId, getChunkMsg2.FileId);
            Assert.AreEqual(getChunkMsg.ChunkNo, getChunkMsg2.ChunkNo);
            Assert.AreEqual(getChunkMsg.ReplicationDeg, getChunkMsg2.ReplicationDeg);
            Assert.AreEqual(getChunkMsg.Body, getChunkMsg2.Body);

            var chunkMsg = Message.BuildChunkMessage(0, 0, FileId1, 0, body);
            bytes = chunkMsg.Serialize();
            var chunkMsg2 = Message.Deserialize(bytes);
            Assert.AreEqual(chunkMsg.MessageType, chunkMsg2.MessageType);
            Assert.AreEqual(chunkMsg.VersionM, chunkMsg2.VersionM);
            Assert.AreEqual(chunkMsg.VersionN, chunkMsg2.VersionN);
            Assert.AreEqual(chunkMsg.FileId, chunkMsg2.FileId);
            Assert.AreEqual(chunkMsg.ChunkNo, chunkMsg2.ChunkNo);
            Assert.AreEqual(chunkMsg.ReplicationDeg, chunkMsg2.ReplicationDeg);
            CollectionAssert.AreEqual(chunkMsg.Body, chunkMsg2.Body);

            var putChunkMsg = Message.BuildPutChunkMessage(0, 0, FileId1, 0, 0, body);
            bytes = putChunkMsg.Serialize();
            var putChunkMsg2 = Message.Deserialize(bytes);
            Assert.AreEqual(putChunkMsg.MessageType, putChunkMsg2.MessageType);
            Assert.AreEqual(putChunkMsg.VersionM, putChunkMsg2.VersionM);
            Assert.AreEqual(putChunkMsg.VersionN, putChunkMsg2.VersionN);
            Assert.AreEqual(putChunkMsg.FileId, putChunkMsg2.FileId);
            Assert.AreEqual(putChunkMsg.ChunkNo, putChunkMsg2.ChunkNo);
            Assert.AreEqual(putChunkMsg.ReplicationDeg, putChunkMsg2.ReplicationDeg);
            CollectionAssert.AreEqual(putChunkMsg.Body, putChunkMsg2.Body);

            var storedMsg = Message.BuildStoredMessage(0, 0, FileId1, 0);
            bytes = storedMsg.Serialize();
            var storedMsg2 = Message.Deserialize(bytes);
            Assert.AreEqual(storedMsg.MessageType, storedMsg2.MessageType);
            Assert.AreEqual(storedMsg.VersionM, storedMsg2.VersionM);
            Assert.AreEqual(storedMsg.VersionN, storedMsg2.VersionN);
            Assert.AreEqual(storedMsg.FileId, storedMsg2.FileId);
            Assert.AreEqual(storedMsg.ChunkNo, storedMsg2.ChunkNo);
            Assert.AreEqual(storedMsg.ReplicationDeg, storedMsg2.ReplicationDeg);
            CollectionAssert.AreEqual(storedMsg.Body, storedMsg2.Body);

            var deleteMsg = Message.BuildDeleteMessage(FileId1);
            bytes = deleteMsg.Serialize();
            var deleteMsg2 = Message.Deserialize(bytes);
            Assert.AreEqual(deleteMsg.MessageType, deleteMsg2.MessageType);
            Assert.AreEqual(deleteMsg.VersionM, deleteMsg2.VersionM);
            Assert.AreEqual(deleteMsg.VersionN, deleteMsg2.VersionN);
            Assert.AreEqual(deleteMsg.FileId, deleteMsg2.FileId);
            Assert.AreEqual(deleteMsg.ChunkNo, deleteMsg2.ChunkNo);
            Assert.AreEqual(deleteMsg.ReplicationDeg, deleteMsg2.ReplicationDeg);
            CollectionAssert.AreEqual(deleteMsg.Body, deleteMsg2.Body);

            var removedMsg = Message.BuildRemovedMessage(0, 0, FileId1, 0);
            bytes = removedMsg.Serialize();
            var removedMsg2 = Message.Deserialize(bytes);
            Assert.AreEqual(removedMsg.MessageType, removedMsg2.MessageType);
            Assert.AreEqual(removedMsg.VersionM, removedMsg2.VersionM);
            Assert.AreEqual(removedMsg.VersionN, removedMsg2.VersionN);
            Assert.AreEqual(removedMsg.FileId, removedMsg2.FileId);
            Assert.AreEqual(removedMsg.ChunkNo, removedMsg2.ChunkNo);
            Assert.AreEqual(removedMsg.ReplicationDeg, removedMsg2.ReplicationDeg);
            CollectionAssert.AreEqual(removedMsg.Body, removedMsg2.Body);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void TestMessageOutOfRangeField()
        {
            Message.BuildGetChunkMessage(10, 10, FileId1, 0);
        }
    }
}
