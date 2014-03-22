using System;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;

namespace DBS
{
    /// <summary>
    /// Listens to GETCHUNK messages on MC
    /// </summary>
    class ChunkRestoreService : IService
    {
        public void Start()
        {
            Core.Instance.MCChannel.Received
                .Where(message => message.MessageType == MessageType.GetChunk)
                .Subscribe(this);
        }

        public void Stop()
        {
            Console.WriteLine("ChunkRestoreService:Stop");
        }

        public void OnNext(Message msg)
        {
            if (!msg.ChunkNo.HasValue)
            {
                Console.WriteLine("ChunkRestoreService: bad msg, ChunkNo has no value.");
                return;
            }

            var fileName = msg.FileId + "_" + msg.ChunkNo;
            var fullPath = Path.Combine(Core.Instance.BackupDirectory, fileName);
            if (!File.Exists(fullPath)) // we don't have this chunk, do nothing
                return;

            try
            {
                var buffer = new byte[64000];
                var fileData = new FileStream(fullPath, FileMode.Open);
                var bytesRead = fileData.Read(buffer, 0, 64000); // try to read the maximum chunk size from the file
                var data = buffer.Take(bytesRead).ToArray();
                var chunkMsg = Message.BuildChunkMessage(msg.FileId, msg.ChunkNo.Value, data);

                var chunkReceived = false;
                var disposable = Core.Instance.MDRChannel.Received.Where(message =>
                    message.MessageType == MessageType.Chunk &&
                    message.ChunkNo == msg.ChunkNo &&
                    message.FileId == msg.FileId).Subscribe(_ => chunkReceived = true);

                Thread.Sleep(Core.Instance.Rnd.Next(0, 401)); // random delay uniformly distributed between 0 and 400 ms

                if(!chunkReceived)
                    Core.Instance.MDRChannel.Send(chunkMsg);

                disposable.Dispose();
            }
            catch (Exception ex)
            {
                Console.WriteLine("ChunkRestoreService: " + ex);
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

