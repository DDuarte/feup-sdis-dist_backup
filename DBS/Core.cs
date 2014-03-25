using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using DBS.Persistence;
using DBS.Protocols;
using DBS.Utilities;

namespace DBS
{
    public sealed class Core
    {
        private static readonly Lazy<Core> Lazy = new Lazy<Core>(() => new Core());

        public static Core Instance { get { return Lazy.Value; } }

        private Core()
        {
            Store = new PersistentStore();
            Rnd = new Random();
            BackupFiles = new HashSet<FileEntry>(new FileEntry.Comparer());
        }

        public PersistentStore Store { get; private set; }
        private Random Rnd { get; set; }

        public IPAddress LocalIP { get; set; }

        public int MaxBackupSize { get; set; }
        public int ChunkSize { get; set; }

        public int BackupChunkTimeout { get; set; }
        public double BackupChunkTimeoutMultiplier { get; set; }
        public int BackupChunkRetries { get; set; }

        public int VersionM { get; set; }
        public int VersionN { get; set; }

        public Channel MCChannel { get; set; }
        public Channel MDBChannel { get; set; }
        public Channel MDRChannel { get; set; }

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
            while (BackupFiles.Any(entry => entry.FileName == newFileName))
            {
                var ext = Path.GetExtension(newFileName);
                var name = Path.GetFileNameWithoutExtension(newFileName);

                newFileName = name + '_' + count + ext;
                count++;
            }

            var fileEntry = new FileEntry
            {
                FileId = FileId.FromFile(fileName),
                FileName = newFileName,
                OriginalFileName = fileName,
                ReplicationDegree = replicationDegree
            };

            BackupFiles.Add(fileEntry);
            return fileEntry;
        }

        public HashSet<FileEntry> BackupFiles { get; private set; }
        public string BackupDirectory { get; set; }
        public string RestoreDirectory { get; set; }

        public int RandomDelayMin { get; set; }
        public int RandomDelayMax { get; set; }
        public int RandomDelay { get { return Rnd.Next(RandomDelayMin, RandomDelayMax + 1); } }

        public void Start()
        {
            new BackupChunkService().Start(); // 3.2 Chunk backup subprotocol
            new RestoreChunkService().Start(); // 3.3 Chunk restore protocol
            new DeleteFileService().Start(); // 3.4 File deletion subprotocol
            new SpaceReclaimingService().Start(); // 3.5 Space reclaiming subprotocol
            //new SpaceReclaimingWatcher().Start();

            foreach (var backupFile in BackupFiles)
            {
                new BackupFileProtocol(backupFile).Run();
            }

            var swt = new CommandSwitch();

            Console.WriteLine("Write 'quit' to exit application.");
            while (true)
            {
                var str = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(str))
                    continue;

                var parts = str.Split(' ');
                var cmd = parts[0].Trim();
                cmd = cmd.ToLower();
                if (cmd == "quit")
                    return;
                else if (cmd == "backup")
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
                else if (cmd == "restore")
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

                    Console.WriteLine("Will restore file '{0}'", fileEntry.FileName);
                    swt.Execute(new RestoreFileCommand(fileEntry));
                }
                else if (cmd == "delete")
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
                else if (cmd == "reclaim")
                {
                    Console.WriteLine("Launching space reclaming algorithm.");
                    swt.Execute(new SpaceReclaimingCommand());
                }
                else
                {
                    Console.WriteLine("Unknown command.");
                }
            }
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
                return Regex.IsMatch(entry.FileName, pattern);
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
                    Console.WriteLine("- {0}", possibleFile.FileName);
                }

                var chosenFile = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(chosenFile))
                    return null;

                possibleFiles.RemoveAll(entry => entry.FileName != chosenFile);
                if (possibleFiles.Count == 1)
                    break;
            }

            return possibleFiles[0];
        }
    }
}
