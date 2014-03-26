﻿using System;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Reactive.Linq;
using System.Threading;
using System.Windows.Forms;
using DBS;
using JsonConfig;

namespace PeerGUI
{
    public partial class Form1 : Form
    {
        private readonly CommandSwitch _commandSwitch = new CommandSwitch();

        public Form1()
        {
            InitializeComponent();
            Core.Instance.Log.ObserveOn(SynchronizationContext.Current).
                Subscribe(s => logTextBox.AppendText(s + Environment.NewLine));

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
                    randomDelayMin, randomDelayMax, backupDirectory, restoreDirectory);
                Core.Instance.Config = config;
            }
            catch (Exception ex)
            {
                Core.Instance.Log.Error("config setup", ex);
                return;
            }

            Core.Instance.Start(false);
        }

        private void addFileButton_Click(object sender, EventArgs e)
        {
            var res = openFileDialog.ShowDialog();
            if (res != DialogResult.OK) return;
            var fileNames = openFileDialog.FileNames;
            foreach (var fileName in fileNames)
            {
                var listViewItem1 = new ListViewItem(new []
                {
                    fileName,
                    "1",
                    NOT_BACKED_UP
                }, -1);

                filesListView.Items.Add(listViewItem1);
            }
        }

        private void removeFileButton_Click(object sender, EventArgs e)
        {
            if (filesListView.SelectedItems.Count == 0)
                return;

            filesListView.Items.Remove(filesListView.SelectedItems[0]);
        }

        private void filesListView_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            if (!e.IsSelected)
            {
                backupButton.Enabled = false;
                restoreButton.Enabled = false;
                deleteButton.Enabled = false;
                incDegreeButton.Enabled = false;
                decDegreeButton.Enabled = false;
            }
            else
            {
                if (e.Item.SubItems[2].Text == BACKED_UP)
                {
                    backupButton.Enabled = false;
                    restoreButton.Enabled = true;
                    deleteButton.Enabled = true;
                    incDegreeButton.Enabled = false;
                    decDegreeButton.Enabled = false;
                }
                else if (e.Item.SubItems[2].Text == RESTORED)
                {
                    backupButton.Enabled = false;
                    restoreButton.Enabled = true;
                    deleteButton.Enabled = true;
                    incDegreeButton.Enabled = false;
                    decDegreeButton.Enabled = false;
                }
                else if (e.Item.SubItems[2].Text == NOT_BACKED_UP)
                {
                    backupButton.Enabled = true;
                    restoreButton.Enabled = false;
                    deleteButton.Enabled = false;
                    incDegreeButton.Enabled = true;
                    decDegreeButton.Enabled = true;
                }
            }
        }

        private void incDegreeButton_Click(object sender, EventArgs e)
        {
            if (filesListView.SelectedItems.Count == 0)
                return;

            var selectedItem = filesListView.SelectedItems[0];
            var currentDegree = int.Parse(selectedItem.SubItems[1].Text);
            if (currentDegree == 9)
                return;

            selectedItem.SubItems[1].Text = (currentDegree + 1).ToString(CultureInfo.InvariantCulture);
        }

        private void decDegreeButton_Click(object sender, EventArgs e)
        {
            if (filesListView.SelectedItems.Count == 0)
                return;

            var selectedItem = filesListView.SelectedItems[0];
            var currentDegree = int.Parse(selectedItem.SubItems[1].Text);
            if (currentDegree == 1)
                return;

            selectedItem.SubItems[1].Text = (currentDegree - 1).ToString(CultureInfo.InvariantCulture);
        }

        private void backupButton_Click(object sender, EventArgs e)
        {
            if (filesListView.SelectedItems.Count == 0)
                return;

            backupButton.Enabled = false;
            restoreButton.Enabled = true;
            deleteButton.Enabled = true;
            incDegreeButton.Enabled = false;
            decDegreeButton.Enabled = false;

            var fileName = filesListView.SelectedItems[0].Text;
            var replicationDegree = int.Parse(filesListView.SelectedItems[0].SubItems[1].Text);
            var fileEntry = Core.Instance.AddBackupFile(fileName, replicationDegree);

            if (fileEntry != null)
            {
                _commandSwitch.Execute(new BackupFileCommand(fileEntry));
                filesListView.SelectedItems[0].SubItems[2].Text = BACKED_UP;
            }
        }

        private void restoreButton_Click(object sender, EventArgs e)
        {
            if (filesListView.SelectedItems.Count == 0)
                return;

            if (Core.Instance.BackupFiles.Count == 0)
                return;

            var fileName = filesListView.SelectedItems[0].Text;
            var files = Core.Instance.BackupFiles
                .Where(backupFile => backupFile.OriginalFileName == fileName).ToList();
            if (files.Count != 1)
                return;

            _commandSwitch.Execute(new RestoreFileCommand(files[0]));
            filesListView.SelectedItems[0].SubItems[2].Text = RESTORED;
        }

        private void deleteButton_Click(object sender, EventArgs e)
        {
            if (filesListView.SelectedItems.Count == 0)
                return;

            if (Core.Instance.BackupFiles.Count == 0)
                return;

            var fileName = filesListView.SelectedItems[0].Text;
            var files = Core.Instance.BackupFiles
                .Where(backupFile => backupFile.OriginalFileName == fileName).ToList();
            if (files.Count != 1)
                return;

            _commandSwitch.Execute(new DeleteFileCommand(files[0]));
            filesListView.SelectedItems[0].SubItems[2].Text = NOT_BACKED_UP;

            backupButton.Enabled = true;
            restoreButton.Enabled = false;
            deleteButton.Enabled = false;
            incDegreeButton.Enabled = true;
            decDegreeButton.Enabled = true;
        }

        private const string BACKED_UP = "backed up";
        private const string NOT_BACKED_UP = "not backed up";
        private const string RESTORED = "restored";

        private void reclaimButton_Click(object sender, EventArgs e)
        {
            _commandSwitch.Execute(new SpaceReclaimingCommand());
        }
    }
}