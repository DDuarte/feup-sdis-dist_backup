namespace PeerGUI
{
    partial class ChunksStoreForm
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
            this.chunksListView = new System.Windows.Forms.ListView();
            this.chunkColumnHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.IPColumnHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.actualRepDegColumnHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.wantedRepDegColumnHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.SuspendLayout();
            // 
            // chunksListView
            // 
            this.chunksListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.chunkColumnHeader,
            this.IPColumnHeader,
            this.actualRepDegColumnHeader,
            this.wantedRepDegColumnHeader});
            this.chunksListView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.chunksListView.Location = new System.Drawing.Point(0, 0);
            this.chunksListView.Name = "chunksListView";
            this.chunksListView.Size = new System.Drawing.Size(602, 420);
            this.chunksListView.TabIndex = 2;
            this.chunksListView.UseCompatibleStateImageBehavior = false;
            this.chunksListView.View = System.Windows.Forms.View.Details;
            // 
            // chunkColumnHeader
            // 
            this.chunkColumnHeader.Text = "Chunk";
            this.chunkColumnHeader.Width = 440;
            // 
            // IPColumnHeader
            // 
            this.IPColumnHeader.Text = "IP";
            this.IPColumnHeader.Width = 90;
            // 
            // actualRepDegColumnHeader
            // 
            this.actualRepDegColumnHeader.Text = "Actual #";
            this.actualRepDegColumnHeader.Width = 40;
            // 
            // wantedRepDegColumnHeader
            // 
            this.wantedRepDegColumnHeader.Text = "Wanted #";
            this.wantedRepDegColumnHeader.Width = 40;
            // 
            // ChunksStoreForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(602, 420);
            this.Controls.Add(this.chunksListView);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Name = "ChunksStoreForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "List of Chunks";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListView chunksListView;
        private System.Windows.Forms.ColumnHeader chunkColumnHeader;
        private System.Windows.Forms.ColumnHeader IPColumnHeader;
        private System.Windows.Forms.ColumnHeader actualRepDegColumnHeader;
        private System.Windows.Forms.ColumnHeader wantedRepDegColumnHeader;
    }
}