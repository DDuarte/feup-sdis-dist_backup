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
        static void SendFileInChunks(IChannel channel, string fileName, FileEntry fileInfo, Func<int, bool> f)
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
                    var msg = Message.BuildPutChunkMessage(fileInfo.FileId, chunkNo, fileInfo.ReplicationDegree, data);

                    do
                    {
                        channel.Send(msg);
                    } while (!f(chunkNo));
                    ++chunkNo;
                }

                if ((fileSize%chunkSize) == 0) // last chunk with an empty body
                {
                    var msg = Message.BuildPutChunkMessage(fileInfo.FileId, chunkNo, fileInfo.ReplicationDegree, new byte[] {});
                    do
                    {
                        channel.Send(msg);
                    } while (!f(chunkNo));
                }
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

            var files = new Dictionary<string, FileEntry>();
            foreach (var f in Config.Global.Files)
                files.Add(f.Name, new FileEntry { FileId = FileIdGenerator.Build(f.Name), ReplicationDegree = f.ReplicationDegree});


            Task.Factory.StartNew(() => StoreFiles("backup"));

            foreach (var f in files)
                BackupFile(f.Key, f.Value);

            Console.ReadKey();
        }

        private static Channel _mcChannel;
        private static Channel _mdbChannel;
        private static Channel _mdrChannel;

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

                if (!msg.ChunkNo.HasValue) // TODO: error
                    return true;

                var fileName = FileIdGenerator.FileIdToString(msg.FileId) + "_" + msg.ChunkNo;
                var fullPath = Path.Combine(dir, fileName);
                if (!File.Exists(fullPath))
                {
                    try
                    {
                        var fs = File.Create(fullPath);
                        fs.Write(msg.Body, 0, msg.Body.Length);
                        fs.Close();
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

        private static void BackupFile(string fileName, FileEntry fileEntry)
        {
            const int initialTimeout = 500;
            var timeoutValue = initialTimeout;
            const int maxRetries = 5;
            var retryCount = 0;

            var oldChunkNo = -1;

            SendFileInChunks(_mdbChannel, fileName, fileEntry, chunkNo =>
            {
                if (oldChunkNo != chunkNo)
                {
                    timeoutValue = initialTimeout;
                    retryCount = 0;
                }
                oldChunkNo = chunkNo;

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
                                  msg.ChunkNo != chunkNo ||
                                  !msg.FileId.SequenceEqual(fileEntry.FileId))
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

                retryCount++;
                var count = t(timeoutValue);
                if (count < fileEntry.ReplicationDegree && retryCount < maxRetries)
                {
                    timeoutValue *= 2;
                    Console.WriteLine("Replication degree is {0} but wanted {1}. Timeout increased to {2}", count, fileEntry.ReplicationDegree, timeoutValue);
                    return false;
                }

                Console.WriteLine("Stored or giving up: retries {0}, rep degree {1}", retryCount, count);
                fileEntry.ActualReplicationDegree = count;

                return true;
            });

            Console.WriteLine("File back'ed up");
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
