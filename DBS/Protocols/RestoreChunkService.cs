﻿using System;
using System.Reactive.Linq;
using System.Threading;
using DBS.Messages;

namespace DBS.Protocols
{
    /// <summary>
    /// Listens to GETCHUNK messages on MC
    /// </summary>
    class RestoreChunkService : IService<GetChunkMessage>
    {
        public void Start()
        {
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
            if (!fileChunk.Exists()) // we don't have this chunk, do nothing
                return;

            try
            {
                var chunkReceived = false;
                var disposable = Core.Instance.MDRChannel.Received
                    .Where(message => message.MessageType == MessageType.Chunk)
                    .Cast<ChunkMessage>()
                    .Where(message => message.ChunkNo == msg.ChunkNo &&
                        message.FileId == msg.FileId)
                    .Subscribe(_ => chunkReceived = true);

                Thread.Sleep(Core.Instance.RandomDelay); // random delay uniformly distributed
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

