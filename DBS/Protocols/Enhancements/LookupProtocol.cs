using System;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using DBS.Messages;
using DBS.Messages.Enhancements;
using DBS.Utilities;

namespace DBS.Protocols.Enhancements
{
    class LookUpProtocol : IProtocol
    {
        private readonly string _backupDir;
        private ConcurrentHashSet<FileId> _backedUpFiles;
        private const int WaitPeriod = 10000;
        public LookUpProtocol(string backupDir)
        {
            _backupDir = backupDir;
        }

        private IDisposable LookUpFileId(FileId fileId)
        {
            Core.Instance.MCChannel.Send(new LookupMessage(fileId));
            return Core.Instance.MCChannel.Received
                .Where(message => message.MessageType == MessageType.Got)
                .Cast<GotMessage>()
                .Where(message => message.FileId == fileId)
                .Subscribe(msg => _backedUpFiles.Remove(msg.FileId));
        }

        public Task Run() // should this run in a separate thread? We're deleting files...
        {
            return Task.Factory.StartNew(() =>
            {
                // get array with all the fileId's associated with the backed up chunks
                var fileIds = Directory.GetFiles(_backupDir, "*_*")
                    .Select(path =>
                    {
                        var fileName = Path.GetFileName(path);
                        return new FileChunk(fileName).FileId;
                    });

                // remove duplicates, transform the collection into a ConcurrentHashSet
                _backedUpFiles = new ConcurrentHashSet<FileId>(fileIds.Distinct());

                // perform a lookup for each fileId
                var subscriptions = _backedUpFiles.Select(LookUpFileId);

                // wait
                Thread.Sleep(WaitPeriod);

                foreach (var subscription in subscriptions)
                    subscription.Dispose();

                // delete all the unused chunks
                foreach (var unusedFile in _backedUpFiles)
                {
                    var fileList = Directory.GetFiles(_backupDir, unusedFile + "_*");
                    foreach (var backedUpChunk in fileList)
                        File.Delete(backedUpChunk);
                }
            });
        }
    }
}
