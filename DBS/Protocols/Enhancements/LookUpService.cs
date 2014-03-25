using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace DBS.Protocols.Enhancements
{
    class LookUpService : IService
    {
        public void OnNext(Message msg)
        {
            var fileIds = Directory.GetFiles(Core.Instance.BackupDirectory, msg.FileId + "_*");
            if (fileIds.Length == 0)
                return;
            
            Core.Instance.MCChannel.Send(Message.BuildGotMessage(msg.FileId));
        }

        public void OnError(Exception error)
        {

        }

        public void OnCompleted()
        {

        }

        public void Start()
        {
            Core.Instance.MCChannel.Received.Where(message =>
                message.MessageType == MessageType.LookUp).Subscribe(this);
        }

        public void Stop()
        {
            
        }
    }
}
