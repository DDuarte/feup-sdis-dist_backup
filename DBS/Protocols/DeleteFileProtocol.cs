using System;
using System.Threading.Tasks;

namespace DBS.Protocols
{
    public class DeleteFileProtocol : IProtocol
    {
        private readonly FileEntry _fileEntry;

        public DeleteFileProtocol(FileEntry fileEntry)
        {
            if (fileEntry == null)
                throw new ArgumentNullException("fileEntry");

            _fileEntry = fileEntry;
        }

        public Task Run()
        {
            return Task.Factory.StartNew(() =>
            {
                var msg = Message.BuildDeleteMessage(_fileEntry.FileId);
                Core.Instance.MCChannel.Send(msg);
            });
        }
    }
}
