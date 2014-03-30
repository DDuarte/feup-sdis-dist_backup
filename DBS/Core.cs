using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using DBS.Persistence;
using DBS.Protocols;
using DBS.Protocols.Enhancements;
using DBS.Utilities;

/* http://web.fe.up.pt/~pfs/aulas/sd2014/proj1.html */

namespace DBS
{
    public sealed class Core
    {
        public class Settings
        {
            public Settings(IPAddress localIP, long maxBackupSize, int chunkSize,
                int backupChunkTimeout, double backupChunkTimeoutMultiplier,
                int backupChunkRetries, int versionM, int versionN, int randomDelayMin,
                int randomDelayMax, string backupDirectory, string restoreDirectory,
                IPAddress mcIP, int mcPort, IPAddress mdbIP, int mdbPort,
                IPAddress mdrIP, int mdrPort)
            {
                Utilities.Utilities.CreateDirectoryIfNotExists(backupDirectory);
                Utilities.Utilities.CreateDirectoryIfNotExists(restoreDirectory);

                LocalIP = localIP;
                MaxBackupSize = maxBackupSize;
                ChunkSize = chunkSize;
                BackupChunkTimeout = backupChunkTimeout;
                BackupChunkTimeoutMultiplier = backupChunkTimeoutMultiplier;
                BackupChunkRetries = backupChunkRetries;
                VersionM = versionM;
                VersionN = versionN;
                RandomDelayMin = randomDelayMin;
                RandomDelayMax = randomDelayMax;
                BackupDirectory = backupDirectory;
                RestoreDirectory = restoreDirectory;

                if (!NetworkUtilities.IsMulticastAddress(mcIP))
                    throw new ArgumentException("MC:IP is not a multicast address", "mcIP");
                if (!NetworkUtilities.IsMulticastAddress(mdbIP))
                    throw new ArgumentException("MDB:IP is not a multicast address", "mdbIP");
                if (!NetworkUtilities.IsMulticastAddress(mdrIP))
                    throw new ArgumentException("MDR:IP is not a multicast address", "mdrIP");

                if (!localIP.Equals(IPAddress.Any) && !NetworkUtilities.GetLocalIPAddresses().Contains(localIP))
                    throw new ArgumentException("LocalIP is not a local IP address", "localIP");

                MCIP = mcIP;
                MCPort = mcPort;
                MDBIP = mdbIP;
                MDBPort = mdbPort;
                MDRIP = mdrIP;
                MDRPort = mdrPort;
            }

            public Settings(IPAddress localIP, long maxBackupSize, int chunkSize,
                int backupChunkTimeout, double backupChunkTimeoutMultiplier,
                int backupChunkRetries, int versionM, int versionN, int randomDelayMin,
                int randomDelayMax, string backupDirectory, string restoreDirectory) :
                    this(localIP, maxBackupSize, chunkSize, backupChunkTimeout, backupChunkTimeoutMultiplier,
                        backupChunkRetries, versionM, versionN, randomDelayMin, randomDelayMax,
                        backupDirectory, restoreDirectory, IPAddress.Parse("225.0.0.10"), 31000,
                        IPAddress.Parse("225.0.0.10"), 31001, IPAddress.Parse("225.0.0.10"), 31002)
            {
            }

            public IPAddress LocalIP { get; private set; }
            public long MaxBackupSize { get; set; }
            public int ChunkSize { get; private set; }
            public int BackupChunkTimeout { get; private set; }
            public double BackupChunkTimeoutMultiplier { get; private set; }
            public int BackupChunkRetries { get; private set; }
            public int VersionM { get; private set; }
            public int VersionN { get; private set; }
            public int RandomDelayMin { get; private set; }
            public int RandomDelayMax { get; private set; }
            public string BackupDirectory { get; private set; }
            public string RestoreDirectory { get; private set; }
            public IPAddress MCIP { get; private set; }
            public int MCPort { get; private set; }
            public IPAddress MDBIP { get; private set; }
            public int MDBPort { get; private set; }
            public IPAddress MDRIP { get; private set; }
            public int MDRPort { get; private set; }
        }

