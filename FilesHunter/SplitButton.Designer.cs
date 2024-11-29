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
			this.splitMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.addAtEndToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.insertToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.insertAfterToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
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
			// splitMenuStrip
			// 
			this.splitMenuStrip.ImageScalingSize = new System.Drawing.Size(20, 20);
			this.splitMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.addAtEndToolStripMenuItem,
            this.insertToolStripMenuItem,
            this.insertAfterToolStripMenuItem});
			this.splitMenuStrip.Name = "splitMenuStrip";
			this.splitMenuStrip.Size = new System.Drawing.Size(167, 76);
			// 
			// addAtEndToolStripMenuItem
			// 
			this.addAtEndToolStripMenuItem.Name = "addAtEndToolStripMenuItem";
			this.addAtEndToolStripMenuItem.Size = new System.Drawing.Size(166, 24);
			this.addAtEndToolStripMenuItem.Tag = "addatend";
			this.addAtEndToolStripMenuItem.Text = "Add At End";
			this.addAtEndToolStripMenuItem.Click += new System.EventHandler(this.addAtEndToolStripMenuItem_Click);
			// 
			// insertToolStripMenuItem
			// 
			this.insertToolStripMenuItem.Name = "insertToolStripMenuItem";
			this.insertToolStripMenuItem.Size = new System.Drawing.Size(166, 24);
			this.insertToolStripMenuItem.Tag = "insertbefore";
			this.insertToolStripMenuItem.Text = "Insert  Before";
			this.insertToolStripMenuItem.Click += new System.EventHandler(this.insertToolStripMenuItem_Click);
			// 
			// insertAfterToolStripMenuItem
			// 
			this.insertAfterToolStripMenuItem.Name = "insertAfterToolStripMenuItem";
			this.insertAfterToolStripMenuItem.Size = new System.Drawing.Size(166, 24);
			this.insertAfterToolStripMenuItem.Tag = "insertafter";
			this.insertAfterToolStripMenuItem.Text = "Insert After";
			this.insertAfterToolStripMenuItem.Click += new System.EventHandler(this.insertAfterToolStripMenuItem_Click);
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
		private System.Windows.Forms.ContextMenuStrip splitMenuStrip;
		private System.Windows.Forms.ToolStripMenuItem addAtEndToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem insertToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem insertAfterToolStripMenuItem;
	}
}
