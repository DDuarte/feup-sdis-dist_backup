using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBS
{
    /// <summary>
    /// Launches the initiator-peer Chunk Restore Protocol
    /// </summary>
    class RestoreChunkSubprotocol : IProtocol
    {
        private readonly FileId _fileId;
        private readonly int _chunkNo;
        public Message Chunk { get; private set; }

        public RestoreChunkSubprotocol(FileId fileId, int chunkNo)
        {
            _fileId = fileId;
            _chunkNo = chunkNo;
        }

        private void SendGetChunk()
        {
            Core.Instance.MCChannel.Send(Message.BuildGetChunkMessage(_fileId, _chunkNo));

            // wait for response
            Chunk = Core.Instance.MDRChannel.Received.Where(message =>
                message.MessageType == MessageType.Chunk &&
                message.ChunkNo == _chunkNo &&
                message.FileId == _fileId)
                .Timeout(System.TimeSpan.FromMilliseconds(5000))
                .Next().First();
        }

        public void Run()
        {
            SendGetChunk();
        }
    }
}
