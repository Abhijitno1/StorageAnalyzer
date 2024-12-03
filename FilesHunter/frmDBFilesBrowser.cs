using FilesHunter.UserControls;
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
using System.Xml.Linq;

namespace FilesHunter
{
	public partial class frmDBFilesBrowser : Form
	{
		private List<string> ImageExtensions = new List<string> { ".JPG", ".JPE", ".BMP", ".GIF", ".PNG" };
		int panel1OrigWidth, panel2OrigWidth, formOrigWidth, formOrigHeight;
		XmlDocument currentFolderNaksha;
		string currentHierarchyParentPath;
		List<CTreeNode> currentFiltererdNodes= new List<CTreeNode>();

		public frmDBFilesBrowser()
		{
			InitializeComponent();
			this.thumbViewer.OpenFolderToViewContents += ThumbViewer_OpenFolderToViewContents;
			this.thumbViewer.GetPreviewData += ThumbViewer_GetPreviewData;
			this.thumbViewer.DeleteResource += ThumbViewer_DeleteResource;
			this.thumbViewer.SaveResource += ThumbViewer_SaveResource;
			this.thumbViewer.RenameResource += ThumbViewer_RenameResource;
			this.splitButton1.MenuItemClick += SplitButton1_MenuItemClick;
			this.panel1OrigWidth = splitContainer1.Panel1.Width;
			this.panel2OrigWidth = splitContainer1.Panel2.Width;
			this.formOrigHeight = this.Height;
		}

		private void ThumbViewer_RenameResource(string itemName, string itemPath)
		{
			var relativeFolderPath = itemPath.TrimStart('\\') + @"\" + itemName;
			var filterClause = GenerateXPathFilterClauseFromRelativeFolderPath(relativeFolderPath, NodeType.File);
			var selectedNode = currentFolderNaksha.SelectSingleNode(filterClause);
			if (selectedNode != null)
			{
				var newName = WinFormsPrompt.ShowDialog("Please enter new name for selected resource", "Rename Resource in DB");
				if (newName != null)
				{
					DirectoryMapDbSaver dbSaver = new DirectoryMapDbSaver();

					//change resource name in xml node
					selectedNode.Attributes["name"].Value = newName;
					dbSaver.UpdateMap(currentFolderNaksha);

					//change resource name in DB related object
					relativeFolderPath = relativeFolderPath.Substring(0, relativeFolderPath.LastIndexOf('\\') +1) + newName;
					Modak modak = new Modak()
					{
						Id = Convert.ToInt32(selectedNode.Attributes["DbId"].Value),
						Title = newName,
						RelativePath = relativeFolderPath
					};
					dbSaver.UpdateModak(modak);

					//Refresh the treeview and listview
					TreeViewRefreshState();
				}
			}
		}
		private void ThumbViewer_SaveResource(string itemName, string itemPath)
		{
			var relativeFolderPath = itemPath.TrimStart('\\') + @"\" + itemName;
			var filterClause = GenerateXPathFilterClauseFromRelativeFolderPath(relativeFolderPath, NodeType.File);
			var selectedNode = currentFolderNaksha.SelectSingleNode(filterClause);
			if (selectedNode != null)
			{
				var resourceDbId = Convert.ToInt32(selectedNode.Attributes["DbId"].Value);
				//Strip off root folder name from relativeFolderPath variable while creating absolute folder path for default save location
				var saveAbsolutePath = currentFolderNaksha.DocumentElement.Attributes["fullPath"].Value + relativeFolderPath.Substring(relativeFolderPath.IndexOf('\\'));
				sfdFileSaver.FileName = saveAbsolutePath;	//ToDo: Set Initial directory and filename separately instead of full path for file name here
				var dlgResult = sfdFileSaver.ShowDialog();
				if (dlgResult == DialogResult.OK)
				{
					saveAbsolutePath = sfdFileSaver.FileName;
					DirectoryMapDbReader dbReader = new DirectoryMapDbReader();
					var fileData = dbReader.GetModak(resourceDbId);
					File.WriteAllBytes(saveAbsolutePath, fileData);
				}
			}
		}

