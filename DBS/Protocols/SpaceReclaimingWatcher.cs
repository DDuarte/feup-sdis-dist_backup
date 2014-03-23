using System;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;

// This is not actually needed but I don't want to delete my beautiful code.

namespace DBS.Protocols
{
    class SpaceReclaimingWatcher
    {
        public void Start()
        {
            // Create a new FileSystemWatcher and set its properties.
            var watcher = new FileSystemWatcher
            {
                Path = Core.Instance.BackupDirectory,
                Filter = "*_*",
                IncludeSubdirectories = true,
                EnableRaisingEvents = true
            };

            var watcherCreated = Observable.FromEventPattern<FileSystemEventArgs>(watcher, "Created");
            watcherCreated.Subscribe(OnCreated);
        }

        private void OnCreated(EventPattern<FileSystemEventArgs> pattern)
        {
            var dirName = ((FileSystemWatcher) pattern.Sender).Path;
            var size = GetDirectorySize(dirName);

            if (size <= Core.Instance.MaxBackupSize) return;

            foreach (var f in Core.Instance.Store)
            {
                var actualDegree = f.Value.ActualDegree;
                var wantedDegree = f.Value.WantedDegree;
                if (actualDegree > wantedDegree)
                {
                    var keyParts = f.Key.Split('_');
                    var fileIdStr = keyParts[0];
                    var chunkNo = int.Parse(keyParts[1]);
                    var fileChunk = new FileChunk(new FileId(fileIdStr), chunkNo);
                    Console.WriteLine("SpaceReclamingWathcer:OnCreated: Starting SpaceReclaimingProtocol for {0}",
                        fileChunk);
                    new SpaceReclaimingProtocol(fileChunk).Run();
                }
            }
        }

        private static long GetDirectorySize(string p)
        {
            try
            {
                return Directory.GetFiles(p).Select(name => new FileInfo(name)).Select(info => info.Length).Sum();
            }
            catch (Exception ex)
            {
                Console.WriteLine("SpaceReclaimingWatcher:GetDirectorySize: {0}", ex);
                return 0L;
            }
        }
    }
}
