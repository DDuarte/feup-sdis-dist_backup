using System;
using System.IO;
using System.Linq;
using System.Security.Permissions;
using System.Threading;
using DBS;
using JsonConfig;

namespace Peer
{
    public class SpaceReclaimingWatcher
    {
        private IChannel _channel;

        public SpaceReclaimingWatcher(IChannel channel)
        {
            _channel = channel;
        }

        private Thread RunningThread { get; set; }

        [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        public void Run(string dir)
        {
            RunningThread = new Thread(() =>
            {
                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);

                // Create a new FileSystemWatcher and set its properties.
                var watcher = new FileSystemWatcher
                {
                    Path = dir,
                    NotifyFilter = NotifyFilters.Size
                };

                // Add event handlers.
                watcher.Changed += OnChanged;

                // Begin watching.
                watcher.EnableRaisingEvents = true;
            });

            RunningThread.Start();
        }

        public void Stop()
        {
            RunningThread.Abort();
        }

        private void OnChanged(object source, FileSystemEventArgs e)
        {
            var dirName = Path.GetDirectoryName(e.FullPath);
            if (dirName == null)
            {
                Console.WriteLine("Invalid path {0} in SpaceReclaimingWatcher:OnChanged", e.FullPath);
                return;
            }

            var size = GetDirectorySize(dirName);

            if (size > Config.Global.DiskSpace * 1000000)
            {
                // launch space reclaiming subprotocol

                foreach (var f in PersistentStore.Dict)
                {
                    var actualDegree = f.Value.ActualDegree;
                    var wantedDegree = f.Value.WantedDegree;
                    if (actualDegree > wantedDegree)
                    {
                        File.Delete(Path.Combine(dirName, f.Key));
                        // Not updating actualDegree here. Will be done when we receive our own REMOVED.

                        Console.WriteLine("Evicting file " + f.Key);

                        var keyParts = f.Key.Split('_');
                        var fileId = keyParts[0];
                        var chunkNo = int.Parse(keyParts[1]);
                        _channel.Send(Message.BuildRemovedMessage(fileId, chunkNo));
                    }
                }
            }
        }

        private static long GetDirectorySize(string p)
        {
            return Directory.GetFiles(p).Select(name => new FileInfo(name)).Select(info => info.Length).Sum();
        }
    }
}