        private static readonly Lazy<Core> Lazy = new Lazy<Core>(() => new Core());

        public static Core Instance { get { return Lazy.Value; } }

        private Core()
        {
            ChunkPeers = new PersistentChunkPeers();
            Rnd = new Random();
            BackupFiles = new PersistentDictionary<string, FileEntry>("backupfiles", "backupfiles");
            Log = new ObservableLog();
            RunningServices = new List<IService>();
        }

        public PersistentChunkPeers ChunkPeers { get; private set; }
        private Random Rnd { get; set; }
        public ObservableLog Log { get; private set; }

        public Channel MCChannel { get; private set; }
        public Channel MDBChannel { get; private set; }
        public Channel MDRChannel { get; private set; }

        public PersistentDictionary<string, FileEntry> BackupFiles { get; private set; }

        private Settings _config;

        public Settings Config
        {
            get { return _config; }
            set
            {
                _config = value;
                SetupChannels();
            }
        }

        public int RandomDelay { get { return Rnd.Next(Config.RandomDelayMin, Config.RandomDelayMax + 1); } }

        private void SetupChannels()
        {
            MCChannel = new Channel(Config.MCIP, Config.MCPort) { Name = "MC" };
            MDBChannel = new Channel(Config.MDBIP, Config.MDBPort) { Name = "MDB" };
            MDRChannel = new Channel(Config.MDRIP, Config.MDRPort) { Name = "MDR" };
        }

        public FileEntry AddBackupFile(string fileName /* C:/Windows/write.exe */, int replicationDegree)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                throw new ArgumentNullException("fileName");

            if (!File.Exists(fileName))
                return null;

            // get a unique filename from the original file path
            // if the new filename already exists, append a number to it
            var fileNameWithoutPath = Path.GetFileName(fileName); /* write.exe */
            var newFileName = fileNameWithoutPath;
            var count = 1;
            while (BackupFiles.ContainsKey(newFileName))
            {
                var ext = Path.GetExtension(newFileName);
                var name = Path.GetFileNameWithoutExtension(newFileName);

                newFileName = name + '_' + count + ext;
                count++;
            }

            var fileEntry = new FileEntry
            {
                FileName = newFileName,
                OriginalFileName = fileName,
                ReplicationDegree = replicationDegree
            };

            BackupFiles.Add(newFileName, fileEntry);
            return fileEntry;
        }

