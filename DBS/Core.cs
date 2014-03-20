using System;
using System.Collections.Generic;
using System.Net;

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
            BackupFiles = new Dictionary<string, FileEntry>();
        }

        public PersistentStore Store { get; private set; }
        public Random Rnd { get; private set; }

        public IPAddress LocalIP { get; set; }

        public int MaxBackupSize { get; set; }

        public Channel MCChannel { get; set; } // you can't touch this
        public Channel MDBChannel { get; set; }
        public Channel MDRChannel { get; set; }

        public Dictionary<string, FileEntry> BackupFiles { get; private set; }
        public string BackupDirectory { get; set; }

        public void Start()
        {
            new BackupFileService().Start();
            new FileDeletionService().Start();

            foreach (var backupFile in BackupFiles)
            {
                new BackupFileProtocol(backupFile.Key, backupFile.Value).Run();
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
