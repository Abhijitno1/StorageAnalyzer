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
            this.SuspendLayout();
            // 
            // lvwTiles
            // 
            this.lvwTiles.HideSelection = false;
            this.lvwTiles.LargeImageList = this.imlTiles;
            this.lvwTiles.Location = new System.Drawing.Point(3, 0);
            this.lvwTiles.Name = "lvwTiles";
            this.lvwTiles.Size = new System.Drawing.Size(1057, 590);
            this.lvwTiles.SmallImageList = this.imlTiles;
            this.lvwTiles.TabIndex = 0;
            this.lvwTiles.UseCompatibleStateImageBehavior = false;
            this.lvwTiles.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.lvwTiles_MouseDoubleClick);
            // 
            // imlTiles
            // 
            this.imlTiles.ColorDepth = System.Windows.Forms.ColorDepth.Depth8Bit;
            this.imlTiles.ImageSize = new System.Drawing.Size(128, 128);
            this.imlTiles.TransparentColor = System.Drawing.Color.Transparent;
            // 
            // ThumbnailViewer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.lvwTiles);
            this.Name = "ThumbnailViewer";
            this.Size = new System.Drawing.Size(1061, 593);
            this.Load += new System.EventHandler(this.ThumbnailViewer_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListView lvwTiles;
        private System.Windows.Forms.ImageList imlTiles;
    }
}
