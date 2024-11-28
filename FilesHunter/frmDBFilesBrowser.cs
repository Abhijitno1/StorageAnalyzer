using StorageAnalyzerService;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace FilesHunter
{
	public partial class frmDBFilesBrowser : Form
	{
		private List<string> ImageExtensions = new List<string> { ".JPG", ".JPE", ".BMP", ".GIF", ".PNG" };
		int panel1OrigWidth, panel2OrigWidth, formOrigWidth, formOrigHeight;
		XmlDocument currentFolderNaksha;

		public frmDBFilesBrowser()
		{
			InitializeComponent();
			this.Load += FrmDBFilesBrowser_Load;
			this.thumbViewer.OpenFolderToViewContents += ThumbViewer_OpenFolderToViewContents;
			this.thumbViewer.GetPreviewData += ThumbViewer_GetPreviewData;
			this.thumbViewer.DeleteResource += ThumbViewer_DeleteResource;
			this.panel1OrigWidth = splitContainer1.Panel1.Width;
			this.panel2OrigWidth = splitContainer1.Panel2.Width;
			this.formOrigHeight = this.Height;
		}

		private void ThumbViewer_DeleteResource(string itemName, string itemPath)
		{
			var relativeFolderPath = itemPath.TrimStart('\\') + @"\" + itemName;
			var filterClause = GenerateXPathFilterClauseFromRelativeFolderPath(relativeFolderPath, NodeType.File);
			var selectedNode = currentFolderNaksha.SelectSingleNode(filterClause);
			if (selectedNode != null)
			{
				var resourceDbId = Convert.ToInt32(selectedNode.Attributes["DbId"].Value);
				var parentNode = selectedNode.ParentNode;
				if (parentNode != null)
				{
					parentNode.RemoveChild(selectedNode);
					DirectoryMapDbSaver saver = new DirectoryMapDbSaver();
					saver.DeleteModak(resourceDbId);
					saver.UpdateMap(currentFolderNaksha);
				}
			}
		}

		private void ThumbViewer_GetPreviewData(string itemName, string itemPath, frmMediaPreview.MediaType itemType, out object fileData)
		{
			var relativeFolderPath = itemPath.TrimStart('\\') + @"\" + itemName;
			var filterClause = GenerateXPathFilterClauseFromRelativeFolderPath(relativeFolderPath, NodeType.File);
			var selectedNode = currentFolderNaksha.SelectSingleNode(filterClause);
			if (selectedNode != null)
			{
				DirectoryMapDbReader reader = new DirectoryMapDbReader();
				int fileId = Convert.ToInt32(selectedNode.Attributes["DbId"].Value);
				fileData = reader.GetModak(fileId);
				if (itemType == frmMediaPreview.MediaType.Text)
				{
					fileData = Encoding.Default.GetString((byte[])fileData);
				}
			}
			else 
			{ 
				fileData = null; 
			}
		}

		private void ThumbViewer_OpenFolderToViewContents(string itemName, string itemPath)
		{
			ExpandSelectedFolderInTreeView(itemName);
			thumbViewer.ClearImages();
			PopulateFirstLevelChildrenInThumViewer(itemPath + @"\" + itemName);
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
			DirectoryMapDbSaver saver = new DirectoryMapDbSaver();
			saver.RootFolderPath = txtFileLocation.Text.Trim();
			saver.SaveMap();
		}

		private void btnLoadTreeview_Click(object sender, EventArgs e)
		{
			DirectoryMapDbReader reader = new DirectoryMapDbReader();
			reader.RootFolderPath = txtFileLocation.Text.Trim();
			currentFolderNaksha = reader.GetMap();
			NodeTreeBuilder browser = new NodeTreeBuilder();
			browser.FolderImageIndex = 0;
			browser.FileImageIndex = 1;
			var rootNode = browser.BuildNodesForTreeView(currentFolderNaksha);
			tvwDirTree.Nodes.Add(rootNode);
			tvwDirTree.PrepareForFiltering();
		}

		private void FrmDBFilesBrowser_Load(object sender, EventArgs e)
		{
			fbdFolderLocation.RootFolder = Environment.SpecialFolder.MyComputer;
		}

		private string GenerateXPathFilterClauseFromRelativeFolderPath(string relativeFolderPath, NodeType nodeType)
		{
			var folderSegments = relativeFolderPath.Split('\\');
			var filterClause = "//";
			int j = 0;
			for (j = 0; j < folderSegments.Length - 1; j++)
			{
				var segment = folderSegments[j];
				filterClause += $"folder[@name='{segment.ToString()}']/";
			}
			if (folderSegments.Length > 0)
			{
				var fileOrFolder = Enum.GetName(typeof(NodeType), nodeType).ToLower();
				filterClause += $"{fileOrFolder}[@name='{folderSegments[j].ToString()}']/";
			}
			filterClause = filterClause.TrimEnd('/');
			return filterClause;
		}

		private void PopulateFirstLevelChildrenInThumViewer(string relativeFolderPath)
		{
			thumbViewer.ClearImages();
			var filterClause = GenerateXPathFilterClauseFromRelativeFolderPath(relativeFolderPath, NodeType.Folder);
			var selectedNode = currentFolderNaksha.SelectSingleNode(filterClause);
			DirectoryMapDbReader reader = new DirectoryMapDbReader();

			for (int i = 0; i < selectedNode.ChildNodes.Count; i++)
			{
				XmlNode childNode = selectedNode.ChildNodes[i];
				Byte[] imageData = null;
				if (childNode.Name == "folder")
				{
					var folderName = childNode.Attributes["name"].Value;
					//Ref: https://www.edgeventures.com/kb/post/2017/05/01/resize-images-in-c-extreme-compression
					var folderImage = imlShowPad.Images[0];
					imageData = ThumbnailViewer.ImageToBinary(folderImage);
					thumbViewer.AddImageItem(NodeType.Folder, imageData, folderName, relativeFolderPath);
				}
				else if (childNode.Name == "file")
				{
					var fileName = childNode.Attributes["name"].Value;
					var fileExtn = Path.GetExtension(fileName);
					if ((new string[] { ".rtf", ".txt" }).Contains(fileExtn))
					{
						imageData = ThumbnailViewer.ImageToBinary(imlShowPad.Images[2]);
					}
					else if ((new string[] { ".doc", ".docx" }).Contains(fileExtn))
					{
						imageData = ThumbnailViewer.ImageToBinary(imlShowPad.Images[3]);
					}
					else if ((new string[] { ".pdf" }).Contains(fileExtn))
					{
						imageData = ThumbnailViewer.ImageToBinary(imlShowPad.Images[4]);
					}
					else if ((new string[] { ".mp4", ".wmv", ".asf" }).Contains(fileExtn))
					{
						imageData = ThumbnailViewer.ImageToBinary(imlShowPad.Images[5]);
					}
					else if ((new string[] { ".mp3", ".wma" }).Contains(fileExtn))
					{
						imageData = ThumbnailViewer.ImageToBinary(imlShowPad.Images[6]);
					}
					else if ((new string[] { ".jpg", ".jpeg", ".png", ".gif", ".avif", ".webp", ".tiff", ".bmp", ".svg" }).Contains(fileExtn))
					{
						var fileId = Convert.ToInt32(childNode.Attributes["DbId"].Value);
						imageData = reader.GetModak(fileId);
					}
					else
					{
						imageData = ThumbnailViewer.ImageToBinary(imlShowPad.Images[1]);
					}
					thumbViewer.AddImageItem(NodeType.File, imageData, childNode.Attributes["name"].Value, relativeFolderPath);
				}
			}
		}

		private void ExpandSelectedFolderInTreeView(string foundItem)
		{
			var foundNode = FindTreeNode(tvwDirTree.Nodes[0], foundItem);
			if (foundNode != null)
			{
				tvwDirTree.SelectedNode = foundNode;
				foundNode.Expand();
			}
		}

		private void frmDBFilesBrowser_Resize(object sender, EventArgs e)
		{
			if (this.formOrigHeight == 0) return;	//Dont do anything on initial form load

			var heightDiff = this.Height - this.formOrigHeight;
			//var widthDiff = this.Width - this.formOrigWidth;
			//tvwDirTree.Width += widthDiff / 2;
			tvwDirTree.Height += heightDiff;
			//thumbViewer.Width += widthDiff / 2;
			thumbViewer.Height += heightDiff;
			grpFolderDetails.Height += heightDiff;

			this.formOrigHeight = this.Height;
			//this.formOrigWidth = this.Width;
		}

		private void tvwDirTree_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
		{
			bool proceed = false;
			if (e.Button == MouseButtons.left)
			{
				var focusedItem = tvwDirTree.SelectedNode;
				//Trying to check whether node action is select, not expand
				if (focusedItem != null && focusedItem.Bounds.Contains(e.Location))
				{
					proceed = true;
				}
			}

			if (proceed && e.Node.Tag.ToString().ToLower() == NodeType.Folder.ToString().ToLower())
			{
				var rootParentDirPath = new DirectoryInfo(txtFileLocation.Text.Trim()).Parent.FullName;
				var offset = rootParentDirPath.Length + 1; //We include the // suffix of parent folder hierarchy for calculating string omission offset
				var relativePath = e.Node.Name.Substring(offset);
				PopulateFirstLevelChildrenInThumViewer(relativePath);
			}
		}

		private void splitContainer1_SplitterMoved(object sender, SplitterEventArgs e)
		{
			tvwDirTree.Width += splitContainer1.Panel1.Width - panel1OrigWidth;
			thumbViewer.Width += splitContainer1.Panel2.Width - panel2OrigWidth;
			grpFolderDetails.Width += splitContainer1.Panel2.Width - panel2OrigWidth;

			//Reset the panel widths to track future changes
			panel1OrigWidth = splitContainer1.Panel1.Width;
			panel2OrigWidth = splitContainer1.Panel2.Width;
		}

		//Ref: https://stackoverflow.com/questions/23091773/find-treeview-node-recursively
		private TreeNode FindTreeNode(TreeNode node, string value2Find)
		{
			foreach (TreeNode child in node.Nodes)
			{
				if (child.Text == value2Find)
				{
					return child;
				}

				if (child.Nodes.Count > 0)
				{
					TreeNode found = FindTreeNode(child, value2Find);
					if (found != null)
					{
						return found;
					}
				}
			}
			return null;
		}
	}
}
