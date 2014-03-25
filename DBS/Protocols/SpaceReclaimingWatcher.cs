using System;
using System.IO;
using System.Reactive;
using System.Reactive.Linq;
using Util = DBS.Utilities.Utilities;

// This is not actually needed but can be required later on.

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
            var size = Util.GetDirectorySize(dirName);

            if (size > Core.Instance.MaxBackupSize);
                new SpaceReclaimingProtocol().Run();
        }
    }
}