		private void SplitButton1_MenuItemClick(object sender, EventArgs e)
		{
			//MessageBox.Show("You clicked " + (sender as ToolStripMenuItem).Tag.ToString(), "Zoomri Tallaiyah");
			DirectoryMapDbSaver saver = new DirectoryMapDbSaver();
			var selectedItemPath = thumbViewer.GetSelectedItemPath();
			var selectedCommand = (sender as ToolStripMenuItem).Tag.ToString();
			XmlElement newElm = null;
			//Omit the topmost folder of hierarchy for storing relativ path in DB.
			var modakRelPath = string.Empty;
			if (currentHierarchyParentPath.IndexOf('\\', 1) > -1)
				modakRelPath = currentHierarchyParentPath.Substring(currentHierarchyParentPath.IndexOf('\\', 1));

			Func<string, bool> isFileOperation = (string selectCommand) => selectedCommand != "folderaddatend";
			if (isFileOperation(selectedCommand))
			{
				//ToDo: Add selected file validation
				var inputFilePathName = txtNewItemLocation.Text.Trim();
				var fileName = Path.GetFileName(inputFilePathName);
				var stream = File.OpenRead(inputFilePathName);
				var fileSize = stream.Length;
				var creationDateTime = File.GetCreationTime(inputFilePathName);
				var fileData = new byte[fileSize];
				//ToDo: Optiomize this file read in future
				stream.Read(fileData, 0, (int)fileSize);
				stream.Dispose();
				Modak modak = new Modak()
				{
					Title = fileName,
					PicData = fileData,
					RelativePath = modakRelPath + "\\" + fileName
				};
				newElm = currentFolderNaksha.CreateElement("file");
				newElm.SetAttribute("name", fileName);
				newElm.SetAttribute("extension", Path.GetExtension(fileName));
				newElm.SetAttribute("creationDate", creationDateTime.ToString("dd-MMM-yyyy"));
				newElm.SetAttribute("size", fileSize.ToString());

				switch (selectedCommand)
				{
					case "fileaddatend":
						var filterClause = GenerateXPathFilterClauseFromRelativeFolderPath(currentHierarchyParentPath, NodeType.Folder);
						var selectedXmlNode = currentFolderNaksha.SelectSingleNode(filterClause);
						if (selectedXmlNode != null)
							selectedXmlNode.AppendChild(newElm);
						break;
					case "fileinsertbefore":
						if (selectedItemPath == null)
						{
							MessageBox.Show("Please select an item from listview to insert file before", "Validation Error");
							return;
						}
						filterClause = GenerateXPathFilterClauseFromRelativeFolderPath(selectedItemPath, NodeType.File);
						selectedXmlNode = currentFolderNaksha.SelectSingleNode(filterClause);
						if (selectedXmlNode != null && selectedXmlNode.ParentNode != null)
							selectedXmlNode.ParentNode.InsertBefore(newElm, selectedXmlNode);

						break;
					case "fileinsertafter":
						if (selectedItemPath == null)
						{
							MessageBox.Show("Please select an item from listview to insert file after", "Validation Error");
							return;
						}
						filterClause = GenerateXPathFilterClauseFromRelativeFolderPath(selectedItemPath, NodeType.File);
						selectedXmlNode = currentFolderNaksha.SelectSingleNode(filterClause);
						if (selectedXmlNode != null && selectedXmlNode.ParentNode != null)
							selectedXmlNode.ParentNode.InsertAfter(newElm, selectedXmlNode);

						break;
				}

				saver.InsertModakIntoDb(modak);
				newElm.SetAttribute("DbId", modak.Id.ToString());
			}
			else //Add folder at end of currently selected folder view
			{
				var folderName = txtNewItemLocation.Text.Trim();
				if (string.IsNullOrWhiteSpace(folderName))
				{
					MessageBox.Show("Please enter a folder name to add", "Validation Error");
					return;
				}
				newElm = currentFolderNaksha.CreateElement("folder");
				//File: name, extension, creationdate, size | folder: name, creationdate
				newElm.SetAttribute("name", folderName);
				newElm.SetAttribute("creationDate", DateTime.Today.ToString("dd-MMM-yyyy"));
				var filterClause = GenerateXPathFilterClauseFromRelativeFolderPath(currentHierarchyParentPath, NodeType.Folder);
				var selectedXmlNode = currentFolderNaksha.SelectSingleNode(filterClause);
				if (selectedXmlNode != null)
					selectedXmlNode.AppendChild(newElm);
				else
					currentFolderNaksha.DocumentElement.AppendChild(newElm);
			}
			saver.UpdateMap(currentFolderNaksha);

			//Refresh the treeview and listview
			TreeViewRefreshState();
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

					//Refresh the treeview and listview
					TreeViewRefreshState();
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
					fileData = Encoding.UTF8.GetString((byte[])fileData);
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
			currentHierarchyParentPath = itemPath + @"\" + itemName;
			PopulateFirstLevelChildrenInThumViewer();
		}

		private void btnLoadTreeview_Click(object sender, EventArgs e)
		{
			using (var form = new frmFolderTreeFamilies())
			{
				var result = form.ShowDialog();
				if (result == DialogResult.OK)
				{
					txtFileLocation.Text = form.SelectedFolderTree;
					DirectoryMapDbReader reader = new DirectoryMapDbReader();
					reader.RootFolderPath = form.SelectedFolderTree;
					currentFolderNaksha = reader.GetMap();
					TreeViewRefreshState();
				}
			}
		}

