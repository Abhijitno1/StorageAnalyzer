namespace FilesHunter
{
    partial class ThumbnailViewer
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
			this.components = new System.ComponentModel.Container();
			this.lvwTiles = new System.Windows.Forms.ListView();
			this.imlTiles = new System.Windows.Forms.ImageList(this.components);
			this.listViewContextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.tsMnuItmSaveToDisk = new System.Windows.Forms.ToolStripMenuItem();
			this.tsMnuItmDeleteFromDB = new System.Windows.Forms.ToolStripMenuItem();
			this.listViewContextMenu.SuspendLayout();
			this.SuspendLayout();
			// 
			// lvwTiles
			// 
			this.lvwTiles.Dock = System.Windows.Forms.DockStyle.Fill;
			this.lvwTiles.HideSelection = false;
			this.lvwTiles.LargeImageList = this.imlTiles;
			this.lvwTiles.Location = new System.Drawing.Point(0, 0);
			this.lvwTiles.MultiSelect = false;
			this.lvwTiles.Name = "lvwTiles";
			this.lvwTiles.Size = new System.Drawing.Size(1061, 593);
			this.lvwTiles.SmallImageList = this.imlTiles;
			this.lvwTiles.TabIndex = 0;
			this.lvwTiles.UseCompatibleStateImageBehavior = false;
			this.lvwTiles.MouseClick += new System.Windows.Forms.MouseEventHandler(this.lvwTiles_MouseClick);
			this.lvwTiles.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.lvwTiles_MouseDoubleClick);
			// 
			// imlTiles
			// 
			this.imlTiles.ColorDepth = System.Windows.Forms.ColorDepth.Depth8Bit;
			this.imlTiles.ImageSize = new System.Drawing.Size(128, 128);
			this.imlTiles.TransparentColor = System.Drawing.Color.Transparent;
			// 
			// listViewContextMenu
			// 
			this.listViewContextMenu.ImageScalingSize = new System.Drawing.Size(20, 20);
			this.listViewContextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsMnuItmSaveToDisk,
            this.tsMnuItmDeleteFromDB});
			this.listViewContextMenu.Name = "listViewContextMenu";
			this.listViewContextMenu.Size = new System.Drawing.Size(183, 52);
			// 
			// tsMnuItmSaveToDisk
			// 
			this.tsMnuItmSaveToDisk.Name = "tsMnuItmSaveToDisk";
			this.tsMnuItmSaveToDisk.Size = new System.Drawing.Size(182, 24);
			this.tsMnuItmSaveToDisk.Text = "Save to Disk";
			this.tsMnuItmSaveToDisk.ToolTipText = "Save this resource to Disk";
			this.tsMnuItmSaveToDisk.Click += new System.EventHandler(this.tsMnuItmSaveToDisk_Click);
			// 
			// tsMnuItmDeleteFromDB
			// 
			this.tsMnuItmDeleteFromDB.Name = "tsMnuItmDeleteFromDB";
			this.tsMnuItmDeleteFromDB.Size = new System.Drawing.Size(182, 24);
			this.tsMnuItmDeleteFromDB.Text = "Delete from DB";
			this.tsMnuItmDeleteFromDB.ToolTipText = "Delete this resource from DB";
			this.tsMnuItmDeleteFromDB.Click += new System.EventHandler(this.tsMnuItmDeleteFromDB_Click);
			// 
			// ThumbnailViewer
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.AutoScroll = true;
			this.Controls.Add(this.lvwTiles);
			this.Name = "ThumbnailViewer";
			this.Size = new System.Drawing.Size(1061, 593);
			this.listViewContextMenu.ResumeLayout(false);
			this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListView lvwTiles;
        private System.Windows.Forms.ImageList imlTiles;
		private System.Windows.Forms.ContextMenuStrip listViewContextMenu;
		private System.Windows.Forms.ToolStripMenuItem tsMnuItmSaveToDisk;
		private System.Windows.Forms.ToolStripMenuItem tsMnuItmDeleteFromDB;
	}
}
