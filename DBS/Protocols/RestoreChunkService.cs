using System;
using System.Reactive.Linq;
using System.Threading;

namespace DBS.Protocols
{
    /// <summary>
    /// Listens to GETCHUNK messages on MC
    /// </summary>
    class RestoreChunkService : IService
    {
        public void Start()
        {
            Core.Instance.MCChannel.Received
                .Where(message => message.MessageType == MessageType.GetChunk)
                .Subscribe(this);
        }

        public void Stop()
        {
            Console.WriteLine("RestoreChunkService:Stop");
        }

        public void OnNext(Message msg)
        {
            if (!msg.ChunkNo.HasValue)
            {
                Console.WriteLine("RestoreChunkService: bad msg, ChunkNo has no value.");
                return;
            }

            var fileChunk = new FileChunk(msg.FileId, msg.ChunkNo.Value);

            if (!fileChunk.Exists()) // we don't have this chunk, do nothing
                return;

            try
            {
                var chunkReceived = false;
                var disposable = Core.Instance.MDRChannel.Received.Where(message =>
                    message.MessageType == MessageType.Chunk &&
                    message.ChunkNo == msg.ChunkNo &&
                    message.FileId == msg.FileId).Subscribe(_ => chunkReceived = true);

                Thread.Sleep(Core.Instance.RandomDelay); // random delay uniformly distributed
                disposable.Dispose();

                if (!chunkReceived)
                {
                    var data = fileChunk.GetData();
                    var chunkMsg = Message.BuildChunkMessage(fileChunk, data);
                    Core.Instance.MDRChannel.Send(chunkMsg);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("RestoreChunkService: " + ex);
            }
        }

        public void OnError(Exception error)
        {
            
        }

        public void OnCompleted()
        {
            
        }
    }
}

