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
        private const int MaxRetries = 5;

        public Task Run()
        {
            var backupDir = Core.Instance.Config.BackupDirectory;
            Core.Instance.Log.InfoFormat("Starting LookUpProtocol: '{0}'", backupDir);
            return Task.Factory.StartNew(() =>
            {
                // get array with all the fileId's associated with the backed up chunks
                var fileIds = Core.Instance.ChunkPeers.Select(peer => new FileChunk(peer.Chunk))
                    .ToList().Where(fc =>
                    {
                        if (!fc.Exists())
                        {
                            Core.Instance.MCChannel.Send(new RemovedMessage(fc));
                            return false;
                        }
                        return true;
                    }).ToList().Select(fc => fc.FileId).ToList();

                if (fileIds.Count == 0)
                {
                    Core.Instance.Log.Info("LookUpProtocol: got no files, no lookup required");
                    return;
                }

                // remove duplicates, transform the collection into a ConcurrentHashSet
                var backedUpFilesId = new ConcurrentHashSet<FileId>(fileIds.Distinct());

                var waitPeriod = 1000;
                for (int retry = 0; retry < MaxRetries; ++retry)
                {
                    // perform a lookup for each fileId
                    var subscriptions = backedUpFilesId.Select(id =>
                    {
                        Core.Instance.MCChannel.Send(new LookupMessage(id));
                        return Core.Instance.MCChannel.Received
                            .Where(message => message.MessageType == MessageType.Got)
                            .Cast<GotMessage>()
                            .Where(message => message.FileId == id)
                            // ReSharper disable once AccessToDisposedClosure
                            .Subscribe(msg => backedUpFilesId.Remove(msg.FileId));
                    }).ToList();

                    // wait
                    Task.Delay(waitPeriod).Wait();

                    foreach (var subscription in subscriptions)
                        subscription.Dispose();

                    // if we got a Got for all the files we don't need to wait longer
                    if (backedUpFilesId.Count == 0)
                        break;

                    waitPeriod *= 2;
                }

                // delete all the unused chunks
                foreach (var unusedFileId in backedUpFilesId)
                {
                    var fileList = Directory.GetFiles(backupDir, unusedFileId + "_*");
                    foreach (var backedUpChunk in fileList)
                        File.Delete(backedUpChunk);

                    Core.Instance.ChunkPeers.RemoveAllChunkPeer(unusedFileId);
                }

                backedUpFilesId.Clear();
                backedUpFilesId.Dispose();
            });
        }
    }
}
