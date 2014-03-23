﻿using System;
using System.Collections.Generic;
using System.Net;
using DBS.Persistence;
using DBS.Protocols;

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
        public int ChunkSize { get; set; }

        public Channel MCChannel { get; set; } // you can't touch this
        public Channel MDBChannel { get; set; }
        public Channel MDRChannel { get; set; }

        public Dictionary<string, FileEntry> BackupFiles { get; private set; }
        public string BackupDirectory { get; set; }

        public void Start()
        {
            new BackupChunkService().Start(); // 3.2 Chunk backup subprotocol
            new RestoreChunkService().Start(); // 3.3 Chunk restore protocol
            new DeleteFileService().Start(); // 3.4 File deletion subprotocol
            new SpaceReclaimingService().Start(); // 3.5 Space reclaiming subprotocol
            new SpaceReclaimingWatcher().Start();

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
