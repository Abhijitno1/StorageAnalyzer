namespace FilesHunter
{
	partial class frmDBFilesBrowser
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
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmDBFilesBrowser));
			this.ofdFilePicker = new System.Windows.Forms.OpenFileDialog();
			this.imlShowPad = new System.Windows.Forms.ImageList(this.components);
			this.splitContainer1 = new System.Windows.Forms.SplitContainer();
			this.tvwDirTree = new System.Windows.Forms.TreeView();
			this.iml4TreeView = new System.Windows.Forms.ImageList(this.components);
			this.grpFolderDetails = new System.Windows.Forms.GroupBox();
			this.btnSearchDuplicates = new System.Windows.Forms.Button();
			this.btnSearchByRegex = new System.Windows.Forms.Button();
			this.btnSearchByExtn = new System.Windows.Forms.Button();
			this.btnDecolorizeTreeview = new System.Windows.Forms.Button();
			this.cboSearchType = new System.Windows.Forms.ComboBox();
			this.label3 = new System.Windows.Forms.Label();
			this.btnSearch = new System.Windows.Forms.Button();
			this.txtSearchName = new System.Windows.Forms.TextBox();
			this.label2 = new System.Windows.Forms.Label();
			this.pnlFolderSelector = new System.Windows.Forms.Panel();
			this.btnLoadTreeview = new System.Windows.Forms.Button();
			this.txtFileLocation = new System.Windows.Forms.TextBox();
			this.lblHierarchyName = new System.Windows.Forms.Label();
			this.pnlBottom = new System.Windows.Forms.Panel();
			this.label4 = new System.Windows.Forms.Label();
			this.btnSaveLocation = new System.Windows.Forms.Button();
			this.txtNewItemLocation = new System.Windows.Forms.TextBox();
			this.splitContainer2 = new System.Windows.Forms.SplitContainer();
			this.thumbViewer = new FilesHunter.ThumbnailViewer();
			this.splitButton1 = new FilesHunter.SplitButton();
			this.sfdFileSaver = new System.Windows.Forms.SaveFileDialog();
			((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
			this.splitContainer1.Panel1.SuspendLayout();
			this.splitContainer1.Panel2.SuspendLayout();
			this.splitContainer1.SuspendLayout();
			this.grpFolderDetails.SuspendLayout();
			this.pnlFolderSelector.SuspendLayout();
			this.pnlBottom.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
			this.splitContainer2.SuspendLayout();
			this.SuspendLayout();
			// 
			// ofdFilePicker
			// 
			this.ofdFilePicker.Title = "Pick File from Disk";
			// 
			// imlShowPad
			// 
			this.imlShowPad.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imlShowPad.ImageStream")));
			this.imlShowPad.TransparentColor = System.Drawing.Color.Transparent;
			this.imlShowPad.Images.SetKeyName(0, "Folder.png");
			this.imlShowPad.Images.SetKeyName(1, "File.png");
			this.imlShowPad.Images.SetKeyName(2, "text file.png");
			this.imlShowPad.Images.SetKeyName(3, "word doc.png");
			this.imlShowPad.Images.SetKeyName(4, "pdf doc.png");
			this.imlShowPad.Images.SetKeyName(5, "Video.png");
			this.imlShowPad.Images.SetKeyName(6, "Music.png");
			// 
			// splitContainer1
			// 
			this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitContainer1.Location = new System.Drawing.Point(0, 0);
			this.splitContainer1.Margin = new System.Windows.Forms.Padding(4);
			this.splitContainer1.Name = "splitContainer1";
			// 
			// splitContainer1.Panel1
			// 
			this.splitContainer1.Panel1.Controls.Add(this.tvwDirTree);
			// 
			// splitContainer1.Panel2
			// 
			this.splitContainer1.Panel2.BackColor = System.Drawing.SystemColors.Control;
			this.splitContainer1.Panel2.Controls.Add(this.grpFolderDetails);
			this.splitContainer1.Panel2.Controls.Add(this.pnlFolderSelector);
			this.splitContainer1.Panel2.Controls.Add(this.pnlBottom);
			this.splitContainer1.Size = new System.Drawing.Size(1542, 813);
			this.splitContainer1.SplitterDistance = 501;
			this.splitContainer1.SplitterWidth = 5;
			this.splitContainer1.TabIndex = 1;
			this.splitContainer1.SplitterMoved += new System.Windows.Forms.SplitterEventHandler(this.splitContainer1_SplitterMoved);
			// 
			// tvwDirTree
			// 
			this.tvwDirTree.ImageIndex = 0;
			this.tvwDirTree.ImageList = this.iml4TreeView;
			this.tvwDirTree.Location = new System.Drawing.Point(4, 2);
			this.tvwDirTree.Margin = new System.Windows.Forms.Padding(4);
			this.tvwDirTree.Name = "tvwDirTree";
			this.tvwDirTree.SelectedImageIndex = 2;
			this.tvwDirTree.ShowNodeToolTips = true;
			this.tvwDirTree.Size = new System.Drawing.Size(487, 796);
			this.tvwDirTree.TabIndex = 1;
			this.tvwDirTree.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.tvwDirTree_AfterSelect);
			// 
			// iml4TreeView
			// 
			this.iml4TreeView.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("iml4TreeView.ImageStream")));
			this.iml4TreeView.TransparentColor = System.Drawing.Color.Transparent;
			this.iml4TreeView.Images.SetKeyName(0, "folder_closed");
			this.iml4TreeView.Images.SetKeyName(1, "file");
			this.iml4TreeView.Images.SetKeyName(2, "folder_open.ico");
			// 
			// grpFolderDetails
			// 
			this.grpFolderDetails.Controls.Add(this.thumbViewer);
			this.grpFolderDetails.Controls.Add(this.btnSearchDuplicates);
			this.grpFolderDetails.Controls.Add(this.btnSearchByRegex);
			this.grpFolderDetails.Controls.Add(this.btnSearchByExtn);
			this.grpFolderDetails.Controls.Add(this.btnDecolorizeTreeview);
			this.grpFolderDetails.Controls.Add(this.cboSearchType);
			this.grpFolderDetails.Controls.Add(this.label3);
			this.grpFolderDetails.Controls.Add(this.btnSearch);
			this.grpFolderDetails.Controls.Add(this.txtSearchName);
			this.grpFolderDetails.Controls.Add(this.label2);
			this.grpFolderDetails.Location = new System.Drawing.Point(0, 58);
			this.grpFolderDetails.Name = "grpFolderDetails";
			this.grpFolderDetails.Size = new System.Drawing.Size(1036, 711);
			this.grpFolderDetails.TabIndex = 16;
			this.grpFolderDetails.TabStop = false;
			this.grpFolderDetails.Text = "Search Folders and Files";
			// 
			// btnSearchDuplicates
			// 
			this.btnSearchDuplicates.Location = new System.Drawing.Point(399, 53);
			this.btnSearchDuplicates.Name = "btnSearchDuplicates";
			this.btnSearchDuplicates.Size = new System.Drawing.Size(119, 31);
			this.btnSearchDuplicates.TabIndex = 35;
			this.btnSearchDuplicates.Text = "Search Duplicates";
			this.btnSearchDuplicates.UseVisualStyleBackColor = true;
			// 
			// btnSearchByRegex
			// 
			this.btnSearchByRegex.Location = new System.Drawing.Point(274, 53);
			this.btnSearchByRegex.Name = "btnSearchByRegex";
			this.btnSearchByRegex.Size = new System.Drawing.Size(119, 31);
			this.btnSearchByRegex.TabIndex = 34;
			this.btnSearchByRegex.Text = "Search Using RegEx";
			this.btnSearchByRegex.UseVisualStyleBackColor = true;
			// 
			// btnSearchByExtn
			// 
			this.btnSearchByExtn.Location = new System.Drawing.Point(149, 53);
			this.btnSearchByExtn.Name = "btnSearchByExtn";
			this.btnSearchByExtn.Size = new System.Drawing.Size(119, 31);
			this.btnSearchByExtn.TabIndex = 33;
			this.btnSearchByExtn.Text = "Search Multiple Extensions";
			this.btnSearchByExtn.UseVisualStyleBackColor = true;
			// 
			// btnDecolorizeTreeview
			// 
			this.btnDecolorizeTreeview.Location = new System.Drawing.Point(524, 53);
			this.btnDecolorizeTreeview.Name = "btnDecolorizeTreeview";
			this.btnDecolorizeTreeview.Size = new System.Drawing.Size(119, 32);
			this.btnDecolorizeTreeview.TabIndex = 32;
			this.btnDecolorizeTreeview.Text = "Clear Filter";
			this.btnDecolorizeTreeview.UseVisualStyleBackColor = true;
			// 
			// cboSearchType
			// 
			this.cboSearchType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cboSearchType.FormattingEnabled = true;
			this.cboSearchType.Items.AddRange(new object[] {
            "Files Only",
            "Folders Only",
            "Files and Folders"});
			this.cboSearchType.Location = new System.Drawing.Point(641, 21);
			this.cboSearchType.Name = "cboSearchType";
			this.cboSearchType.Size = new System.Drawing.Size(121, 24);
			this.cboSearchType.TabIndex = 31;
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(542, 24);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(85, 16);
			this.label3.TabIndex = 30;
			this.label3.Text = "Search Type";
			// 
			// btnSearch
			// 
			this.btnSearch.Location = new System.Drawing.Point(24, 51);
			this.btnSearch.Name = "btnSearch";
			this.btnSearch.Size = new System.Drawing.Size(119, 31);
			this.btnSearch.TabIndex = 29;
			this.btnSearch.Text = "Search By Name";
			this.btnSearch.UseVisualStyleBackColor = true;
			// 
			// txtSearchName
			// 
			this.txtSearchName.Location = new System.Drawing.Point(168, 21);
			this.txtSearchName.Name = "txtSearchName";
			this.txtSearchName.Size = new System.Drawing.Size(364, 22);
			this.txtSearchName.TabIndex = 28;
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(25, 24);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(112, 16);
			this.label2.TabIndex = 27;
			this.label2.Text = "File/Folder Name";
			// 
			// pnlFolderSelector
			// 
			this.pnlFolderSelector.Controls.Add(this.btnLoadTreeview);
			this.pnlFolderSelector.Controls.Add(this.txtFileLocation);
			this.pnlFolderSelector.Controls.Add(this.lblHierarchyName);
			this.pnlFolderSelector.Dock = System.Windows.Forms.DockStyle.Top;
			this.pnlFolderSelector.Location = new System.Drawing.Point(0, 0);
			this.pnlFolderSelector.Name = "pnlFolderSelector";
			this.pnlFolderSelector.Size = new System.Drawing.Size(1036, 52);
			this.pnlFolderSelector.TabIndex = 15;
			// 
			// btnLoadTreeview
			// 
			this.btnLoadTreeview.Location = new System.Drawing.Point(800, 12);
			this.btnLoadTreeview.Name = "btnLoadTreeview";
			this.btnLoadTreeview.Size = new System.Drawing.Size(82, 25);
			this.btnLoadTreeview.TabIndex = 16;
			this.btnLoadTreeview.Text = "Load Treeview";
			this.btnLoadTreeview.UseVisualStyleBackColor = true;
			this.btnLoadTreeview.Click += new System.EventHandler(this.btnLoadTreeview_Click);
			// 
			// txtFileLocation
			// 
			this.txtFileLocation.BackColor = System.Drawing.SystemColors.ButtonHighlight;
			this.txtFileLocation.Location = new System.Drawing.Point(123, 12);
			this.txtFileLocation.Name = "txtFileLocation";
			this.txtFileLocation.ReadOnly = true;
			this.txtFileLocation.Size = new System.Drawing.Size(629, 22);
			this.txtFileLocation.TabIndex = 14;
			// 
			// lblHierarchyName
			// 
			this.lblHierarchyName.AutoSize = true;
			this.lblHierarchyName.Location = new System.Drawing.Point(15, 12);
			this.lblHierarchyName.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.lblHierarchyName.Name = "lblHierarchyName";
			this.lblHierarchyName.Size = new System.Drawing.Size(90, 16);
			this.lblHierarchyName.TabIndex = 13;
			this.lblHierarchyName.Text = "File Hierarchy";
			// 
			// pnlBottom
			// 
			this.pnlBottom.Controls.Add(this.splitButton1);
			this.pnlBottom.Controls.Add(this.label4);
			this.pnlBottom.Controls.Add(this.btnSaveLocation);
			this.pnlBottom.Controls.Add(this.txtNewItemLocation);
			this.pnlBottom.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.pnlBottom.Location = new System.Drawing.Point(0, 763);
			this.pnlBottom.Name = "pnlBottom";
			this.pnlBottom.Size = new System.Drawing.Size(1036, 50);
			this.pnlBottom.TabIndex = 13;
			// 
			// label4
			// 
			this.label4.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.label4.AutoSize = true;
			this.label4.Location = new System.Drawing.Point(7, 16);
			this.label4.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(83, 16);
			this.label4.TabIndex = 12;
			this.label4.Text = "File Location";
			// 
			// btnSaveLocation
			// 
			this.btnSaveLocation.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.btnSaveLocation.AutoSize = true;
			this.btnSaveLocation.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.btnSaveLocation.Location = new System.Drawing.Point(768, 12);
			this.btnSaveLocation.Name = "btnSaveLocation";
			this.btnSaveLocation.Size = new System.Drawing.Size(26, 26);
			this.btnSaveLocation.TabIndex = 14;
			this.btnSaveLocation.Text = "...";
			this.btnSaveLocation.UseVisualStyleBackColor = true;
			this.btnSaveLocation.Click += new System.EventHandler(this.btnSaveLocation_Click);
			// 
			// txtNewItemLocation
			// 
			this.txtNewItemLocation.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.txtNewItemLocation.Location = new System.Drawing.Point(112, 14);
			this.txtNewItemLocation.Name = "txtNewItemLocation";
			this.txtNewItemLocation.Size = new System.Drawing.Size(650, 22);
			this.txtNewItemLocation.TabIndex = 13;
			// 
			// splitContainer2
			// 
			this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitContainer2.Location = new System.Drawing.Point(0, 0);
			this.splitContainer2.Name = "splitContainer2";
			this.splitContainer2.Size = new System.Drawing.Size(1542, 813);
			this.splitContainer2.SplitterDistance = 514;
			this.splitContainer2.TabIndex = 2;
			// 
			// thumbViewer
			// 
			this.thumbViewer.AutoScroll = true;
			this.thumbViewer.AutoSize = true;
			this.thumbViewer.Location = new System.Drawing.Point(19, 92);
			this.thumbViewer.Margin = new System.Windows.Forms.Padding(4);
			this.thumbViewer.Name = "thumbViewer";
			this.thumbViewer.RootFolderPath = null;
			this.thumbViewer.Size = new System.Drawing.Size(985, 615);
			this.thumbViewer.TabIndex = 36;
			// 
			// splitButton1
			// 
			this.splitButton1.Location = new System.Drawing.Point(826, 12);
			this.splitButton1.Name = "splitButton1";
			this.splitButton1.Size = new System.Drawing.Size(125, 29);
			this.splitButton1.TabIndex = 16;
			// 
			// sfdFileSaver
			// 
			this.sfdFileSaver.Title = "Save File to Disk";
			// 
			// frmDBFilesBrowser
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(1542, 813);
			this.Controls.Add(this.splitContainer1);
			this.Controls.Add(this.splitContainer2);
			this.Name = "frmDBFilesBrowser";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "DB Files Browser";
			this.Resize += new System.EventHandler(this.frmDBFilesBrowser_Resize);
			this.splitContainer1.Panel1.ResumeLayout(false);
			this.splitContainer1.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
			this.splitContainer1.ResumeLayout(false);
			this.grpFolderDetails.ResumeLayout(false);
			this.grpFolderDetails.PerformLayout();
			this.pnlFolderSelector.ResumeLayout(false);
			this.pnlFolderSelector.PerformLayout();
			this.pnlBottom.ResumeLayout(false);
			this.pnlBottom.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
			this.splitContainer2.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.OpenFileDialog ofdFilePicker;
		private System.Windows.Forms.ImageList imlShowPad;
		private System.Windows.Forms.SplitContainer splitContainer1;
		private System.Windows.Forms.TreeView tvwDirTree;
		private System.Windows.Forms.ImageList iml4TreeView;
		private System.Windows.Forms.Panel pnlBottom;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.Button btnSaveLocation;
		private System.Windows.Forms.TextBox txtNewItemLocation;
		private System.Windows.Forms.Panel pnlFolderSelector;
		private System.Windows.Forms.Button btnLoadTreeview;
		private System.Windows.Forms.TextBox txtFileLocation;
		private System.Windows.Forms.Label lblHierarchyName;
		private System.Windows.Forms.GroupBox grpFolderDetails;
		private ThumbnailViewer thumbViewer;
		private System.Windows.Forms.Button btnSearchDuplicates;
		private System.Windows.Forms.Button btnSearchByRegex;
		private System.Windows.Forms.Button btnSearchByExtn;
		private System.Windows.Forms.Button btnDecolorizeTreeview;
		private System.Windows.Forms.ComboBox cboSearchType;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Button btnSearch;
		private System.Windows.Forms.TextBox txtSearchName;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.SplitContainer splitContainer2;
		private SplitButton splitButton1;
		private System.Windows.Forms.SaveFileDialog sfdFileSaver;
	}
}