        public void Start(bool console = true)
        {
            if (_config == null)
                throw new Exception("Called Start() without setting config.");

            if (!console)
                return;

            var swt = new CommandSwitch();

            Console.WriteLine("Write 'quit' to exit application.");
            while (true)
            {
                var str = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(str))
                    continue;

                var parts = str.Split(' ');
                var cmd = parts[0].ToLower().Trim();
                switch (cmd)
                {
                    case "quit":
                        return;
                    case "backup":
                    {
                        if (parts.Length != 3)
                        {
                            Console.WriteLine("Wrong number of arguments.");
                            continue;
                        }

                        var fileName = parts[1];
                        var repDegree = int.Parse(parts[2]);

                        var fileEntry = AddBackupFile(fileName, repDegree);
                        if (fileEntry == null)
                        {
                            Console.WriteLine("Tried to backup unknown file.");
                            continue;
                        }

                        Console.WriteLine("Will backup file '{0}'", fileEntry.FileName);
                        swt.Execute(new BackupFileCommand(fileEntry));
                    }
                        break;
                    case "restore":
                        switch (parts.Length)
                        {
                            case 2:
                            {
                                var fileName = parts[1];
                                var fileEntry = ConsoleGetFileEntry(fileName);
                                if (fileEntry == null)
                                {
                                    Console.WriteLine("Wrong file name provided.");
                                    continue;
                                }

                                Console.WriteLine("Will restore file '{0}'", fileEntry.FileName);
                                swt.Execute(new RestoreFileCommand(fileEntry));
                            }
                                break;
                            case 3:
                            {
                                var fileName = parts[1];
                                var flag = parts[2];
                                var fileEntry = ConsoleGetFileEntry(fileName);
                                if (fileEntry == null)
                                {
                                    Console.WriteLine("Wrong file name provided.");
                                    continue;
                                }

                                if (flag.ToLower() != "-e")
                                {
                                    Console.WriteLine("Invalid flag specified");
                                    continue;
                                }

                                Console.WriteLine("Will restore file '{0}'", fileEntry.FileName);
                                swt.Execute(new RestoreFileCommand(fileEntry, true));
                            }
                                break;
                            default:
                                Console.WriteLine("Wrong number of arguments.");
                                break;
                        }
                        break;
                    case "delete":
                    {
                        if (parts.Length != 2)
                        {
                            Console.WriteLine("Wrong number of arguments.");
                            continue;
                        }

                        var fileName = parts[1];
                        var fileEntry = ConsoleGetFileEntry(fileName);
                        if (fileEntry == null)
                        {
                            Console.WriteLine("Wrong file name provided.");
                            continue;
                        }

                        Console.WriteLine("Will delete file '{0}'", fileEntry.FileName);
                        swt.Execute(new DeleteFileCommand(fileEntry));
                    }
                        break;
                    case "reclaim":
                        Console.WriteLine("Launching space reclaming algorithm.");
                        swt.Execute(new SpaceReclaimingCommand());
                        break;
                    default:
                        Console.WriteLine("Unknown command.");
                        break;
                }
            }
        }

        private List<IService> RunningServices { get; set; }

        public void StartServices(bool enhanced)
        {
            if (enhanced)
            {
                RunningServices.AddRange(new IService[]
                {
                    new BackupChunkService(), // 3.2 Chunk backup subprotocol
                    new BackupChunkStoredService(), // 3.2 Chunk backup subprotocol
                    new EnhancedRestoreChunkACKService(), // 3.3 Chunk ENH restore protocol
                    new EnhancedRestoreChunkConnInfoService(), // 3.3 Chunk ENH restore protocol
                    new DeleteFileService(), // 3.4 File deletion subprotocol
                    new LookUpService(), // 3.4 File ENH deletion subprotocol
                    new EnhancedSpaceReclaimingWatcher(), // 3.4 File ENH deletion subprotocol
                    new SpaceReclaimingService() // 3.5 Space reclaiming subprotocol
                });
            }
            else
            {
                RunningServices.AddRange(new IService[]
                {
                    new BackupChunkService(), // 3.2 Chunk backup subprotocol
                    new BackupChunkStoredService(), // 3.2 Chunk backup subprotocol
                    new RestoreChunkService(), // 3.3 Chunk restore protocol
                    new DeleteFileService(), // 3.4 File deletion subprotocol
                    new SpaceReclaimingService() // 3.5 Space reclaiming subprotocol
                });
            }

            foreach (var runningService in RunningServices)
                runningService.Start();

            foreach (var backupFile in BackupFiles)
                new BackupFileProtocol(backupFile.Value).Run();
        }

        public void StopServices()
        {
            foreach (var runningService in RunningServices)
                runningService.Stop();
            RunningServices.Clear();
        }

        private FileEntry ConsoleGetFileEntry(string fileName)
        {
            var possibleFiles = BackupFiles.Where(entry =>
            {
                var name = Path.GetFileNameWithoutExtension(fileName);
                var ext = Path.GetExtension(fileName);

                var pattern = name + @"(_[0-9]+)?";
                if (!string.IsNullOrEmpty(ext))
                    pattern += '\\'+ ext; // must escape .
                return Regex.IsMatch(entry.Key, pattern);
            }).ToList();
            if (possibleFiles.Count == 0)
            {
                Console.WriteLine("File not found.");
                return null;
            }

            while (possibleFiles.Count > 1)
            {
                Console.WriteLine("Which file do you want?");
                foreach (var possibleFile in possibleFiles)
                {
                    Console.WriteLine("- {0}", possibleFile.Key);
                }

                var chosenFile = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(chosenFile))
                    return null;

                possibleFiles.RemoveAll(entry => entry.Key != chosenFile);
                if (possibleFiles.Count == 1)
                    break;
            }

            return possibleFiles[0].Value;
        }
    }
}