		private void TreeViewRefreshState()
		{
			CNodeTreeBuilder browser = new CNodeTreeBuilder();
			browser.FolderImageIndex = 0;
			browser.FileImageIndex = 1;
			var rootNode = browser.BuildNodesForTreeView(currentFolderNaksha);
			tvwDirTree.Nodes.Clear();
			tvwDirTree.Nodes.Add(rootNode);
			tvwDirTree.PrepareForFiltering();

			//Select first node in treeview by default
			rootNode.Expand();
			tvwDirTree.SelectedNode = rootNode;
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


		private void PopulateFirstLevelChildrenInThumViewer()
		{
			thumbViewer.ClearImages();
			var filterClause = GenerateXPathFilterClauseFromRelativeFolderPath(currentHierarchyParentPath, NodeType.Folder);
			var selectedNode = currentFolderNaksha.SelectSingleNode(filterClause);
			DirectoryMapDbReader reader = new DirectoryMapDbReader();

			for (int i = 0; i < selectedNode.ChildNodes.Count; i++)
			{
				XmlNode childNode = selectedNode.ChildNodes[i];
				Byte[] imageData = null;
				if (childNode.Name == "folder")
				{
					var folderName = childNode.Attributes["name"].Value;
					if (currentFiltererdNodes.Any(x => x.Text == folderName && x.Tag.ToString() == childNode.Name))
					{
						//Ref: https://www.edgeventures.com/kb/post/2017/05/01/resize-images-in-c-extreme-compression
						var folderImage = imlShowPad.Images[0];
						imageData = ThumbnailViewer.ImageToBinary(folderImage);
						thumbViewer.AddImageItem(NodeType.Folder, imageData, folderName, currentHierarchyParentPath);
					}
				}
				else if (childNode.Name == "file")
				{
					var fileName = childNode.Attributes["name"].Value;
					if (currentFiltererdNodes.Any(x => x.Text == fileName && x.Tag.ToString() == childNode.Name))
					{
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
						else if ((new string[] { ".jpg", ".jpeg", ".png", ".gif", ".avif", ".webp", ".tiff", ".bmp" }).Contains(fileExtn))
						{
							var fileId = Convert.ToInt32(childNode.Attributes["DbId"].Value);
							imageData = reader.GetModak(fileId);
						}
						else
						{
							imageData = ThumbnailViewer.ImageToBinary(imlShowPad.Images[1]);
						}
						thumbViewer.AddImageItem(NodeType.File, imageData, childNode.Attributes["name"].Value, currentHierarchyParentPath);
					}
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

		private void btnSaveLocation_Click(object sender, EventArgs e)
		{
			var dialogResult = ofdFilePicker.ShowDialog();
			if (dialogResult == DialogResult.OK)
			{
				txtNewItemLocation.Text = ofdFilePicker.FileName;
			}
		}

		private void btnSearch_Click(object sender, EventArgs e)
		{
			tvwDirTree.Filter(FilterMethod);
			//Select first node of tree so that Listview gets refreshed
			tvwDirTree.SelectedNode = null;
			tvwDirTree.SelectedNode = tvwDirTree.Nodes[0];
		}

		private bool FilterMethod(CTreeNode node)
		{
			if (cboSearchType.SelectedItem.ToString().ToLower() == "files only" && node.Tag.ToString() != "file")
				return false;
			if (cboSearchType.SelectedItem.ToString().ToLower() == "folders only" && node.Tag.ToString() != "folder")
				return false;

			return node.Text.ToLower().Contains(txtSearchName.Text.Trim().ToLower());
		}

		private void btnClearFilter_Click(object sender, EventArgs e)
		{
			txtSearchName.Text = string.Empty;
			tvwDirTree.Filter((node) => true);
			//Select first node of tree so that Listview gets refreshed
			tvwDirTree.SelectedNode = null;
			tvwDirTree.SelectedNode = tvwDirTree.Nodes[0];
		}

		private void tvwDirTree_AfterSelect(object sender, TreeViewEventArgs e)
		{
			var rootParentDirPath = new DirectoryInfo(txtFileLocation.Text.Trim()).Parent.FullName;
			var offset = rootParentDirPath.Length + 1; //We include the // suffix of parent folder hierarchy for calculating string omission offset
			TreeNode nazaraNode = null;

			if (e.Node.Tag.ToString().ToLower() == NodeType.Folder.ToString().ToLower())
			{
				nazaraNode = e.Node;
			}
			else if (e.Node.Tag.ToString().ToLower() == NodeType.File.ToString().ToLower())
			{
				//We show folder details for parent folder of selected file in tree view
				nazaraNode = e.Node.Parent;
			}
			var relativePath = nazaraNode.Name.Substring(offset);
			currentHierarchyParentPath = relativePath;

			currentFiltererdNodes.Clear();
			foreach (CTreeNode child in nazaraNode.Nodes)
			{
				if (!child.Hidden)
					currentFiltererdNodes.Add(child);
			}

			PopulateFirstLevelChildrenInThumViewer();
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
		private CTreeNode FindTreeNode(CTreeNode node, string value2Find)
		{
			foreach (CTreeNode child in node.Nodes)
			{
				if (child.Text == value2Find)
				{
					return child;
				}

				if (child.Nodes.Count > 0)
				{
					CTreeNode found = FindTreeNode(child, value2Find);
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
