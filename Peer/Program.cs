using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using DBS;
using JsonConfig;
using System.Linq;

/* http://web.fe.up.pt/~pfs/aulas/sd2014/proj1.html */

namespace Peer
{
    static class Program
    {
        static void SendFileInChunks(string fileName, FileEntry fileEntry)
        {
            const int chunkSize = 64000; // read the file in chunks of 64KB
            using (var file = File.OpenRead(fileName))
            {
                int bytesRead, chunkNo = 0;
                var fileSize = file.Length;
                var buffer = new byte[chunkSize];
                while ((bytesRead = file.Read(buffer, 0, buffer.Length)) > 0)
                {
                    var data = buffer.Take(bytesRead).ToArray(); // slice the buffer with bytesRead
                    BackupChunk(fileEntry.FileId, chunkNo, data, fileEntry.ReplicationDegree);
                    ++chunkNo;
                }

                if ((fileSize%chunkSize) == 0) // last chunk with an empty body
                    BackupChunk(fileEntry.FileId, chunkNo, new byte[] {}, fileEntry.ReplicationDegree);
            }
        }

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

            // Create channels
            
            _mcChannel = new Channel(mcIP, mcPort) { Name = "MC" };
            _mdbChannel = new Channel(mdbIP, mdbPort) { Name = "MDB" };
            _mdrChannel = new Channel(mdrIP, mdrPort) { Name = "MDR" };

            // Create dictionary of files to mantain

            var files = new Dictionary<string, FileEntry>();
            foreach (var f in Config.Global.Files)
                files.Add(f.Name, new FileEntry { FileId = new FileId(f.Name), ReplicationDegree = f.ReplicationDegree});

            // Setup directories
            string backupDir = Config.Global.BackupDir;
            string storeDir = Config.Global.StoreDir;

            if (!Directory.Exists(backupDir))
                Directory.CreateDirectory(backupDir);
            if (!Directory.Exists(storeDir))
                Directory.CreateDirectory(storeDir);

            // Start Tasks

            Task.Factory.StartNew(() => StoreFiles(backupDir));
            Task.Factory.StartNew(() => ListenRemoved(backupDir));

            foreach (var f in files)
                Task.Factory.StartNew(() => SendFileInChunks(f.Key, f.Value));

            var spaceRecl = new SpaceReclaimingWatcher(_mcChannel);
            spaceRecl.Run(backupDir);

            Console.ReadKey();
        }

        private static Channel _mcChannel;
        private static Channel _mdbChannel;
        private static Channel _mdrChannel;

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
                if (!PersistentStore.Dict.ContainsKey(key))
                    return true;

                var fullPath = Path.Combine(dir, key);

                PersistentStore.DecrementActualDegree(key, 0 /* won't be used, dict contains key*/);
                ReplicationDegrees rd;
                PersistentStore.TryGetDegrees(key, out rd);

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
        }

        private static void StoreFiles(string dir)
        {
            var rnd = new Random();

            _mdbChannel.OnReceive += msg =>
            {
                if (msg.MessageType != MessageType.PutChunk)
                    return false; // not what we want

                try
                {
                    if (!Directory.Exists(dir))
                        Directory.CreateDirectory(dir);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("StoreFiles: " + ex);
                    return true;
                }

                if (!msg.ChunkNo.HasValue)
                {
                    Console.WriteLine("StoreFiles: bad msg, ChunkNo has no value.");
                    return true;
                }

                var fileName = msg.FileId.ToString() + "_" + msg.ChunkNo;
                var fullPath = Path.Combine(dir, fileName);
                if (!File.Exists(fullPath))
                {
                    try
                    {
                        var fs = File.Create(fullPath);
                        fs.Write(msg.Body, 0, msg.Body.Length);
                        fs.Close();

                        PersistentStore.IncrementActualDegree(fileName, msg.ReplicationDeg.Value);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("StoreFiles: " + ex);
                        return true;
                    }
                }

                Thread.Sleep(rnd.Next(0, 401)); // random delay uniformly distributed between 0 and 400 ms

                _mcChannel.Send(Message.BuildStoredMessage(msg.FileId, msg.ChunkNo.Value));
                return true;
            };
        }

        private static void BackupChunk(FileId fileId, int chunkNo, byte[] data, int repDegree)
        {
            var t = new Func<int, int>(timeout =>
            {
                var cts = new CancellationTokenSource(timeout);
                var token = cts.Token;
                var storeTask = new Task<int>(() =>
                {
                    var receivedMessagesFrom = new HashSet<string>();

                    OnReceive onReceivedStored = msg =>
                    {
                        if (msg.MessageType != MessageType.Stored ||
                              msg.ChunkNo != chunkNo || msg.FileId != fileId)
                            return false;

                        receivedMessagesFrom.Add(msg.RemoteEndPoint.Address.ToString());
                        return true;
                    };

                    try
                    {

                        _mcChannel.OnReceive += onReceivedStored;
                        while (true)
                        {
                            token.ThrowIfCancellationRequested();
                            Thread.Sleep(10);
                        }
                    }
                    catch (OperationCanceledException)
                    {
                        _mcChannel.OnReceive -= onReceivedStored;
                        return receivedMessagesFrom.Count;
                    }
                });

                storeTask.RunSynchronously();
                return storeTask.Result;
            });

            const int initialTimeout = 500;
            const int maxRetries = 5;

            var timeoutValue = initialTimeout;

            var count = 0;
            for (var retryCount = 0; retryCount < maxRetries; retryCount++)
            {
                var msg = Message.BuildPutChunkMessage(fileId, chunkNo, repDegree, data);
                _mdbChannel.Send(msg);

                count = t(timeoutValue);
                if (count < repDegree)
                {
                    timeoutValue *= 2;
                    Console.WriteLine("Replication degree is {0} but wanted {1}. Timeout increased to {2}", count, repDegree, timeoutValue);
                }
                else
                {
                    Console.WriteLine("Stored or giving up: retries {0}, rep degree {1}", retryCount, count);
                    break;
                }
            }

            PersistentStore.UpdateDegrees(fileId + "_" + chunkNo, count, repDegree);
        }


        private static void PrintUsage()
        {
            Console.WriteLine("Usage: {0} <MC:IP> <MC:Port> <MDB:IP> <MDB:PORT> <MDR:IP> <MDR:Port>", Path.GetFileName(Process.GetCurrentProcess().MainModule.FileName));
            Console.WriteLine("  <MC:IP> <MC:Port>: IP multicast address and port of control channel");
            Console.WriteLine("  <MDB:IP> <MDB:Port>: IP multicast address and port of data backup channel");
            Console.WriteLine("  <MDR:IP> <MDR:Port>: IP multicast address and port of data restore channel");
        }
    }
}
