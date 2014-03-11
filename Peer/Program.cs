using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using DBS;
using JsonConfig;
using System.Net.Sockets;
using System.Linq;

namespace Peer
{
    static class Program
    {
        static void SendFileInChunks(Channel channel, Tuple<string, FileEntry> fileInfo, Func<int> chunkIntervalDist)
        {
            const int chunkSize = 64000; // read the file in chunks of 64KB
            using (var file = File.OpenRead(fileInfo.Item1))
            {
                int bytesRead, chunkNo = 0;
                var fileSize = file.Length;
                var buffer = new byte[chunkSize];
                while ((bytesRead = file.Read(buffer, 0, buffer.Length)) > 0)
                {
                    var data = buffer.Take(bytesRead).ToArray(); // slice the buffer with bytesRead
                    channel.Send(Message.BuildPutChunkMessage(1, 0, fileInfo.Item2.FileId, chunkNo, fileInfo.Item2.ReplicationDegree, data));
                    ++chunkNo;

                    System.Threading.Thread.Sleep(chunkIntervalDist());
                }
                
                if((fileSize % chunkSize) == 0)
                    channel.Send(Message.BuildPutChunkMessage(1, 0, fileInfo.Item2.FileId, chunkNo, fileInfo.Item2.ReplicationDegree, new byte[] {})); // last chunk with an empty body
            }
        }

        static void Main(string[] args)
        {
            if (args.Length != 6)
            {
                PrintUsage();
                Console.ReadKey();
                return;
            }

            // Get console arguments

            var mcIP = IPAddress.Parse(args[0]);
            var mcPort = int.Parse(args[1]);

            var mdbIP = IPAddress.Parse(args[2]);
            var mdbPort = int.Parse(args[3]);
            
            var mdrIP = IPAddress.Parse(args[4]);
            var mdrPort = int.Parse(args[5]);
            
            // Create channels
            
            var mcChannel = new Channel(mcIP, mcPort) {Name = "MC"};
            var mdbChannel = new Channel(mdbIP, mdbPort) {Name = "MDB"};
            var mdrChannel = new Channel(mdrIP, mdrPort) {Name = "MDR"};
            
            // Join multicast groups
            
            mcChannel.JoinMulticast();
            mdbChannel.JoinMulticast();
            mdrChannel.JoinMulticast();

            var files = new Dictionary<string, FileEntry>();
            foreach (var f in Config.Global.Files)
                files.Add(f.Name, new FileEntry { FileId = FileIdGenerator.Build(f.Name), ReplicationDegree = f.ReplicationDegree});

            foreach (var f in files)
            {
                mdbChannel.Send(Message.BuildPutChunkMessage(1, 0, f.Value.FileId, chunkNo, f.Value.ReplicationDegree, data));
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
