using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Reactive;
using System.Threading;
using System.Windows.Forms;
using DBS;
using JsonConfig;

namespace PeerGUI
{
    public partial class Form1 : Form
    {
        private readonly CommandSwitch _commandSwitch = new CommandSwitch();

        private static void AppendTextBox(TextBoxBase tb, string s)
        {
            try
            {
                tb.AppendText(s + Environment.NewLine);
            }
            catch (ObjectDisposedException)
            {
                // FIXME: properly dispose subscribers created in the Form1() cctor
            }
        }

        public Form1()
        {
            InitializeComponent();

            var sc = SynchronizationContext.Current;
            var appendAll = new Action<string>(s => AppendTextBox(allLogTextBox, s));
            var appendTransmitted = new Action<string>(s => AppendTextBox(transmittedTextBox, s));
            var appendReceived = new Action<string>(s => AppendTextBox(receivedLogTextBox, s));
            var appendSent = new Action<string>(s => AppendTextBox(sentLogTextBox, s));
            var appendErrors = new Action<string>(s => AppendTextBox(errorsLogTextBox, s));
            var appendInfo = new Action<string>(s => AppendTextBox(infoLogTextBox, s));

            Core.Instance.Log.SubscribeCustomOn("receive", Observer.Create<string>(s =>
            {
                appendTransmitted(s);
                appendReceived(s);
                appendAll(s);
            }), sc);
            Core.Instance.Log.SubscribeCustomOn("send", Observer.Create<string>(s =>
            {
                appendTransmitted(s);
                appendSent(s);
                appendAll(s);
            }), sc);
            Core.Instance.Log.SubscribeErrorOn(Observer.Create<string>(s =>
            {
                appendErrors(s);
                appendAll(s);
            }), sc);
            Core.Instance.Log.SubscribeInfoOn(Observer.Create<string>(s =>
            {
                appendInfo(s);
                appendAll(s);
            }), sc);

            IPAddress localIP;
            if (!IPAddress.TryParse(Config.Global.LocalIP, out localIP))
            {
                Console.WriteLine("Config LocalIP '{0}' is not a valid IP address.", Config.Global.LocalIP);
                return;
            }

            long maxBackupSize = Config.Global.DiskSpace; // Max site used to backup (locally)
            int chunkSize = Config.Global.ChunkSize; // Size of each chunk stored locally and sent over the network
            IPAddress MCIP = IPAddress.Parse(Config.Global.MCIP);
            IPAddress MDBIP = IPAddress.Parse(Config.Global.MDBIP);
            IPAddress MDRIP = IPAddress.Parse(Config.Global.MDRIP);

            int MCPort = Config.Global.MCPort;
            int MDBPort = Config.Global.MDBPort;
            int MDRPort = Config.Global.MDRPort;

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
                Core.Instance.Config = new Core.Settings(localIP, maxBackupSize, chunkSize, backupChunkTimeout,
                    backupChunkTimeoutMultiplier, backupChunkRetries, versionM, versionN,
                    randomDelayMin, randomDelayMax, backupDirectory, restoreDirectory, MCIP, MCPort, MDBIP, MDBPort, MDRIP, MDRPort);
            }
            catch (Exception ex)
            {
                Core.Instance.Log.Error("config setup", ex);
                return;
            }

            reclaimSizeNumericUpDown.Maximum = decimal.MaxValue;
            reclaimSizeNumericUpDown.Value = Core.Instance.Config.MaxBackupSize;

            foreach (var backupFile in Core.Instance.BackupFiles)
            {
                var entry = backupFile.Value;
                filesListView.Items.AddWithTextAndSubItems(entry.OriginalFileName,
                    entry.ReplicationDegree.ToString(CultureInfo.InvariantCulture),
                    BACKED_UP,
                    GetNumberOfChunks(entry.OriginalFileName).ToString(CultureInfo.InvariantCulture));
            }

            Core.Instance.Start(false);
        }

        private int GetSizeMultiplier() // in bytes
        {
            if (sizeTypeListBox.SelectedItem == null)
                sizeTypeListBox.SelectedItem = "B";

            switch (sizeTypeListBox.SelectedItem.ToString())
            {
                case "B":
                    return 1;
                case "KB":
                    return 1000;
                case "MB":
                    return 1000000;
                case "GB":
                    return 1000000000;
                default:
                    return 1;
            }
        }

        private void addFileButton_Click(object sender, EventArgs e)
        {
            var res = openFileDialog.ShowDialog();
            if (res != DialogResult.OK) return;
            var fileNames = openFileDialog.FileNames;
            foreach (var fileName in fileNames)
            {
                var numberOfChunks = GetNumberOfChunks(fileName);
                var strNumber = numberOfChunks.ToString(CultureInfo.InvariantCulture);

                filesListView.Items.AddWithTextAndSubItems(fileName, "1", NOT_BACKED_UP, strNumber);
            }
        }

