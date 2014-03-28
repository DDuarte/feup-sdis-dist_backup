using System;
using System.IO;
using System.Reactive;
using System.Reactive.Linq;
using Util = DBS.Utilities.Utilities;

// This is not actually needed but can be required later on.

namespace DBS.Protocols
{
    class SpaceReclaimingWatcher : IService
    {
        private FileSystemWatcher _watcher;
        private IDisposable _watcherCreatedSubs;

        public void Start()
        {
            Core.Instance.Log.Info("Starting SpaceReclaimingWatcher");
            // Create a new FileSystemWatcher and set its properties.
            _watcher = new FileSystemWatcher
            {
                Path = Core.Instance.Config.BackupDirectory,
                Filter = "*_*",
                IncludeSubdirectories = true,
                EnableRaisingEvents = true
            };

            var watcherCreated = Observable.FromEventPattern<FileSystemEventArgs>(_watcher, "Created");
            _watcherCreatedSubs = watcherCreated.Subscribe(OnCreated);
        }

        public void Stop()
        {
            if (_watcherCreatedSubs != null)
                _watcherCreatedSubs.Dispose();

            if (_watcher != null)
            {
                _watcher.EnableRaisingEvents = false;
                _watcher.Dispose();
            }
        }

        private void OnCreated(EventPattern<FileSystemEventArgs> pattern)
        {
            var dirName = ((FileSystemWatcher) pattern.Sender).Path;
            var size = Util.GetDirectorySize(dirName);

            if (size > Core.Instance.Config.MaxBackupSize)
                new SpaceReclaimingProtocol().Run();
        }
    }
}
