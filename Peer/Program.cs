using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using DBS;
using DBS.Utilities;
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
            IPAddress localIP;
            if (IPAddress.TryParse(Config.Global.LocalIP, out localIP))
            {
                if (localIP.Equals(IPAddress.Any) || NetworkUtilities.GetLocalIPAddresses().Contains(localIP))
                    Core.Instance.LocalIP = localIP;
                else
                {
                    Console.WriteLine("Config LocalIP '{0}' is not a local IP address",  localIP);
                    return;
                }
            }
            else
            {
                Console.WriteLine("Config LocalIP '{0}' is not a valid IP address.", Config.Global.LocalIP);
                return;
            }
            
            Core.Instance.LocalIP = IPAddress.Parse(Config.Global.LocalIP);

            // Max site used to backup (locally)
            Core.Instance.MaxBackupSize = Config.Global.DiskSpace;

            // Size of each chunk stored locally and sent over the network
            Core.Instance.ChunkSize = Config.Global.ChunkSize;

            // Backup chunk protocol configurations
            Core.Instance.BackupChunkTimeout = Config.Global.BackupChunkTimeout;
            Core.Instance.BackupChunkTimeoutMultiplier = Config.Global.BackupChunkTimeoutMultiplier;
            Core.Instance.BackupChunkRetries = Config.Global.BackupChunkRetries;

            // Protocol version
            Core.Instance.VersionM = Config.Global.Version.M;
            Core.Instance.VersionN = Config.Global.Version.N;

            // Random delay used in multiple protocols
            Core.Instance.RandomDelayMin = Config.Global.RandomDelay.Min;
            Core.Instance.RandomDelayMax = Config.Global.RandomDelay.Max;

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
