﻿using System;
using System.IO;
using System.Reactive.Linq;
using DBS.Messages;
using DBS.Messages.Enhancements;

namespace DBS.Protocols.Enhancements
{
    class LookUpService : IService<LookupMessage>
    {
        public void OnNext(LookupMessage msg)
        {
            var fileIds = Directory.GetFiles(Core.Instance.Config.BackupDirectory, msg.FileId + "_*");
            if (fileIds.Length == 0)
                return;
            
            Core.Instance.MCChannel.Send(new GotMessage(msg.FileId));
        }

        public void OnError(Exception error)
        {
            Core.Instance.Log.Error("LookUpService:OnError", error);
        }

        public void OnCompleted()
        {
            Core.Instance.Log.Info("LookUpService:OnCompleted");
        }

        public void Start()
        {
            Core.Instance.Log.Info("Starting LookUpService");
            Core.Instance.MCChannel.Received.Where(message =>
                message.MessageType == MessageType.Lookup)
                .Cast<LookupMessage>().Subscribe(this);
        }

        public void Stop()
        {
            Core.Instance.Log.Info("LookUpService:Stop");   
        }
    }
}
