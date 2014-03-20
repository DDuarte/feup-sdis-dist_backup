using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using DBS;
using JsonConfig;

/* http://web.fe.up.pt/~pfs/aulas/sd2014/proj1.html */

namespace Peer
{
    static class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
                args = new[] {"225.0.0.10", "31000", "225.0.0.10", "31001", "225.0.0.10", "31002"}; // defaults

            if (args.Length != 6)
            {
                PrintUsage();
                Console.ReadKey();
                return;
            }

            // Get console arguments
            IPAddress mcIP, mdbIP, mdrIP;
            int mcPort, mdbPort, mdrPort;

            if (!IPAddress.TryParse(args[0], out mcIP))
            {
                Console.WriteLine("Invalid MC:IP");
                return;
            }
            if (!int.TryParse(args[1], out mcPort))
            {
                Console.WriteLine("Invalid MC:Port");
                return;
            }
            if (!IPAddress.TryParse(args[2], out mdbIP))
            {
                Console.WriteLine("Invalid MDB:IP");
                return;
            }
            if (!int.TryParse(args[3], out mdbPort))
            {
                Console.WriteLine("Invalid MDB:Port");
                return;
            }
            if (!IPAddress.TryParse(args[4], out mdrIP))
            {
                Console.WriteLine("Invalid MDR:IP");
                return;
            }
            if (!int.TryParse(args[5], out mdrPort))
            {
                Console.WriteLine("Invalid MDR:Port");
                return;
            }

            if (!NetworkUtilities.IsMulticastAddress(mcIP))
            {
                Console.WriteLine("MC:IP is not a multicast address");
                return;
            }
            if (!NetworkUtilities.IsMulticastAddress(mdbIP))
            {
                Console.WriteLine("MDB:IP is not a multicast address");
                return;
            }
            if (!NetworkUtilities.IsMulticastAddress(mdrIP))
            {
                Console.WriteLine("MDR:IP is not a multicast address");
                return;
            }

            // Get IPAddress
            Core.Instance.LocalIP = IPAddress.Parse(Config.Global.LocalIP);

            // Max site used to backup (locally)
            Core.Instance.MaxBackupSize = Config.Global.DiskSpace;

            // Create dictionary of files to mantain
            foreach (var f in Config.Global.Files)
                Core.Instance.BackupFiles.Add(f.Name, new FileEntry
                {
                    FileId = FileId.FromFile(f.Name),
                    ReplicationDegree = f.ReplicationDegree
                });

            // Setup directories
            Core.Instance.BackupDirectory = Config.Global.BackupDir;

            if (!Directory.Exists(Core.Instance.BackupDirectory))
                Directory.CreateDirectory(Core.Instance.BackupDirectory);

            // Create channels
            Core.Instance.MCChannel = new Channel(mcIP, mcPort) { Name = "MC" };
            Core.Instance.MDBChannel = new Channel(mdbIP, mdbPort) { Name = "MDB" };
            Core.Instance.MDRChannel = new Channel(mdrIP, mdrPort) { Name = "MDR" };

            Core.Instance.Start();

            /*
            // Start Tasks

            Task.Factory.StartNew(() => StoreFiles(backupDir));
            Task.Factory.StartNew(() => ListenRemoved(backupDir));

            foreach (var f in files)
                Task.Factory.StartNew(() => SendFileInChunks(f.Key, f.Value));

            var spaceRecl = new SpaceReclaimingWatcher(_mcChannel);
            spaceRecl.Run(backupDir);
             * */
        }

        /*
        private static void ListenRemoved(string dir)
        {
            var rnd = new Random();

            _mcChannel.OnReceive += msg =>
            {
                if (msg.MessageType != MessageType.Removed)
                    return false; // not what we want

                if (!msg.ChunkNo.HasValue)
                {
                    Console.WriteLine("ListenRemoved: bad msg, ChunkNo has no value.");
                    return true;
                }

                var chunkNo = msg.ChunkNo.Value;
                var fileId = msg.FileId;

                var key = fileId.ToString() + "_" + chunkNo;
                if (!Core.Instance.Store.ContainsFile(key))
                    return true;

                var fullPath = Path.Combine(dir, key);

                Core.Instance.Store.DecrementActualDegree(key, 0 /* won't be used, dict contains key);
                ReplicationDegrees rd;
                Core.Instance.Store.TryGetDegrees(key, out rd);

                if (rd.ActualDegree < rd.WantedDegree)
                {
                    var cts = new CancellationTokenSource();
                    var token = cts.Token;

                    Task.Factory.StartNew(async () =>
                    {
                        await Task.Delay(rnd.Next(0, 401), token);
                        if (token.IsCancellationRequested)
                            return;

                        var fs = File.OpenRead(fullPath);
                        var buffer = new byte[64000];
                        var bytesRead = fs.Read(buffer, 0, buffer.Length);
                        var data = buffer.Take(bytesRead).ToArray();

                        BackupChunk(fileId, chunkNo, data, rd.WantedDegree);
                    }, token);

                    _mcChannel.OnReceive += msg2 =>
                    {
                        if (msg2.MessageType != MessageType.PutChunk ||
                              msg2.ChunkNo != chunkNo || msg2.FileId != fileId)
                            return false;

                        cts.Cancel();
                        return true;
                    };
                }

                return true;
            };
        }*/

        private static void PrintUsage()
        {
            Console.WriteLine("Usage: {0} <MC:IP> <MC:Port> <MDB:IP> <MDB:PORT> <MDR:IP> <MDR:Port>", Path.GetFileName(Process.GetCurrentProcess().MainModule.FileName));
            Console.WriteLine("  <MC:IP> <MC:Port>: IP multicast address and port of control channel");
            Console.WriteLine("  <MDB:IP> <MDB:Port>: IP multicast address and port of data backup channel");
            Console.WriteLine("  <MDR:IP> <MDR:Port>: IP multicast address and port of data restore channel");
        }
    }
}
