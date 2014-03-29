using System;
using System.Reactive.Linq;
using System.Threading.Tasks;
using DBS.Messages;

namespace DBS.Protocols
{
    /// <summary>
    /// Listens to GETCHUNK messages on MC
    /// </summary>
    class RestoreChunkService : IServiceObserver<GetChunkMessage>
    {
        public void Start()
        {
            Core.Instance.Log.Info("Starting RestoreChunkService");
            Core.Instance.MCChannel.Received
                .Where(message => message.MessageType == MessageType.GetChunk)
                .Cast<GetChunkMessage>()
                .Subscribe(this);
        }

        public void Stop()
        {
            Core.Instance.Log.Info("RestoreChunkService:Stop");
        }

        public void OnNext(GetChunkMessage msg)
        {
            var fileChunk = new FileChunk(msg.FileId, msg.ChunkNo);
            if (!fileChunk.Exists())
            {
                // we were supposed to have this chunk: send removed message and remove the entry in the local count
                if (Core.Instance.ChunkPeers.HasChunkPeer(fileChunk)) 
                    Core.Instance.MCChannel.Send(new RemovedMessage(fileChunk));
                return;
            }

            try
            {
                var chunkReceived = false;
                var disposable = Core.Instance.MDRChannel.Received
                    .Where(message => message.MessageType == MessageType.Chunk)
                    .Cast<ChunkMessage>()
                    .Where(message => message.ChunkNo == msg.ChunkNo &&
                        message.FileId == msg.FileId)
                    .Subscribe(_ => chunkReceived = true);

                Task.Delay(Core.Instance.RandomDelay).Wait();
                disposable.Dispose();

                if (!chunkReceived)
                {
                    var data = fileChunk.GetData();
                    var chunkMsg = new ChunkMessage(fileChunk, data);
                    Core.Instance.MDRChannel.Send(chunkMsg);
                }
            }
            catch (Exception ex)
            {
                Core.Instance.Log.Error("RestoreChunkService", ex);
            }
        }

        public void OnError(Exception error)
        {
            Core.Instance.Log.Error("RestoreChunkService:OnError", error);
        }

        public void OnCompleted()
        {
            Core.Instance.Log.Info("RestoreChunkService:OnCompleted");
        }
    }
}
