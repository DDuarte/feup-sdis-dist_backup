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

            IPAddress localIP;
            if (!IPAddress.TryParse(Config.Global.LocalIP, out localIP))
            {
                Console.WriteLine("Config LocalIP '{0}' is not a valid IP address.", Config.Global.LocalIP);
                return;
            }

            int maxBackupSize = Config.Global.DiskSpace; // Max site used to backup (locally)
            int chunkSize = Config.Global.ChunkSize; // Size of each chunk stored locally and sent over the network

            // Backup chunk protocol configurations
            int backupChunkTimeout = Config.Global.BackupChunkTimeout;
            double backupChunkTimeoutMultiplier = Config.Global.BackupChunkTimeoutMultiplier;
            int backupChunkRetries = Config.Global.BackupChunkRetries;

            // Protocol version
            int versionM = Config.Global.Version.M;
            int versionN = Config.Global.Version.N;

            // Random delay used in multiple protocols
            int randomDelayMin = Config.Global.RandomDelay.Min;
            int randomDelayMax = Config.Global.RandomDelay.Max;

            // Setup directories
            string backupDirectory = Config.Global.BackupDir;
            string restoreDirectory = Config.Global.RestoreDir;

            try
            {
                var config = new Core.Settings(localIP, maxBackupSize, chunkSize, backupChunkTimeout,
                    backupChunkTimeoutMultiplier, backupChunkRetries, versionM, versionN,
                    randomDelayMin, randomDelayMax, backupDirectory, restoreDirectory,
                    mcIP, mcPort, mdbIP, mdbPort, mdrIP, mdrPort);
                Core.Instance.Config = config;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return;
            }

            Core.Instance.Log.Subscribe(Console.WriteLine);
            Core.Instance.Start();
        }

        private static void PrintUsage()
        {
            Console.WriteLine("Usage: {0} <MC:IP> <MC:Port> <MDB:IP> <MDB:PORT> <MDR:IP> <MDR:Port>", Path.GetFileName(Process.GetCurrentProcess().MainModule.FileName));
            Console.WriteLine("  <MC:IP> <MC:Port>: IP multicast address and port of control channel;");
            Console.WriteLine("  <MDB:IP> <MDB:Port>: IP multicast address and port of data backup channel;");
            Console.WriteLine("  <MDR:IP> <MDR:Port>: IP multicast address and port of data restore channel.");
            Console.WriteLine("Extra configurations in settings.conf file.");
        }
    }
}
