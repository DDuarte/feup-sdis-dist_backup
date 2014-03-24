using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
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

        public void AddBackupFile(string fileName, int replicationDegree)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                throw new ArgumentNullException("fileName");

            if (!File.Exists(fileName))
                throw new ArgumentException("File '{0}' does not exist.".FormatWith(fileName), "fileName");

            // get a unique filename from the original file path
            // if the new filename already exists, append a number to it
            var newFileName = Path.GetFileName(fileName) + "_";
            var count = 1;
            while (BackupFiles.Any(entry => entry.FileName == newFileName))
            {
                var countStr = count.ToString(CultureInfo.InvariantCulture);
                newFileName = newFileName.Remove(newFileName.Length - countStr.Length);
                newFileName += countStr;
                count++;
            }

            if (count == 1) // no changes
                newFileName = newFileName.Remove(newFileName.Length - 1); // the underscore

            var fileEntry = new FileEntry
            {
                FileId = FileId.FromFile(fileName),
                FileName = newFileName,
                OriginalFileName = fileName,
                ReplicationDegree = replicationDegree
            };

            BackupFiles.Add(fileEntry);
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

            Console.WriteLine("Write 'quit' to exit application.");
            string str;
            do
            {
                str = Console.ReadLine();
            } while (str != null && str != "quit");
        }
    }
}
