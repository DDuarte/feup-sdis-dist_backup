using System;
using System.Net;
using System.Text;
using DBS;
using DBS.Messages;
using DBS.Messages.Enhancements;
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

        [TestMethod]
        public void TestPutChunkMessage()
        {
            var msg = new PutChunkMessage(1, 0, FileId1, 50, 5, Body1);

            Assert.AreEqual(MessageType.PutChunk, msg.MessageType);
            Assert.AreEqual(1, msg.VersionM);
            Assert.AreEqual(0, msg.VersionN);
            Assert.AreEqual("1.0", msg.GetVersion());
            Assert.AreEqual(FileId1, msg.FileId);
            Assert.AreEqual(50, msg.ChunkNo);
            Assert.AreEqual(5, msg.ReplicationDeg);
            CollectionAssert.AreEqual(Body1, msg.Body);
        }

        [TestMethod]
        public void TestSerializeDeserialize()
        {
            var body = new byte[255];
            for (var i = 0; i < body.Length; ++i)
                body[i] = (byte)i;

            var msgs = new Message[]
            {
                new PutChunkMessage(5, 7, FileId1, 100, 2, body),
                new StoredMessage(5, 7, FileId1, 100),
                new GetChunkMessage(5, 7, FileId1, 100),
                new ChunkMessage(5, 7, FileId1, 100, body),
                new DeleteMessage(FileId1),
                new RemovedMessage(5, 7, FileId1, 100),
                new LookupMessage(5, 7, FileId1),
                new GotMessage(5, 7, FileId1),
                new ACKMessage(5, 7, FileId1, 100) 
            };

            foreach (var message in msgs)
            {
                var arr = message.Serialize();
                var arr2 = Message.Deserialize(arr).Serialize();
                CollectionAssert.AreEqual(arr2, arr);
            }
        }

        [TestMethod]
        public void TestMessageFieldUpperBounds()
        {
            var body = new byte[255];
            for (var i = 0; i < body.Length; ++i)
                    body[i] = (byte)i;

            var getChunkMsg = new GetChunkMessage(9, 9, FileId1, 999999);
            var bytes = getChunkMsg.Serialize();
            var getChunkMsg2 = Message.Deserialize(bytes) as GetChunkMessage;
            Assert.IsNotNull(getChunkMsg2);
            Assert.AreEqual(getChunkMsg.MessageType, getChunkMsg2.MessageType);
            Assert.AreEqual(getChunkMsg.VersionM, getChunkMsg2.VersionM);
            Assert.AreEqual(getChunkMsg.VersionN, getChunkMsg2.VersionN);
            Assert.AreEqual(getChunkMsg.FileId, getChunkMsg2.FileId);
            Assert.AreEqual(getChunkMsg.ChunkNo, getChunkMsg2.ChunkNo);

            var chunkMsg = new ChunkMessage(9, 9, FileId1, 999999, body);
            bytes = chunkMsg.Serialize();
            var chunkMsg2 = Message.Deserialize(bytes) as ChunkMessage;
            Assert.IsNotNull(chunkMsg2);
            Assert.AreEqual(chunkMsg.MessageType, chunkMsg2.MessageType);
            Assert.AreEqual(chunkMsg.VersionM, chunkMsg2.VersionM);
            Assert.AreEqual(chunkMsg.VersionN, chunkMsg2.VersionN);
            Assert.AreEqual(chunkMsg.FileId, chunkMsg2.FileId);
            Assert.AreEqual(chunkMsg.ChunkNo, chunkMsg2.ChunkNo);
            CollectionAssert.AreEqual(chunkMsg.Body, chunkMsg2.Body);

            var putChunkMsg = new PutChunkMessage(9, 9, FileId1, 999999, 9, body);
            bytes = putChunkMsg.Serialize();
            var putChunkMsg2 = Message.Deserialize(bytes) as PutChunkMessage;
            Assert.IsNotNull(putChunkMsg2);
            Assert.AreEqual(putChunkMsg.MessageType, putChunkMsg2.MessageType);
            Assert.AreEqual(putChunkMsg.VersionM, putChunkMsg2.VersionM);
            Assert.AreEqual(putChunkMsg.VersionN, putChunkMsg2.VersionN);
            Assert.AreEqual(putChunkMsg.FileId, putChunkMsg2.FileId);
            Assert.AreEqual(putChunkMsg.ChunkNo, putChunkMsg2.ChunkNo);
            Assert.AreEqual(putChunkMsg.ReplicationDeg, putChunkMsg2.ReplicationDeg);
            CollectionAssert.AreEqual(putChunkMsg.Body, putChunkMsg2.Body);

            var storedMsg = new StoredMessage(9, 9, FileId1, 999999);
            bytes = storedMsg.Serialize();
            var storedMsg2 = Message.Deserialize(bytes) as StoredMessage;
            Assert.IsNotNull(storedMsg2);
            Assert.AreEqual(storedMsg.MessageType, storedMsg2.MessageType);
            Assert.AreEqual(storedMsg.VersionM, storedMsg2.VersionM);
            Assert.AreEqual(storedMsg.VersionN, storedMsg2.VersionN);
            Assert.AreEqual(storedMsg.FileId, storedMsg2.FileId);
            Assert.AreEqual(storedMsg.ChunkNo, storedMsg2.ChunkNo);

            var deleteMsg = new DeleteMessage(FileId1);
            bytes = deleteMsg.Serialize();
            var deleteMsg2 = Message.Deserialize(bytes) as DeleteMessage;
            Assert.IsNotNull(deleteMsg2);
            Assert.AreEqual(deleteMsg.MessageType, deleteMsg2.MessageType);

            var removedMsg = new RemovedMessage(9, 9, FileId1, 999999);
            bytes = removedMsg.Serialize();
            var removedMsg2 = Message.Deserialize(bytes) as RemovedMessage;
            Assert.IsNotNull(removedMsg2);
            Assert.AreEqual(removedMsg.MessageType, removedMsg2.MessageType);
            Assert.AreEqual(removedMsg.VersionM, removedMsg2.VersionM);
            Assert.AreEqual(removedMsg.VersionN, removedMsg2.VersionN);
            Assert.AreEqual(removedMsg.FileId, removedMsg2.FileId);
            Assert.AreEqual(removedMsg.ChunkNo, removedMsg2.ChunkNo);

            var lookupMsg = new LookupMessage(9, 9, FileId1);
            bytes = lookupMsg.Serialize();
            var lookupMsg2 = Message.Deserialize(bytes) as LookupMessage;
            Assert.IsNotNull(lookupMsg2);
            Assert.AreEqual(lookupMsg.MessageType, lookupMsg2.MessageType);
            Assert.AreEqual(lookupMsg.VersionM, lookupMsg2.VersionM);
            Assert.AreEqual(lookupMsg.VersionN, lookupMsg2.VersionN);
            Assert.AreEqual(lookupMsg.FileId, lookupMsg2.FileId);

            var gotMsg = new GotMessage(9, 9, FileId1);
            bytes = gotMsg.Serialize();
            var gotMsg2 = Message.Deserialize(bytes) as GotMessage;
            Assert.IsNotNull(gotMsg2);
            Assert.AreEqual(gotMsg.MessageType, gotMsg2.MessageType);
            Assert.AreEqual(gotMsg.VersionM, gotMsg2.VersionM);
            Assert.AreEqual(gotMsg.VersionN, gotMsg2.VersionN);
            Assert.AreEqual(gotMsg.FileId, gotMsg2.FileId);

            var ackMsg = new ACKMessage(9, 9, FileId1, 999999);
            bytes = ackMsg.Serialize();
            var ackMsg2 = Message.Deserialize(bytes) as ACKMessage;
            Assert.IsNotNull(ackMsg2);
            Assert.AreEqual(ackMsg.MessageType, ackMsg2.MessageType);
            Assert.AreEqual(ackMsg.VersionM, ackMsg2.VersionM);
            Assert.AreEqual(ackMsg.VersionN, ackMsg2.VersionN);
            Assert.AreEqual(ackMsg.FileId, ackMsg2.FileId);
            Assert.AreEqual(ackMsg.ChunkNo, ackMsg2.ChunkNo);
        }

        [TestMethod]
        public void TestMessageFieldLowerBounds()
        {
            var body = new byte[255];
            for (var i = 0; i < body.Length; ++i)
                body[i] = (byte)i;

            var getChunkMsg = new GetChunkMessage(0, 0, FileId1, 0);
            var bytes = getChunkMsg.Serialize();
            var getChunkMsg2 = Message.Deserialize(bytes) as GetChunkMessage;
            Assert.IsNotNull(getChunkMsg2);
            Assert.AreEqual(getChunkMsg.MessageType, getChunkMsg2.MessageType);
            Assert.AreEqual(getChunkMsg.VersionM, getChunkMsg2.VersionM);
            Assert.AreEqual(getChunkMsg.VersionN, getChunkMsg2.VersionN);
            Assert.AreEqual(getChunkMsg.FileId, getChunkMsg2.FileId);
            Assert.AreEqual(getChunkMsg.ChunkNo, getChunkMsg2.ChunkNo);

            var chunkMsg = new ChunkMessage(0, 0, FileId1, 0, body);
            bytes = chunkMsg.Serialize();
            var chunkMsg2 = Message.Deserialize(bytes) as ChunkMessage;
            Assert.IsNotNull(chunkMsg2);
            Assert.AreEqual(chunkMsg.MessageType, chunkMsg2.MessageType);
            Assert.AreEqual(chunkMsg.VersionM, chunkMsg2.VersionM);
            Assert.AreEqual(chunkMsg.VersionN, chunkMsg2.VersionN);
            Assert.AreEqual(chunkMsg.FileId, chunkMsg2.FileId);
            Assert.AreEqual(chunkMsg.ChunkNo, chunkMsg2.ChunkNo);
            CollectionAssert.AreEqual(chunkMsg.Body, chunkMsg2.Body);

            var putChunkMsg = new PutChunkMessage(0, 0, FileId1, 0, 0, body);
            bytes = putChunkMsg.Serialize();
            var putChunkMsg2 = Message.Deserialize(bytes) as PutChunkMessage;
            Assert.IsNotNull(putChunkMsg2);
            Assert.AreEqual(putChunkMsg.MessageType, putChunkMsg2.MessageType);
            Assert.AreEqual(putChunkMsg.VersionM, putChunkMsg2.VersionM);
            Assert.AreEqual(putChunkMsg.VersionN, putChunkMsg2.VersionN);
            Assert.AreEqual(putChunkMsg.FileId, putChunkMsg2.FileId);
            Assert.AreEqual(putChunkMsg.ChunkNo, putChunkMsg2.ChunkNo);
            Assert.AreEqual(putChunkMsg.ReplicationDeg, putChunkMsg2.ReplicationDeg);
            CollectionAssert.AreEqual(putChunkMsg.Body, putChunkMsg2.Body);

            var storedMsg = new StoredMessage(0, 0, FileId1, 0);
            bytes = storedMsg.Serialize();
            var storedMsg2 = Message.Deserialize(bytes) as StoredMessage;
            Assert.IsNotNull(storedMsg2);
            Assert.AreEqual(storedMsg.MessageType, storedMsg2.MessageType);
            Assert.AreEqual(storedMsg.VersionM, storedMsg2.VersionM);
            Assert.AreEqual(storedMsg.VersionN, storedMsg2.VersionN);
            Assert.AreEqual(storedMsg.FileId, storedMsg2.FileId);
            Assert.AreEqual(storedMsg.ChunkNo, storedMsg2.ChunkNo);

            var deleteMsg = new DeleteMessage(FileId1);
            bytes = deleteMsg.Serialize();
            var deleteMsg2 = Message.Deserialize(bytes) as DeleteMessage;
            Assert.IsNotNull(deleteMsg2);
            Assert.AreEqual(deleteMsg.MessageType, deleteMsg2.MessageType);

            var removedMsg = new RemovedMessage(0, 0, FileId1, 0);
            bytes = removedMsg.Serialize();
            var removedMsg2 = Message.Deserialize(bytes) as RemovedMessage;
            Assert.IsNotNull(removedMsg2);
            Assert.AreEqual(removedMsg.MessageType, removedMsg2.MessageType);
            Assert.AreEqual(removedMsg.VersionM, removedMsg2.VersionM);
            Assert.AreEqual(removedMsg.VersionN, removedMsg2.VersionN);
            Assert.AreEqual(removedMsg.FileId, removedMsg2.FileId);
            Assert.AreEqual(removedMsg.ChunkNo, removedMsg2.ChunkNo);

            var lookupMsg = new LookupMessage(0, 0, FileId1);
            bytes = lookupMsg.Serialize();
            var lookupMsg2 = Message.Deserialize(bytes) as LookupMessage;
            Assert.IsNotNull(lookupMsg2);
            Assert.AreEqual(lookupMsg.MessageType, lookupMsg2.MessageType);
            Assert.AreEqual(lookupMsg.VersionM, lookupMsg2.VersionM);
            Assert.AreEqual(lookupMsg.VersionN, lookupMsg2.VersionN);
            Assert.AreEqual(lookupMsg.FileId, lookupMsg2.FileId);

            var gotMsg = new GotMessage(0, 0, FileId1);
            bytes = gotMsg.Serialize();
            var gotMsg2 = Message.Deserialize(bytes) as GotMessage;
            Assert.IsNotNull(gotMsg2);
            Assert.AreEqual(gotMsg.MessageType, gotMsg2.MessageType);
            Assert.AreEqual(gotMsg.VersionM, gotMsg2.VersionM);
            Assert.AreEqual(gotMsg.VersionN, gotMsg2.VersionN);
            Assert.AreEqual(gotMsg.FileId, gotMsg2.FileId);

            var ackMsg = new ACKMessage(0, 0, FileId1, 0);
            bytes = ackMsg.Serialize();
            var ackMsg2 = Message.Deserialize(bytes) as ACKMessage;
            Assert.IsNotNull(ackMsg2);
            Assert.AreEqual(ackMsg.MessageType, ackMsg2.MessageType);
            Assert.AreEqual(ackMsg.VersionM, ackMsg2.VersionM);
            Assert.AreEqual(ackMsg.VersionN, ackMsg2.VersionN);
            Assert.AreEqual(ackMsg.FileId, ackMsg2.FileId);
            Assert.AreEqual(ackMsg.ChunkNo, ackMsg2.ChunkNo);

            var connInfoMsg = new ConnInfoMessage(0, 0, FileId1, 0, 50, IPAddress.Parse("0.0.0.0"));
            bytes = connInfoMsg.Serialize();
            var connInfoMsg2 = Message.Deserialize(bytes) as ConnInfoMessage;
            Assert.IsNotNull(connInfoMsg2);
            Assert.AreEqual(connInfoMsg.MessageType, connInfoMsg2.MessageType);
            Assert.AreEqual(connInfoMsg.VersionM, connInfoMsg2.VersionM);
            Assert.AreEqual(connInfoMsg.VersionN, connInfoMsg2.VersionN);
            Assert.AreEqual(connInfoMsg.FileId, connInfoMsg2.FileId);
            Assert.AreEqual(connInfoMsg.ChunkNo, connInfoMsg2.ChunkNo);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void TestMessageOutOfRangeField()
        {
// ReSharper disable once ObjectCreationAsStatement
            new GetChunkMessage(10, 10, FileId1, 0);
        }

        [TestMethod]
        public void TestMessageBadSerialize()
        {
            var rnd = new Random(1);
            var bytes = new byte[100];
            for (var i = 0; i < 20; ++i)
            {
                rnd.NextBytes(bytes);

                var msg = Message.Deserialize(bytes);
                Assert.IsNull(msg);
            }

            const string headerPart = "PUTCHUNK 1.0 ";
            var headerPartBytes = Encoding.ASCII.GetBytes(headerPart);
            rnd.NextBytes(bytes);

            var newBytes = new byte[headerPartBytes.Length + bytes.Length];
        }

        [TestMethod]
        public void TestMessageBadPartialSerialize()
        {
            var rnd = new Random();
            var bytes = new byte[100];

            const string headerPart = "PUTCHUNK 1.0 ";
            var headerPartBytes = Encoding.ASCII.GetBytes(headerPart);

            for (var i = 0; i < 20; ++i)
            {
                rnd.NextBytes(bytes);

                var newBytes = new byte[headerPartBytes.Length + bytes.Length];
                Array.Copy(headerPartBytes, newBytes, headerPartBytes.Length);
                Array.Copy(bytes, 0, newBytes, headerPartBytes.Length, bytes.Length);

                var msg = Message.Deserialize(newBytes);
                Assert.IsNull(msg);
            }
        }
    }
}
