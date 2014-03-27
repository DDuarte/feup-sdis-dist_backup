namespace PeerGUI
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.backupButton = new System.Windows.Forms.Button();
            this.restoreButton = new System.Windows.Forms.Button();
            this.deleteButton = new System.Windows.Forms.Button();
            this.reclaimButton = new System.Windows.Forms.Button();
            this.actionsGroupBox = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanel4 = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel5 = new System.Windows.Forms.TableLayoutPanel();
            this.incDegreeButton = new System.Windows.Forms.Button();
            this.decDegreeButton = new System.Windows.Forms.Button();
            this.openFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.filesGroupBox = new System.Windows.Forms.GroupBox();
            this.filesListView = new System.Windows.Forms.ListView();
            this.fileHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.backupDegreeHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.statusHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.addFileButton = new System.Windows.Forms.Button();
            this.removeFileButton = new System.Windows.Forms.Button();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel3 = new System.Windows.Forms.TableLayoutPanel();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.allTabPage = new System.Windows.Forms.TabPage();
            this.allLogTextBox = new System.Windows.Forms.TextBox();
            this.receivedTabPage = new System.Windows.Forms.TabPage();
            this.receivedLogTextBox = new System.Windows.Forms.TextBox();
            this.sentTabPage = new System.Windows.Forms.TabPage();
            this.sentLogTextBox = new System.Windows.Forms.TextBox();
            this.errorsTabPage = new System.Windows.Forms.TabPage();
            this.errorsLogTextBox = new System.Windows.Forms.TextBox();
            this.infoTabPage = new System.Windows.Forms.TabPage();
            this.infoLogTextBox = new System.Windows.Forms.TextBox();
            this.transmittedTabPage = new System.Windows.Forms.TabPage();
            this.transmittedTextBox = new System.Windows.Forms.TextBox();
            this.actionsGroupBox.SuspendLayout();
            this.tableLayoutPanel4.SuspendLayout();
            this.tableLayoutPanel5.SuspendLayout();
            this.filesGroupBox.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.tableLayoutPanel3.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.allTabPage.SuspendLayout();
            this.receivedTabPage.SuspendLayout();
            this.sentTabPage.SuspendLayout();
            this.errorsTabPage.SuspendLayout();
            this.infoTabPage.SuspendLayout();
            this.transmittedTabPage.SuspendLayout();
            this.SuspendLayout();
            // 
            // backupButton
            // 
            this.backupButton.Dock = System.Windows.Forms.DockStyle.Fill;
            this.backupButton.Enabled = false;
            this.backupButton.Location = new System.Drawing.Point(3, 3);
            this.backupButton.Name = "backupButton";
            this.backupButton.Size = new System.Drawing.Size(498, 28);
            this.backupButton.TabIndex = 0;
            this.backupButton.Text = "Backup File";
            this.backupButton.UseVisualStyleBackColor = true;
            this.backupButton.Click += new System.EventHandler(this.backupButton_Click);
            // 
            // restoreButton
            // 
            this.restoreButton.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.restoreButton.Dock = System.Windows.Forms.DockStyle.Fill;
            this.restoreButton.Enabled = false;
            this.restoreButton.Location = new System.Drawing.Point(3, 37);
            this.restoreButton.Name = "restoreButton";
            this.restoreButton.Size = new System.Drawing.Size(498, 28);
            this.restoreButton.TabIndex = 1;
            this.restoreButton.Text = "Restore File";
            this.restoreButton.UseVisualStyleBackColor = true;
            this.restoreButton.Click += new System.EventHandler(this.restoreButton_Click);
            // 
            // deleteButton
            // 
            this.deleteButton.Dock = System.Windows.Forms.DockStyle.Fill;
            this.deleteButton.Enabled = false;
            this.deleteButton.Location = new System.Drawing.Point(3, 71);
            this.deleteButton.Name = "deleteButton";
            this.deleteButton.Size = new System.Drawing.Size(498, 28);
            this.deleteButton.TabIndex = 2;
            this.deleteButton.Text = "Delete File";
            this.deleteButton.UseVisualStyleBackColor = true;
            this.deleteButton.Click += new System.EventHandler(this.deleteButton_Click);
            // 
            // reclaimButton
            // 
            this.reclaimButton.Dock = System.Windows.Forms.DockStyle.Fill;
            this.reclaimButton.Location = new System.Drawing.Point(3, 139);
            this.reclaimButton.Name = "reclaimButton";
            this.reclaimButton.Size = new System.Drawing.Size(498, 28);
            this.reclaimButton.TabIndex = 3;
            this.reclaimButton.Text = "Reclaim Space";
            this.reclaimButton.UseVisualStyleBackColor = true;
            this.reclaimButton.Click += new System.EventHandler(this.reclaimButton_Click);
            // 
            // actionsGroupBox
            // 
            this.actionsGroupBox.Controls.Add(this.tableLayoutPanel4);
            this.actionsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.actionsGroupBox.Location = new System.Drawing.Point(3, 3);
            this.actionsGroupBox.Name = "actionsGroupBox";
            this.actionsGroupBox.Size = new System.Drawing.Size(510, 189);
            this.actionsGroupBox.TabIndex = 4;
            this.actionsGroupBox.TabStop = false;
            this.actionsGroupBox.Text = "Actions";
            // 
            // tableLayoutPanel4
            // 
            this.tableLayoutPanel4.ColumnCount = 1;
            this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel4.Controls.Add(this.reclaimButton, 0, 4);
            this.tableLayoutPanel4.Controls.Add(this.backupButton, 0, 0);
            this.tableLayoutPanel4.Controls.Add(this.deleteButton, 0, 2);
            this.tableLayoutPanel4.Controls.Add(this.restoreButton, 0, 1);
            this.tableLayoutPanel4.Controls.Add(this.tableLayoutPanel5, 0, 3);
            this.tableLayoutPanel4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel4.Location = new System.Drawing.Point(3, 16);
            this.tableLayoutPanel4.Name = "tableLayoutPanel4";
            this.tableLayoutPanel4.RowCount = 5;
            this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tableLayoutPanel4.Size = new System.Drawing.Size(504, 170);
            this.tableLayoutPanel4.TabIndex = 0;
            // 
            // tableLayoutPanel5
            // 
            this.tableLayoutPanel5.ColumnCount = 2;
            this.tableLayoutPanel5.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel5.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel5.Controls.Add(this.incDegreeButton, 0, 0);
            this.tableLayoutPanel5.Controls.Add(this.decDegreeButton, 1, 0);
            this.tableLayoutPanel5.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel5.Location = new System.Drawing.Point(3, 105);
            this.tableLayoutPanel5.Name = "tableLayoutPanel5";
            this.tableLayoutPanel5.RowCount = 1;
            this.tableLayoutPanel5.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel5.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 28F));
            this.tableLayoutPanel5.Size = new System.Drawing.Size(498, 28);
            this.tableLayoutPanel5.TabIndex = 3;
            // 
            // incDegreeButton
            // 
            this.incDegreeButton.Dock = System.Windows.Forms.DockStyle.Fill;
            this.incDegreeButton.Enabled = false;
            this.incDegreeButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.incDegreeButton.Location = new System.Drawing.Point(3, 3);
            this.incDegreeButton.Name = "incDegreeButton";
            this.incDegreeButton.Size = new System.Drawing.Size(243, 22);
            this.incDegreeButton.TabIndex = 0;
            this.incDegreeButton.Text = "+ Replication Degree";
            this.incDegreeButton.UseVisualStyleBackColor = true;
            this.incDegreeButton.Click += new System.EventHandler(this.incDegreeButton_Click);
            // 
            // decDegreeButton
            // 
            this.decDegreeButton.Dock = System.Windows.Forms.DockStyle.Fill;
            this.decDegreeButton.Enabled = false;
            this.decDegreeButton.Location = new System.Drawing.Point(252, 3);
            this.decDegreeButton.Name = "decDegreeButton";
            this.decDegreeButton.Size = new System.Drawing.Size(243, 22);
            this.decDegreeButton.TabIndex = 1;
            this.decDegreeButton.Text = "- Replication Degree";
            this.decDegreeButton.UseVisualStyleBackColor = true;
            this.decDegreeButton.Click += new System.EventHandler(this.decDegreeButton_Click);
            // 
            // openFileDialog
            // 
            this.openFileDialog.FileName = "openFileDialog1";
            this.openFileDialog.Multiselect = true;
            // 
            // filesGroupBox
            // 
            this.filesGroupBox.Controls.Add(this.filesListView);
            this.filesGroupBox.Controls.Add(this.tableLayoutPanel1);
            this.filesGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.filesGroupBox.Location = new System.Drawing.Point(3, 3);
            this.filesGroupBox.Name = "filesGroupBox";
            this.filesGroupBox.Size = new System.Drawing.Size(168, 391);
            this.filesGroupBox.TabIndex = 6;
            this.filesGroupBox.TabStop = false;
            this.filesGroupBox.Text = "Files";
            // 
            // filesListView
            // 
            this.filesListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.fileHeader,
            this.backupDegreeHeader,
            this.statusHeader});
            this.filesListView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.filesListView.Location = new System.Drawing.Point(3, 16);
            this.filesListView.MultiSelect = false;
            this.filesListView.Name = "filesListView";
            this.filesListView.Size = new System.Drawing.Size(162, 328);
            this.filesListView.TabIndex = 1;
            this.filesListView.UseCompatibleStateImageBehavior = false;
            this.filesListView.View = System.Windows.Forms.View.Details;
            this.filesListView.ItemSelectionChanged += new System.Windows.Forms.ListViewItemSelectionChangedEventHandler(this.filesListView_ItemSelectionChanged);
            // 
            // fileHeader
            // 
            this.fileHeader.Text = "File";
            // 
            // backupDegreeHeader
            // 
            this.backupDegreeHeader.Text = "D";
            this.backupDegreeHeader.Width = 27;
            // 
            // statusHeader
            // 
            this.statusHeader.Text = "Status";
            this.statusHeader.Width = 85;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Controls.Add(this.addFileButton, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.removeFileButton, 1, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(3, 344);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 1;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(162, 44);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // addFileButton
            // 
            this.addFileButton.Dock = System.Windows.Forms.DockStyle.Fill;
            this.addFileButton.Location = new System.Drawing.Point(3, 3);
            this.addFileButton.Name = "addFileButton";
            this.addFileButton.Size = new System.Drawing.Size(75, 38);
            this.addFileButton.TabIndex = 7;
            this.addFileButton.Text = "Add File";
            this.addFileButton.UseVisualStyleBackColor = true;
            this.addFileButton.Click += new System.EventHandler(this.addFileButton_Click);
            // 
            // removeFileButton
            // 
            this.removeFileButton.Dock = System.Windows.Forms.DockStyle.Fill;
            this.removeFileButton.Location = new System.Drawing.Point(84, 3);
            this.removeFileButton.Name = "removeFileButton";
            this.removeFileButton.Size = new System.Drawing.Size(75, 38);
            this.removeFileButton.TabIndex = 8;
            this.removeFileButton.Text = "Remove File";
            this.removeFileButton.UseVisualStyleBackColor = true;
            this.removeFileButton.Click += new System.EventHandler(this.removeFileButton_Click);
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.ColumnCount = 2;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 75F));
            this.tableLayoutPanel2.Controls.Add(this.tableLayoutPanel3, 1, 0);
            this.tableLayoutPanel2.Controls.Add(this.filesGroupBox, 0, 0);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 1;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(696, 397);
            this.tableLayoutPanel2.TabIndex = 7;
            // 
            // tableLayoutPanel3
            // 
            this.tableLayoutPanel3.ColumnCount = 1;
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel3.Controls.Add(this.actionsGroupBox, 0, 0);
            this.tableLayoutPanel3.Controls.Add(this.tabControl1, 0, 1);
            this.tableLayoutPanel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel3.Location = new System.Drawing.Point(177, 3);
            this.tableLayoutPanel3.Name = "tableLayoutPanel3";
            this.tableLayoutPanel3.RowCount = 2;
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel3.Size = new System.Drawing.Size(516, 391);
            this.tableLayoutPanel3.TabIndex = 7;
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.allTabPage);
            this.tabControl1.Controls.Add(this.transmittedTabPage);
            this.tabControl1.Controls.Add(this.receivedTabPage);
            this.tabControl1.Controls.Add(this.sentTabPage);
            this.tabControl1.Controls.Add(this.errorsTabPage);
            this.tabControl1.Controls.Add(this.infoTabPage);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Location = new System.Drawing.Point(3, 198);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(510, 190);
            this.tabControl1.TabIndex = 5;
            // 
            // allTabPage
            // 
            this.allTabPage.Controls.Add(this.allLogTextBox);
            this.allTabPage.Location = new System.Drawing.Point(4, 22);
            this.allTabPage.Name = "allTabPage";
            this.allTabPage.Size = new System.Drawing.Size(502, 164);
            this.allTabPage.TabIndex = 6;
            this.allTabPage.Text = "All";
            this.allTabPage.UseVisualStyleBackColor = true;
            // 
            // allLogTextBox
            // 
            this.allLogTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.allLogTextBox.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.allLogTextBox.Location = new System.Drawing.Point(0, 0);
            this.allLogTextBox.Multiline = true;
            this.allLogTextBox.Name = "allLogTextBox";
            this.allLogTextBox.ReadOnly = true;
            this.allLogTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.allLogTextBox.Size = new System.Drawing.Size(502, 164);
            this.allLogTextBox.TabIndex = 4;
            // 
            // receivedTabPage
            // 
            this.receivedTabPage.Controls.Add(this.receivedLogTextBox);
            this.receivedTabPage.Location = new System.Drawing.Point(4, 22);
            this.receivedTabPage.Name = "receivedTabPage";
            this.receivedTabPage.Size = new System.Drawing.Size(502, 164);
            this.receivedTabPage.TabIndex = 5;
            this.receivedTabPage.Text = "Received";
            this.receivedTabPage.UseVisualStyleBackColor = true;
            // 
            // receivedLogTextBox
            // 
            this.receivedLogTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.receivedLogTextBox.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.receivedLogTextBox.Location = new System.Drawing.Point(0, 0);
            this.receivedLogTextBox.Multiline = true;
            this.receivedLogTextBox.Name = "receivedLogTextBox";
            this.receivedLogTextBox.ReadOnly = true;
            this.receivedLogTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.receivedLogTextBox.Size = new System.Drawing.Size(502, 164);
            this.receivedLogTextBox.TabIndex = 5;
            // 
            // sentTabPage
            // 
            this.sentTabPage.Controls.Add(this.sentLogTextBox);
            this.sentTabPage.Location = new System.Drawing.Point(4, 22);
            this.sentTabPage.Name = "sentTabPage";
            this.sentTabPage.Size = new System.Drawing.Size(502, 164);
            this.sentTabPage.TabIndex = 2;
            this.sentTabPage.Text = "Sent";
            this.sentTabPage.UseVisualStyleBackColor = true;
            // 
            // sentLogTextBox
            // 
            this.sentLogTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.sentLogTextBox.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.sentLogTextBox.Location = new System.Drawing.Point(0, 0);
            this.sentLogTextBox.Multiline = true;
            this.sentLogTextBox.Name = "sentLogTextBox";
            this.sentLogTextBox.ReadOnly = true;
            this.sentLogTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.sentLogTextBox.Size = new System.Drawing.Size(502, 164);
            this.sentLogTextBox.TabIndex = 5;
            // 
            // errorsTabPage
            // 
            this.errorsTabPage.Controls.Add(this.errorsLogTextBox);
            this.errorsTabPage.Location = new System.Drawing.Point(4, 22);
            this.errorsTabPage.Name = "errorsTabPage";
            this.errorsTabPage.Size = new System.Drawing.Size(502, 164);
            this.errorsTabPage.TabIndex = 3;
            this.errorsTabPage.Text = "Errors";
            this.errorsTabPage.UseVisualStyleBackColor = true;
            // 
            // errorsLogTextBox
            // 
            this.errorsLogTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.errorsLogTextBox.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.errorsLogTextBox.Location = new System.Drawing.Point(0, 0);
            this.errorsLogTextBox.Multiline = true;
            this.errorsLogTextBox.Name = "errorsLogTextBox";
            this.errorsLogTextBox.ReadOnly = true;
            this.errorsLogTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.errorsLogTextBox.Size = new System.Drawing.Size(502, 164);
            this.errorsLogTextBox.TabIndex = 5;
            // 
            // infoTabPage
            // 
            this.infoTabPage.Controls.Add(this.infoLogTextBox);
            this.infoTabPage.Location = new System.Drawing.Point(4, 22);
            this.infoTabPage.Name = "infoTabPage";
            this.infoTabPage.Size = new System.Drawing.Size(502, 164);
            this.infoTabPage.TabIndex = 4;
            this.infoTabPage.Text = "Info";
            this.infoTabPage.UseVisualStyleBackColor = true;
            // 
            // infoLogTextBox
            // 
            this.infoLogTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.infoLogTextBox.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.infoLogTextBox.Location = new System.Drawing.Point(0, 0);
            this.infoLogTextBox.Multiline = true;
            this.infoLogTextBox.Name = "infoLogTextBox";
            this.infoLogTextBox.ReadOnly = true;
            this.infoLogTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.infoLogTextBox.Size = new System.Drawing.Size(502, 164);
            this.infoLogTextBox.TabIndex = 5;
            // 
            // transmittedTabPage
            // 
            this.transmittedTabPage.Controls.Add(this.transmittedTextBox);
            this.transmittedTabPage.Location = new System.Drawing.Point(4, 22);
            this.transmittedTabPage.Name = "transmittedTabPage";
            this.transmittedTabPage.Size = new System.Drawing.Size(502, 164);
            this.transmittedTabPage.TabIndex = 7;
            this.transmittedTabPage.Text = "Transmitted";
            this.transmittedTabPage.UseVisualStyleBackColor = true;
            // 
            // transmittedTextBox
            // 
            this.transmittedTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.transmittedTextBox.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.transmittedTextBox.Location = new System.Drawing.Point(0, 0);
            this.transmittedTextBox.Multiline = true;
            this.transmittedTextBox.Name = "transmittedTextBox";
            this.transmittedTextBox.ReadOnly = true;
            this.transmittedTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.transmittedTextBox.Size = new System.Drawing.Size(502, 164);
            this.transmittedTextBox.TabIndex = 6;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(696, 397);
            this.Controls.Add(this.tableLayoutPanel2);
            this.Name = "Form1";
            this.Text = "Distributed Backup Service";
            this.actionsGroupBox.ResumeLayout(false);
            this.tableLayoutPanel4.ResumeLayout(false);
            this.tableLayoutPanel5.ResumeLayout(false);
            this.filesGroupBox.ResumeLayout(false);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tableLayoutPanel3.ResumeLayout(false);
            this.tabControl1.ResumeLayout(false);
            this.allTabPage.ResumeLayout(false);
            this.allTabPage.PerformLayout();
            this.receivedTabPage.ResumeLayout(false);
            this.receivedTabPage.PerformLayout();
            this.sentTabPage.ResumeLayout(false);
            this.sentTabPage.PerformLayout();
            this.errorsTabPage.ResumeLayout(false);
            this.errorsTabPage.PerformLayout();
            this.infoTabPage.ResumeLayout(false);
            this.infoTabPage.PerformLayout();
            this.transmittedTabPage.ResumeLayout(false);
            this.transmittedTabPage.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button backupButton;
        private System.Windows.Forms.Button restoreButton;
        private System.Windows.Forms.Button deleteButton;
        private System.Windows.Forms.Button reclaimButton;
        private System.Windows.Forms.GroupBox actionsGroupBox;
        private System.Windows.Forms.OpenFileDialog openFileDialog;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel4;
        private System.Windows.Forms.GroupBox filesGroupBox;
        private System.Windows.Forms.ListView filesListView;
        private System.Windows.Forms.ColumnHeader fileHeader;
        private System.Windows.Forms.ColumnHeader backupDegreeHeader;
        private System.Windows.Forms.ColumnHeader statusHeader;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Button addFileButton;
        private System.Windows.Forms.Button removeFileButton;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel5;
        private System.Windows.Forms.Button incDegreeButton;
        private System.Windows.Forms.Button decDegreeButton;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel3;
        private System.Windows.Forms.TextBox receivedLogTextBox;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TextBox allLogTextBox;
        private System.Windows.Forms.TabPage receivedTabPage;
        private System.Windows.Forms.TabPage sentTabPage;
        private System.Windows.Forms.TextBox sentLogTextBox;
        private System.Windows.Forms.TabPage errorsTabPage;
        private System.Windows.Forms.TextBox errorsLogTextBox;
        private System.Windows.Forms.TabPage infoTabPage;
        private System.Windows.Forms.TextBox infoLogTextBox;
        private System.Windows.Forms.TabPage allTabPage;
        private System.Windows.Forms.TabPage transmittedTabPage;
        private System.Windows.Forms.TextBox transmittedTextBox;
    }
}