        private static double GetNumberOfChunks(string fileName)
        {
            var fileSize = GetFileSize(fileName);
            if (fileSize == -1)
                return -1;
            var numberOfChunks = Math.Ceiling(fileSize/(double)Core.Instance.Config.ChunkSize);
            return numberOfChunks;
        }

        private static long GetFileSize(string fileName)
        {
            try
            {
                return new FileInfo(fileName).Length;
            }
            catch (Exception)
            {
                return -1;
            }
        }

        private void removeFileButton_Click(object sender, EventArgs e)
        {
            if (filesListView.SelectedItems.Count == 0)
                return;

            var removeFileItem = filesListView.SelectedItems[0];
            var removeFileName = removeFileItem.Text;

            if (deleteButton.Enabled)
                deleteButton_Click(sender, null);

            filesListView.Items.Remove(removeFileItem);
            var toRemove = Core.Instance.BackupFiles
                .Where(pair => pair.Value.OriginalFileName == removeFileName)
                .Select(pair => pair.Value.OriginalFileName).ToList();

            foreach (var rem in toRemove)
            {
                Core.Instance.BackupFiles.Remove(rem);
            }
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
                    restoreButton.Enabled = _servicesStarted;
                    deleteButton.Enabled = _servicesStarted;
                    incDegreeButton.Enabled = false;
                    decDegreeButton.Enabled = false;
                }
                else if (e.Item.SubItems[2].Text == RESTORED)
                {
                    backupButton.Enabled = false;
                    restoreButton.Enabled = _servicesStarted;
                    deleteButton.Enabled = _servicesStarted;
                    incDegreeButton.Enabled = false;
                    decDegreeButton.Enabled = false;
                }
                else if (e.Item.SubItems[2].Text == NOT_BACKED_UP)
                {
                    backupButton.Enabled = _servicesStarted;
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
            restoreButton.Enabled = _servicesStarted;
            deleteButton.Enabled = _servicesStarted;
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
                .Where(backupFile => backupFile.Value.OriginalFileName == fileName).ToList();
            if (files.Count != 1)
                return;

            _commandSwitch.Execute(new RestoreFileCommand(files[0].Value, enhanCheckBox.Checked));
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
                .Where(backupFile => backupFile.Value.OriginalFileName == fileName).ToList();

            foreach (var file in files)
            {
                Core.Instance.BackupFiles.Remove(file.Key);
                _commandSwitch.Execute(new DeleteFileCommand(file.Value));
                filesListView.SelectedItems[0].SubItems[2].Text = NOT_BACKED_UP;
            }

            

            backupButton.Enabled = _servicesStarted;
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

        private bool _servicesStarted;

        private void startServicesButton_Click(object sender, EventArgs e)
        {
            var useEnhanced = enhanCheckBox.Checked;
            Core.Instance.StartServices(useEnhanced);
            _servicesStarted = true;
            startServicesButton.Enabled = false;
            stopServicesButton.Enabled = true;
            enhanCheckBox.Enabled = false;
            reclaimButton.Enabled = true;
        }

        private void stopServicesButton_Click(object sender, EventArgs e)
        {
            Core.Instance.StopServices();
            _servicesStarted = false;
            startServicesButton.Enabled = true;
            stopServicesButton.Enabled = false;
            enhanCheckBox.Enabled = true;
            reclaimButton.Enabled = false;
        }

        private void clearLogsButton_Click(object sender, EventArgs e)
        {
            allLogTextBox.Clear();
            transmittedTextBox.Clear();
            receivedLogTextBox.Clear();
            sentLogTextBox.Clear();
            errorsLogTextBox.Clear();
            infoLogTextBox.Clear();
        }

        private void showChunksButton_Click(object sender, EventArgs e)
        {
            var f = new ChunksStoreForm();
            f.Show();
        }

        private void reclaimSizeNumericUpDown_ValueChanged(object sender, EventArgs e)
        {
            var size = reclaimSizeNumericUpDown.Value * GetSizeMultiplier();
            if (size > long.MaxValue)
            {
                size = long.MaxValue;
                reclaimSizeNumericUpDown.Value = size;
            }

            var sizeProper = (long) size;
            if (sizeProper == Core.Instance.Config.MaxBackupSize)
                return;

            Core.Instance.Config.MaxBackupSize = sizeProper;
            Core.Instance.Log.InfoFormat("Updated MaxBackupSize to {0} bytes", sizeProper);
        }

        private void sizeTypeListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            reclaimSizeNumericUpDown_ValueChanged(sender, e);
        }
    }
}
