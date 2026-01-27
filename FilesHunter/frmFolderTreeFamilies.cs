using StorageAnalyzerService;
using StorageAnalyzerService.DbModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace FilesHunter
{
	public partial class frmFolderTreeFamilies : Form
	{
		public string SelectedFolderTree {  get; private set; }
		public frmFolderTreeFamilies()
		{
			InitializeComponent();
		}

		private void frmFolderTreeFamilies_Load(object sender, EventArgs e)
		{
			try
			{
                fbdFolderLocation.RootFolder = Environment.SpecialFolder.MyComputer;

                MongoDbRepository reader = new MongoDbRepository();
                var allMaps = reader.GetAllFolderMaps();
                var mapNames = allMaps.Select(map => map.AbsolutePath).ToList();
                foreach (var mapName in mapNames)
                {
                    lstFolderHrchies.Items.Add(mapName);
                }
            }
            catch (Exception ex)
			{
                if (ex.Message.CompareTo("The underlying provider failed on Open.") == 0)
                    this.Close();
			}
		}

        private void btnOpenSelected_Click(object sender, EventArgs e)
		{
			if (lstFolderHrchies.Items.Count == 0 || lstFolderHrchies.SelectedItems.Count == 0)
				return;
			this.SelectedFolderTree = lstFolderHrchies.SelectedItem.ToString();
			this.DialogResult = DialogResult.OK;
			this.Close();
		}

		private void btnOpenDialog_Click(object sender, EventArgs e)
		{
			DialogResult result = fbdFolderLocation.ShowDialog();
			if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbdFolderLocation.SelectedPath))
			{
				txtFileLocation.Text = fbdFolderLocation.SelectedPath;
			}
		}

        private void btnSaveFolderData_Click(object sender, EventArgs e)
        {
            MongoDbRepository saver = new MongoDbRepository();
            saver.RootFolderPath = txtFileLocation.Text.Trim();
            saver.SaveMap();
            //Also add item to the list
            lstFolderHrchies.Items.Add(saver.RootFolderPath);
        }

        private void btnSaveFolderData_Click_Old(object sender, EventArgs e)
		{
			MongoDbRepository saver = new MongoDbRepository();
			saver.RootFolderPath = txtFileLocation.Text.Trim();
			saver.SaveMap();
			//Also add item to the list
			lstFolderHrchies.Items.Add(saver.RootFolderPath);
		}

		private void btnDeleteSelected_Click(object sender, EventArgs e)
		{
			if (lstFolderHrchies.Items.Count == 0 || lstFolderHrchies.SelectedItems.Count == 0)
				return;
			var saver = new MongoDbRepository();
			saver.DeleteFolderMap(lstFolderHrchies.SelectedItem.ToString());
			//Also remove item from the list
			lstFolderHrchies.Items.RemoveAt(lstFolderHrchies.SelectedIndex);
		}

		private void lstFolderHrchies_MouseDoubleClick(object sender, MouseEventArgs e)
		{
			if (lstFolderHrchies.SelectedItem == null) return;
			this.SelectedFolderTree = lstFolderHrchies.SelectedItem.ToString();
			this.DialogResult = DialogResult.OK;
			this.Close();
		}

        private void btnSave2Disk_Click(object sender, EventArgs e)
        {
			//MessageBox.Show($"Saving {lstFolderHrchies.SelectedItem.ToString()} to disk");
            var reader = new MongoDbRepository();
			var rootFolderPath = lstFolderHrchies.SelectedItem.ToString();
			reader.RootFolderPath = rootFolderPath;
			var folderMap = reader.GetMap();
            var rootNode = folderMap.DocumentElement as XmlNode;
            Action<XmlNode> saveFileToDisk = (XmlNode curNode) =>
            {
                var resourceDbId = curNode.Attributes["DbId"].Value;
                var fileData = reader.GetModakData(resourceDbId);
                var childFilePathName = curNode.Attributes["name"].Value;
                var iNode = curNode;
                while (iNode.ParentNode != null && iNode.ParentNode.Name != "#document")
                {
                    childFilePathName = iNode.ParentNode.Attributes["name"].Value.TrimEnd('\\') + @"\" + childFilePathName;
                    iNode = iNode.ParentNode;
                }
                childFilePathName = childFilePathName.Substring(childFilePathName.IndexOf(@"\") + 1);
                childFilePathName = Path.Combine(rootFolderPath, childFilePathName);
                //Debug.WriteLine("Writing file: " + childFilePathName + " to disk");
                File.WriteAllBytes(childFilePathName, fileData);
            };
            Action<XmlNode> createFolderOnDisk = (XmlNode curNode) =>
            {
                var childFolderName = curNode.Attributes["name"].Value;
                var iNode = curNode;
                while (iNode.ParentNode != null && iNode.ParentNode.Name != "#document")
                {
                    childFolderName = iNode.ParentNode.Attributes["name"].Value.TrimEnd('\\') + @"\" + childFolderName;
                    iNode = iNode.ParentNode;
                }
                childFolderName = childFolderName.Substring(childFolderName.IndexOf(@"\") + 1);
                childFolderName = Path.Combine(rootFolderPath, childFolderName);
                //Debug.WriteLine("creating Folder: " + childFolderName);
                Directory.CreateDirectory(childFolderName);
            };

            fbdFolderLocation.SelectedPath = reader.RootFolderPath;
            //Allow user to change default save path
            var dialogResult = fbdFolderLocation.ShowDialog();
            if (dialogResult == DialogResult.OK)
            {
                rootFolderPath = fbdFolderLocation.SelectedPath; //changing root folder path to user selection 
                IterateThruXmlHierarchy(rootNode, saveFileToDisk, createFolderOnDisk);
            }
        }

        private void IterateThruXmlHierarchy(XmlNode parentNode, Action<XmlNode> endNodeActionToCall, Action<XmlNode> interimNodeActionToCall)
        {
            interimNodeActionToCall(parentNode);
            var childNodes = parentNode.ChildNodes;
            foreach (XmlNode childNode in childNodes)
            {
                if (childNode.Name == "file")
                {
                    endNodeActionToCall(childNode);
                }
                else
                {
                    IterateThruXmlHierarchy(childNode, endNodeActionToCall, interimNodeActionToCall);
                }
            }
        }

    }
}
