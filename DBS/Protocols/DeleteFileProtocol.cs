using System;
using System.Threading.Tasks;
using DBS.Messages;

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
            Core.Instance.Log.InfoFormat("Starting DeleteFileProtocol: {0}", _fileEntry);
            return Task.Factory.StartNew(() =>
            {
                var msg = new DeleteMessage(_fileEntry.FileId);
                Core.Instance.MCChannel.Send(msg);
            });
        }
    }
}
