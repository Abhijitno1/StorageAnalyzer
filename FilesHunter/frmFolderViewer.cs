﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using StorageAnalyzerService;
using System.Diagnostics;
using System.IO;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;

namespace FilesHunter
{
    public partial class frmFolderViewer : Form
    {
        public frmFolderViewer()
        {
            InitializeComponent();
            this.Load += frmFolderViewer_Load;
        }

        void frmFolderViewer_Load(object sender, EventArgs e)
        {
            fbdFolderLocation.RootFolder = Environment.SpecialFolder.MyComputer;
        }

        private void btnOpenDialog_Click(object sender, EventArgs e)
        {
            DialogResult result = fbdFolderLocation.ShowDialog();
            if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbdFolderLocation.SelectedPath))
            {
                txtFileLocation.Text = fbdFolderLocation.SelectedPath;
            }
        }

        private void btnLoadTreeview_Click(object sender, EventArgs e)
        {
            DirNodeTreeBuilder browser = new DirNodeTreeBuilder();
            browser.InputFolderPath = txtFileLocation.Text.Trim();
            browser.FolderImageIndex = 0;
            browser.FileImageIndex = 1;
            var rootNode = browser.BuildNodesForTreeView();
            tvwDirTree.Nodes.Add(rootNode);
            tvwDirTree.PrepareForFiltering();
            //Also set the root folder path for ThumbViewer control
            thumbViewer.RootFolderPath = new DirectoryInfo(txtFileLocation.Text.Trim()).Parent.FullName;
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            DirectoryMapReader searcher = new DirectoryMapReader();
            searcher.InputFilePathName = txtFileLocation.Text.Trim();
            string what2Search = txtSearchName.Text.Trim();
            var result = searcher.SearchFileByName(what2Search, "FilePath");
            DisplayResults(result);
        }

        private void ClearSearchResultsInTreeview()
        {
            if (tvwDirTree.Nodes.Count > 0)
            {
              //tvwDirTree.BackColor = Color.White;
              var rootNode = tvwDirTree.Nodes[0];
              DecolorizeTreeNode(rootNode);
            }
        }

        private void ShowSearchResultsInTreeview(string foundItem)
        {
            var foundNodes = tvwDirTree.Nodes.Find(foundItem, true);
            if (foundNodes.Any())
                foundNodes[0].BackColor= Color.Yellow;
            
        }

        private void DecolorizeTreeNode(TreeNode currentNode)
        {
            currentNode.BackColor = Color.White;
            foreach (var child in currentNode.Nodes)
            {
                DecolorizeTreeNode(child as TreeNode);
            }
        }

        private void btnDecolorizeTreeview_Click(object sender, EventArgs e)
        {
            //ClearSearchResultsInTreeview();
            tvwDirTree.ClearFilters();
        }

        private void btnSaveLocation_Click(object sender, EventArgs e)
        {
            ofdFileLocation.CheckFileExists = false;
            var dlgResult = ofdFileLocation.ShowDialog();
            if (dlgResult != DialogResult.Cancel)
            {
                txtSaveLocation.Text = ofdFileLocation.FileName;
            }
        }

        private void btnSaveResults_Click(object sender, EventArgs e)
        {
            var writer = File.CreateText(txtSaveLocation.Text.Trim());
            //foreach (var item in lstSearchResults.Items)
            //{
            //    writer.WriteLine(item.ToString());
            //}
            writer.Close();
            MessageBox.Show("File successfully created");
        }

        private void btnSearchDuplicates_Click(object sender, EventArgs e)
        {
            //lstSearchResults.Items.Clear();
            DirectoryMapReader searcher = new DirectoryMapReader();
            searcher.InputFilePathName = txtFileLocation.Text.Trim();
            var result2 = searcher.SearchXactDuplicates();
            if (result2.Any())
            {
                var iterator = result2.GetEnumerator();
                while (iterator.MoveNext())
                {
                    //lstSearchResults.Items.Add(string.Format("Duplicate group {0}:", iterator.Current.Key));
                    //iterator.Current.Value.ForEach(file => lstSearchResults.Items.Add(file));
                }
            }
            else
            {
                //lstSearchResults.Items.Add("No duplicates found");
            }
        }

        private void btnSearchByRegex_Click(object sender, EventArgs e)
        {
            DirectoryMapReader searcher = new DirectoryMapReader();
            searcher.InputFilePathName = txtFileLocation.Text.Trim();
            string errorMessage;
            var result = searcher.SearchFileUsingRegEx(txtSearchName.Text, out errorMessage);
            if (errorMessage.Length == 0)
                DisplayResults(result);
            else
                MessageBox.Show(errorMessage);
        }

        private void btnSearchByExtn_Click(object sender, EventArgs e)
        {
            var extnList = txtSearchName.Text.Trim().Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            if (extnList.Length == 0) return;
            //lstSearchResults.Items.Clear();
            DirectoryMapReader searcher = new DirectoryMapReader();
            searcher.InputFilePathName = txtFileLocation.Text.Trim();
            var result = searcher.SearchFileByExtensions(extnList);
            DisplayResults(result);
        }

        private void DisplayResults(List<string> result)
        {
            if (result.Any())
            {
                foreach (var entry in result)
                {
                    //lstSearchResults.Items.Add(entry);
                }
            }
            else
            {
                //lstSearchResults.Items.Add("No matching files found for given search criteria");
            }
            tvwDirTree.FilterNodes(result);
        }

        private void tvwDirTree_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Node.Tag.ToString() == NodeType.Folder.ToString())
            {
                PopulateFirstLevelChildrenInThumViewer(e.Node.Name);
            }
        }


        private void PopulateFirstLevelChildrenInThumViewer(string relativeFolderPath)
        {
            thumbViewer.ClearImages();
            var rootParentDirPath = new DirectoryInfo(txtFileLocation.Text.Trim()).Parent.FullName;
            var parentFolderPath = Path.Combine(rootParentDirPath, relativeFolderPath);
            DirectoryInfo directoryInfo = new DirectoryInfo(parentFolderPath);
            foreach (DirectoryInfo dir in directoryInfo.GetDirectories())
            {
                //Ref: https://www.edgeventures.com/kb/post/2017/05/01/resize-images-in-c-extreme-compression
                var folderImage = iml4TreeView.Images[0];
                var ms = new MemoryStream();
                ImageCodecInfo pngCodec = GetEncoderInfo("image/png");
                var myEncoder = System.Drawing.Imaging.Encoder.Quality;
                var encoderParam = new EncoderParameter(myEncoder, 90L);    //Quality level 75
                var myEncoderParameters = new EncoderParameters(1);
                myEncoderParameters.Param[0] = encoderParam;
                folderImage.Save(ms, pngCodec, myEncoderParameters);
                thumbViewer.AddImageItem(ms.GetBuffer(), dir.Name, relativeFolderPath);
            }

        }
        private static ImageCodecInfo GetEncoderInfo(string mimeType)
        {
            foreach (ImageCodecInfo codec in ImageCodecInfo.GetImageEncoders())
                if (codec.MimeType == mimeType)
                    return codec;

            return null;
        }


    }
}