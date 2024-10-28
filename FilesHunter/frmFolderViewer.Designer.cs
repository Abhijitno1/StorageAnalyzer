namespace FilesHunter
{
    partial class frmFolderViewer
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmFolderViewer));
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.iml4TreeView = new System.Windows.Forms.ImageList(this.components);
            this.btnLoadTreeview = new System.Windows.Forms.Button();
            this.btnOpenDialog = new System.Windows.Forms.Button();
            this.txtFileLocation = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.btnSearchDuplicates = new System.Windows.Forms.Button();
            this.btnSearchByRegex = new System.Windows.Forms.Button();
            this.btnSearchByExtn = new System.Windows.Forms.Button();
            this.btnSaveResults = new System.Windows.Forms.Button();
            this.btnSaveLocation = new System.Windows.Forms.Button();
            this.txtSaveLocation = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.btnDecolorizeTreeview = new System.Windows.Forms.Button();
            this.cboSearchType = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.btnSearch = new System.Windows.Forms.Button();
            this.txtSearchName = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.tvwDirTree = new System.Windows.Forms.TreeView();
            this.imlShowPad = new System.Windows.Forms.ImageList(this.components);
            this.lvFilesShow = new System.Windows.Forms.ListView();
            this.fbdFolderLocation = new System.Windows.Forms.FolderBrowserDialog();
            this.ofdFileLocation = new System.Windows.Forms.OpenFileDialog();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
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
            this.splitContainer1.Panel2.Controls.Add(this.btnLoadTreeview);
            this.splitContainer1.Panel2.Controls.Add(this.btnOpenDialog);
            this.splitContainer1.Panel2.Controls.Add(this.txtFileLocation);
            this.splitContainer1.Panel2.Controls.Add(this.label1);
            this.splitContainer1.Size = new System.Drawing.Size(1487, 804);
            this.splitContainer1.SplitterDistance = 495;
            this.splitContainer1.SplitterWidth = 5;
            this.splitContainer1.TabIndex = 0;
            // 
            // iml4TreeView
            // 
            this.iml4TreeView.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("iml4TreeView.ImageStream")));
            this.iml4TreeView.TransparentColor = System.Drawing.Color.Transparent;
            this.iml4TreeView.Images.SetKeyName(0, "folder.ico");
            this.iml4TreeView.Images.SetKeyName(1, "Generic_Document.ico");
            this.iml4TreeView.Images.SetKeyName(2, "folder_open.ico");
            // 
            // btnLoadTreeview
            // 
            this.btnLoadTreeview.Location = new System.Drawing.Point(771, 9);
            this.btnLoadTreeview.Name = "btnLoadTreeview";
            this.btnLoadTreeview.Size = new System.Drawing.Size(110, 25);
            this.btnLoadTreeview.TabIndex = 3;
            this.btnLoadTreeview.Text = "Load Treeview";
            this.btnLoadTreeview.UseVisualStyleBackColor = true;
            this.btnLoadTreeview.Click += new System.EventHandler(this.btnLoadTreeview_Click);
            // 
            // btnOpenDialog
            // 
            this.btnOpenDialog.AutoSize = true;
            this.btnOpenDialog.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnOpenDialog.Location = new System.Drawing.Point(735, 7);
            this.btnOpenDialog.Name = "btnOpenDialog";
            this.btnOpenDialog.Size = new System.Drawing.Size(31, 30);
            this.btnOpenDialog.TabIndex = 2;
            this.btnOpenDialog.Text = "...";
            this.btnOpenDialog.UseVisualStyleBackColor = true;
            this.btnOpenDialog.Click += new System.EventHandler(this.btnOpenDialog_Click);
            // 
            // txtFileLocation
            // 
            this.txtFileLocation.Location = new System.Drawing.Point(117, 9);
            this.txtFileLocation.Name = "txtFileLocation";
            this.txtFileLocation.Size = new System.Drawing.Size(608, 26);
            this.txtFileLocation.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(8, 9);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(105, 20);
            this.label1.TabIndex = 0;
            this.label1.Text = "File Location";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.lvFilesShow);
            this.groupBox1.Controls.Add(this.btnSearchDuplicates);
            this.groupBox1.Controls.Add(this.btnSearchByRegex);
            this.groupBox1.Controls.Add(this.btnSearchByExtn);
            this.groupBox1.Controls.Add(this.btnSaveResults);
            this.groupBox1.Controls.Add(this.btnSaveLocation);
            this.groupBox1.Controls.Add(this.txtSaveLocation);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.btnDecolorizeTreeview);
            this.groupBox1.Controls.Add(this.cboSearchType);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.btnSearch);
            this.groupBox1.Controls.Add(this.txtSearchName);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Location = new System.Drawing.Point(512, 54);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(972, 747);
            this.groupBox1.TabIndex = 5;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Search Files or Folders";
            // 
            // btnSearchDuplicates
            // 
            this.btnSearchDuplicates.Location = new System.Drawing.Point(402, 71);
            this.btnSearchDuplicates.Name = "btnSearchDuplicates";
            this.btnSearchDuplicates.Size = new System.Drawing.Size(119, 31);
            this.btnSearchDuplicates.TabIndex = 14;
            this.btnSearchDuplicates.Text = "Search Duplicates";
            this.btnSearchDuplicates.UseVisualStyleBackColor = true;
            // 
            // btnSearchByRegex
            // 
            this.btnSearchByRegex.Location = new System.Drawing.Point(277, 73);
            this.btnSearchByRegex.Name = "btnSearchByRegex";
            this.btnSearchByRegex.Size = new System.Drawing.Size(119, 31);
            this.btnSearchByRegex.TabIndex = 13;
            this.btnSearchByRegex.Text = "Search Using RegEx";
            this.btnSearchByRegex.UseVisualStyleBackColor = true;
            // 
            // btnSearchByExtn
            // 
            this.btnSearchByExtn.Location = new System.Drawing.Point(152, 71);
            this.btnSearchByExtn.Name = "btnSearchByExtn";
            this.btnSearchByExtn.Size = new System.Drawing.Size(119, 34);
            this.btnSearchByExtn.TabIndex = 12;
            this.btnSearchByExtn.Text = "Search Multiple Extensions";
            this.btnSearchByExtn.UseVisualStyleBackColor = true;
            // 
            // btnSaveResults
            // 
            this.btnSaveResults.Location = new System.Drawing.Point(802, 719);
            this.btnSaveResults.Name = "btnSaveResults";
            this.btnSaveResults.Size = new System.Drawing.Size(99, 25);
            this.btnSaveResults.TabIndex = 11;
            this.btnSaveResults.Text = "Save Results";
            this.btnSaveResults.UseVisualStyleBackColor = true;
            // 
            // btnSaveLocation
            // 
            this.btnSaveLocation.AutoSize = true;
            this.btnSaveLocation.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnSaveLocation.Location = new System.Drawing.Point(770, 717);
            this.btnSaveLocation.Name = "btnSaveLocation";
            this.btnSaveLocation.Size = new System.Drawing.Size(31, 30);
            this.btnSaveLocation.TabIndex = 10;
            this.btnSaveLocation.Text = "...";
            this.btnSaveLocation.UseVisualStyleBackColor = true;
            // 
            // txtSaveLocation
            // 
            this.txtSaveLocation.Location = new System.Drawing.Point(114, 719);
            this.txtSaveLocation.Name = "txtSaveLocation";
            this.txtSaveLocation.Size = new System.Drawing.Size(650, 26);
            this.txtSaveLocation.TabIndex = 9;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(9, 721);
            this.label4.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(105, 20);
            this.label4.TabIndex = 8;
            this.label4.Text = "File Location";
            // 
            // btnDecolorizeTreeview
            // 
            this.btnDecolorizeTreeview.Location = new System.Drawing.Point(527, 71);
            this.btnDecolorizeTreeview.Name = "btnDecolorizeTreeview";
            this.btnDecolorizeTreeview.Size = new System.Drawing.Size(119, 32);
            this.btnDecolorizeTreeview.TabIndex = 7;
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
            this.cboSearchType.Location = new System.Drawing.Point(622, 26);
            this.cboSearchType.Name = "cboSearchType";
            this.cboSearchType.Size = new System.Drawing.Size(121, 28);
            this.cboSearchType.TabIndex = 5;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(523, 29);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(103, 20);
            this.label3.TabIndex = 4;
            this.label3.Text = "Search Type";
            // 
            // btnSearch
            // 
            this.btnSearch.Location = new System.Drawing.Point(27, 71);
            this.btnSearch.Name = "btnSearch";
            this.btnSearch.Size = new System.Drawing.Size(119, 31);
            this.btnSearch.TabIndex = 3;
            this.btnSearch.Text = "Search By Name";
            this.btnSearch.UseVisualStyleBackColor = true;
            // 
            // txtSearchName
            // 
            this.txtSearchName.Location = new System.Drawing.Point(149, 26);
            this.txtSearchName.Name = "txtSearchName";
            this.txtSearchName.Size = new System.Drawing.Size(364, 26);
            this.txtSearchName.TabIndex = 2;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(6, 29);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(137, 20);
            this.label2.TabIndex = 0;
            this.label2.Text = "File/Folder Name";
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
            // 
            // imlShowPad
            // 
            this.imlShowPad.ColorDepth = System.Windows.Forms.ColorDepth.Depth8Bit;
            this.imlShowPad.ImageSize = new System.Drawing.Size(32, 32);
            this.imlShowPad.TransparentColor = System.Drawing.Color.Transparent;
            // 
            // lvFilesShow
            // 
            this.lvFilesShow.HideSelection = false;
            this.lvFilesShow.LargeImageList = this.imlShowPad;
            this.lvFilesShow.Location = new System.Drawing.Point(13, 127);
            this.lvFilesShow.Name = "lvFilesShow";
            this.lvFilesShow.Size = new System.Drawing.Size(953, 584);
            this.lvFilesShow.SmallImageList = this.imlShowPad;
            this.lvFilesShow.TabIndex = 15;
            this.lvFilesShow.UseCompatibleStateImageBehavior = false;
            // 
            // frmFolderViewer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(10F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1487, 804);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.splitContainer1);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "frmFolderViewer";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Folder Viewer";
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.Button btnLoadTreeview;
        private System.Windows.Forms.Button btnOpenDialog;
        private System.Windows.Forms.TextBox txtFileLocation;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ImageList iml4TreeView;
    private System.Windows.Forms.GroupBox groupBox1;
    private System.Windows.Forms.Button btnSearchDuplicates;
    private System.Windows.Forms.Button btnSearchByRegex;
    private System.Windows.Forms.Button btnSearchByExtn;
    private System.Windows.Forms.Button btnSaveResults;
    private System.Windows.Forms.Button btnSaveLocation;
    private System.Windows.Forms.TextBox txtSaveLocation;
    private System.Windows.Forms.Label label4;
    private System.Windows.Forms.Button btnDecolorizeTreeview;
    private System.Windows.Forms.ComboBox cboSearchType;
    private System.Windows.Forms.Label label3;
    private System.Windows.Forms.Button btnSearch;
    private System.Windows.Forms.TextBox txtSearchName;
    private System.Windows.Forms.Label label2;
    private System.Windows.Forms.TreeView tvwDirTree;
    private System.Windows.Forms.ImageList imlShowPad;
    private System.Windows.Forms.ListView lvFilesShow;
        private System.Windows.Forms.FolderBrowserDialog fbdFolderLocation;
        private System.Windows.Forms.OpenFileDialog ofdFileLocation;
    }
}