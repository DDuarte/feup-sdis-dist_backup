using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DBS.Protocols.Enhancements
{
    class LookUpProtocol : IProtocol
    {
        private readonly string _backupDir;
        private ConcurrentDictionary<string, byte> _backedUpFiles;
        private const int WaitPeriod = 10000;
        public LookUpProtocol(string backupDir)
        {
            _backupDir = backupDir;
            _backedUpFiles = new ConcurrentDictionary<string, byte>();
        }

        private IDisposable LookUpFileId(FileId fileId)
        {
            Core.Instance.MCChannel.Send(Message.BuildLookUpMessage(fileId));
            return Core.Instance.MCChannel.Received.Where(message =>
                message.MessageType == MessageType.Got &&
                message.FileId == fileId).Subscribe(msg =>
                {
                    byte val;
                    _backedUpFiles.TryRemove(msg.FileId.ToString(), out val);
                });
        }

        public void Run() // should this run in a separate thread? We're deleting files...
        {
            // get array with all the fileId's associated with the backed up chunks
            var fileIdArray = Directory.GetFiles(_backupDir, "*_*")
                .Select(path =>
                {
                    var fileName = Path.GetFileName(path);
                    return fileName != null ? fileName.Split('_')[0] : null;
                });

            // remove duplicates, transform the collection into a ConcurrentDictionary
            _backedUpFiles = new ConcurrentDictionary<string, byte>(fileIdArray.ToDictionary(item => item, _ => new byte()));

            // perform a lookup for each fileId
            var subscriptions = _backedUpFiles.Keys.Select(fileIdStr => LookUpFileId(new FileId(fileIdStr)));

            // wait
            Thread.Sleep(WaitPeriod);

            foreach (var subscription in subscriptions)
                subscription.Dispose();

            // delete all the unused chunks
            foreach (var unusedFile in _backedUpFiles.Keys)
            {
                var fileList = Directory.GetFiles(_backupDir, unusedFile + '_' + '*');
                foreach (var backedUpChunk in fileList)
                    System.IO.File.Delete(backedUpChunk);
            }
        }
    }
}
