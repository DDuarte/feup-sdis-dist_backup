using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using DBS;
using JsonConfig;
using System.Linq;

/* http://web.fe.up.pt/~pfs/aulas/sd2014/proj1.html */

namespace Peer
{
    static class Program
    {
        static void SendFileInChunks(IChannel channel, string fileName, FileEntry fileInfo)
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
                    channel.Send(Message.BuildPutChunkMessage(fileInfo.FileId, chunkNo, fileInfo.ReplicationDegree, data));
                    ++chunkNo;
                }

                if ((fileSize % chunkSize) == 0) // last chunk with an empty body
                    channel.Send(Message.BuildPutChunkMessage(fileInfo.FileId, chunkNo, fileInfo.ReplicationDegree, new byte[] {}));
            }
        }

        static void Main(string[] args)
        {
            if (args.Length == 0)
                args = new[] {"225.0.0.1", "31000", "225.0.0.1", "31001", "225.0.0.1", "31002"}; // defaults

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
            
            IChannel mcChannel = new Channel(mcIP, mcPort) { Name = "MC" };
            IChannel mdbChannel = new Channel(mdbIP, mdbPort) { Name = "MDB" };
            IChannel mdrChannel = new Channel(mdrIP, mdrPort) { Name = "MDR" };
            
            // Join multicast groups
            
            mcChannel.JoinMulticast();
            mdbChannel.JoinMulticast();
            mdrChannel.JoinMulticast();

            var files = new Dictionary<string, FileEntry>();
            foreach (var f in Config.Global.Files)
                files.Add(f.Name, new FileEntry { FileId = FileIdGenerator.Build(f.Name), ReplicationDegree = f.ReplicationDegree});

            foreach (var f in files)
            {
                SendFileInChunks(mdbChannel, f.Key, f.Value);
            }

            Console.ReadKey();
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
