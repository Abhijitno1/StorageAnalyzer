namespace FilesHunter
{
	partial class frmFolderTreeFamilies
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
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.btnOpenDialog = new System.Windows.Forms.Button();
			this.btnSaveFolderData = new System.Windows.Forms.Button();
			this.txtFileLocation = new System.Windows.Forms.TextBox();
			this.label1 = new System.Windows.Forms.Label();
			this.groupBox2 = new System.Windows.Forms.GroupBox();
			this.btnDeleteSelected = new System.Windows.Forms.Button();
			this.btnOpenSelected = new System.Windows.Forms.Button();
			this.lstFolderHrchies = new System.Windows.Forms.ListBox();
			this.fbdFolderLocation = new System.Windows.Forms.FolderBrowserDialog();
			this.groupBox1.SuspendLayout();
			this.groupBox2.SuspendLayout();
			this.SuspendLayout();
			// 
			// groupBox1
			// 
			this.groupBox1.Controls.Add(this.btnOpenDialog);
			this.groupBox1.Controls.Add(this.btnSaveFolderData);
			this.groupBox1.Controls.Add(this.txtFileLocation);
			this.groupBox1.Controls.Add(this.label1);
			this.groupBox1.Location = new System.Drawing.Point(6, 392);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(941, 67);
			this.groupBox1.TabIndex = 21;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "Create New Folder Hierarchy";
			// 
			// btnOpenDialog
			// 
			this.btnOpenDialog.AutoSize = true;
			this.btnOpenDialog.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.btnOpenDialog.Location = new System.Drawing.Point(789, 25);
			this.btnOpenDialog.Name = "btnOpenDialog";
			this.btnOpenDialog.Size = new System.Drawing.Size(26, 26);
			this.btnOpenDialog.TabIndex = 24;
			this.btnOpenDialog.Text = "...";
			this.btnOpenDialog.UseVisualStyleBackColor = true;
			this.btnOpenDialog.Click += new System.EventHandler(this.btnOpenDialog_Click);
			// 
			// btnSaveFolderData
			// 
			this.btnSaveFolderData.Location = new System.Drawing.Point(821, 27);
			this.btnSaveFolderData.Name = "btnSaveFolderData";
			this.btnSaveFolderData.Size = new System.Drawing.Size(82, 25);
			this.btnSaveFolderData.TabIndex = 23;
			this.btnSaveFolderData.Text = "Save Treeview";
			this.btnSaveFolderData.UseVisualStyleBackColor = true;
			this.btnSaveFolderData.Click += new System.EventHandler(this.btnSaveFolderData_Click);
			// 
			// txtFileLocation
			// 
			this.txtFileLocation.Location = new System.Drawing.Point(116, 27);
			this.txtFileLocation.Name = "txtFileLocation";
			this.txtFileLocation.Size = new System.Drawing.Size(667, 22);
			this.txtFileLocation.TabIndex = 22;
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(8, 30);
			this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(83, 16);
			this.label1.TabIndex = 21;
			this.label1.Text = "File Location";
			// 
			// groupBox2
			// 
			this.groupBox2.Controls.Add(this.btnDeleteSelected);
			this.groupBox2.Controls.Add(this.btnOpenSelected);
			this.groupBox2.Controls.Add(this.lstFolderHrchies);
			this.groupBox2.Location = new System.Drawing.Point(5, 1);
			this.groupBox2.Name = "groupBox2";
			this.groupBox2.Size = new System.Drawing.Size(942, 384);
			this.groupBox2.TabIndex = 22;
			this.groupBox2.TabStop = false;
			this.groupBox2.Text = "Existing Folder Hierarchies List";
			// 
			// btnDeleteSelected
			// 
			this.btnDeleteSelected.Location = new System.Drawing.Point(822, 89);
			this.btnDeleteSelected.Name = "btnDeleteSelected";
			this.btnDeleteSelected.Size = new System.Drawing.Size(108, 46);
			this.btnDeleteSelected.TabIndex = 5;
			this.btnDeleteSelected.Text = "Delete Selected";
			this.btnDeleteSelected.UseVisualStyleBackColor = true;
			this.btnDeleteSelected.Click += new System.EventHandler(this.btnDeleteSelected_Click);
			// 
			// btnOpenSelected
			// 
			this.btnOpenSelected.Location = new System.Drawing.Point(822, 34);
			this.btnOpenSelected.Name = "btnOpenSelected";
			this.btnOpenSelected.Size = new System.Drawing.Size(108, 32);
			this.btnOpenSelected.TabIndex = 4;
			this.btnOpenSelected.Text = "Open Selected";
			this.btnOpenSelected.UseVisualStyleBackColor = true;
			this.btnOpenSelected.Click += new System.EventHandler(this.btnOpenSelected_Click);
			// 
			// lstFolderHrchies
			// 
			this.lstFolderHrchies.FormattingEnabled = true;
			this.lstFolderHrchies.ItemHeight = 16;
			this.lstFolderHrchies.Location = new System.Drawing.Point(7, 21);
			this.lstFolderHrchies.Name = "lstFolderHrchies";
			this.lstFolderHrchies.Size = new System.Drawing.Size(809, 340);
			this.lstFolderHrchies.TabIndex = 3;
			this.lstFolderHrchies.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.lstFolderHrchies_MouseDoubleClick);
			// 
			// frmFolderTreeFamilies
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(953, 471);
			this.Controls.Add(this.groupBox2);
			this.Controls.Add(this.groupBox1);
			this.Name = "frmFolderTreeFamilies";
			this.Text = "Virtual Folder Tree Maintenance";
			this.Load += new System.EventHandler(this.frmFolderTreeFamilies_Load);
			this.groupBox1.ResumeLayout(false);
			this.groupBox1.PerformLayout();
			this.groupBox2.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion
		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.Button btnSaveFolderData;
		private System.Windows.Forms.TextBox txtFileLocation;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.GroupBox groupBox2;
		private System.Windows.Forms.Button btnDeleteSelected;
		private System.Windows.Forms.Button btnOpenSelected;
		private System.Windows.Forms.ListBox lstFolderHrchies;
		private System.Windows.Forms.Button btnOpenDialog;
		private System.Windows.Forms.FolderBrowserDialog fbdFolderLocation;
	}
}