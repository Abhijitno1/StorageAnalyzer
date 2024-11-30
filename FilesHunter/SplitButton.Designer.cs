namespace FilesHunter
{
	partial class SplitButton
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SplitButton));
			this.btnMain = new System.Windows.Forms.Button();
			this.addFolderAtEndMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.addAtEndToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.insertToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.insertAfterToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.splitMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.splitMenuStrip.SuspendLayout();
			this.SuspendLayout();
			// 
			// btnMain
			// 
			this.btnMain.Dock = System.Windows.Forms.DockStyle.Fill;
			this.btnMain.Image = ((System.Drawing.Image)(resources.GetObject("btnMain.Image")));
			this.btnMain.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.btnMain.Location = new System.Drawing.Point(0, 0);
			this.btnMain.Name = "btnMain";
			this.btnMain.Size = new System.Drawing.Size(108, 38);
			this.btnMain.TabIndex = 0;
			this.btnMain.Text = "Add to DB";
			this.btnMain.TextImageRelation = System.Windows.Forms.TextImageRelation.TextBeforeImage;
			this.btnMain.UseVisualStyleBackColor = true;
			this.btnMain.Click += new System.EventHandler(this.btnMain_Click);
			// 
			// addFolderAtEndMenuItem
			// 
			this.addFolderAtEndMenuItem.Name = "addFolderAtEndMenuItem";
			this.addFolderAtEndMenuItem.Size = new System.Drawing.Size(210, 24);
			this.addFolderAtEndMenuItem.Tag = "folderaddatend";
			this.addFolderAtEndMenuItem.Text = "Add Folder At End";
			this.addFolderAtEndMenuItem.Click += new System.EventHandler(this.toolStripMenuItem_Click);
			// 
			// addAtEndToolStripMenuItem
			// 
			this.addAtEndToolStripMenuItem.Name = "addAtEndToolStripMenuItem";
			this.addAtEndToolStripMenuItem.Size = new System.Drawing.Size(210, 24);
			this.addAtEndToolStripMenuItem.Tag = "fileaddatend";
			this.addAtEndToolStripMenuItem.Text = "Add File At End";
			this.addAtEndToolStripMenuItem.Click += new System.EventHandler(this.toolStripMenuItem_Click);
			// 
			// insertToolStripMenuItem
			// 
			this.insertToolStripMenuItem.Name = "insertToolStripMenuItem";
			this.insertToolStripMenuItem.Size = new System.Drawing.Size(210, 24);
			this.insertToolStripMenuItem.Tag = "fileinsertbefore";
			this.insertToolStripMenuItem.Text = "Insert File Before";
			this.insertToolStripMenuItem.Click += new System.EventHandler(this.toolStripMenuItem_Click);
			// 
			// insertAfterToolStripMenuItem
			// 
			this.insertAfterToolStripMenuItem.Name = "insertAfterToolStripMenuItem";
			this.insertAfterToolStripMenuItem.Size = new System.Drawing.Size(210, 24);
			this.insertAfterToolStripMenuItem.Tag = "fileinsertafter";
			this.insertAfterToolStripMenuItem.Text = "Insert File After";
			this.insertAfterToolStripMenuItem.Click += new System.EventHandler(this.toolStripMenuItem_Click);
			// 
			// splitMenuStrip
			// 
			this.splitMenuStrip.ImageScalingSize = new System.Drawing.Size(20, 20);
			this.splitMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.addFolderAtEndMenuItem,
            this.addAtEndToolStripMenuItem,
            this.insertToolStripMenuItem,
            this.insertAfterToolStripMenuItem});
			this.splitMenuStrip.Name = "splitMenuStrip";
			this.splitMenuStrip.Size = new System.Drawing.Size(211, 128);
			// 
			// SplitButton
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.btnMain);
			this.Name = "SplitButton";
			this.Size = new System.Drawing.Size(108, 38);
			this.splitMenuStrip.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion
		private System.Windows.Forms.Button btnMain;
		private System.Windows.Forms.ToolStripMenuItem addFolderAtEndMenuItem;
		private System.Windows.Forms.ToolStripMenuItem addAtEndToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem insertToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem insertAfterToolStripMenuItem;
		private System.Windows.Forms.ContextMenuStrip splitMenuStrip;
	}
}
