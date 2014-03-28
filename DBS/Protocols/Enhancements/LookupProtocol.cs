using System;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using DBS.Messages;
using DBS.Messages.Enhancements;
using DBS.Utilities;

namespace DBS.Protocols.Enhancements
{
    class LookUpProtocol : IProtocol
    {
        private const int WaitPeriod = 10000;

        public Task Run() // should this run in a separate thread? We're deleting files...
        {
            var backupDir = Core.Instance.Config.BackupDirectory;
            Core.Instance.Log.InfoFormat("Starting LookUpProtocol: {0}", backupDir);
            return Task.Factory.StartNew(() =>
            {
                // get array with all the fileId's associated with the backed up chunks
                var fileIds = Directory.GetFiles(backupDir, "*_*")
                    .Select(path =>
                    {
                        var fileName = Path.GetFileName(path);
                        return new FileChunk(fileName).FileId;
                    });

                // remove duplicates, transform the collection into a ConcurrentHashSet
                var backedUpFiles = new ConcurrentHashSet<FileId>(fileIds.Distinct());

                // perform a lookup for each fileId
                var subscriptions = backedUpFiles.Select(id =>
                {
                    Core.Instance.MCChannel.Send(new LookupMessage(id));
                    return Core.Instance.MCChannel.Received
                        .Where(message => message.MessageType == MessageType.Got)
                        .Cast<GotMessage>()
                        .Where(message => message.FileId == id)
// ReSharper disable once AccessToDisposedClosure
                        .Subscribe(msg => backedUpFiles.Remove(msg.FileId));
                });

                // wait
                Task.Delay(WaitPeriod).Wait();

                foreach (var subscription in subscriptions)
                    subscription.Dispose();

                // delete all the unused chunks
                foreach (var unusedFile in backedUpFiles)
                {
                    var fileList = Directory.GetFiles(backupDir, unusedFile + "_*");
                    foreach (var backedUpChunk in fileList)
                        File.Delete(backedUpChunk);
                }

                backedUpFiles.Dispose();
            });
        }
    }
}
