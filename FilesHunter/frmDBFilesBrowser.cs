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
using System.Runtime.Remoting.Channels;
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
		string currentHierarchyParentPath, cutNodePath, copyNodePath;
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

					//Strip off root folder name from relative folder path before saving Modak in DB
					relativeFolderPath = relativeFolderPath.Substring(relativeFolderPath.IndexOf('\\'));
					//Change the file name portion in the relative path
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
					var fileData = dbReader.GetModakData(resourceDbId);
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

		private void ThumbViewer_DeleteResource(string itemName, string itemPath, string nodeType)
		{
			var relativeFolderPath = itemPath.TrimStart('\\') + @"\" + itemName;
			var filterClause = string.Empty;
			if (nodeType == NodeType.File.ToString())
			{
				filterClause = GenerateXPathFilterClauseFromRelativeFolderPath(relativeFolderPath, NodeType.File);
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
			else if (nodeType == NodeType.Folder.ToString())
			{
				filterClause = GenerateXPathFilterClauseFromRelativeFolderPath(relativeFolderPath, NodeType.Folder);
				var selectedNode = currentFolderNaksha.SelectSingleNode(filterClause);
				if (selectedNode != null)
				{
					var parentNode = selectedNode.ParentNode;
					if (parentNode != null)
					{
						DirectoryMapDbSaver saver = new DirectoryMapDbSaver();
						
						//Delete child files stored under this folder before deleting it. 
						saver.DeleteFiles4Node(selectedNode);

						parentNode.RemoveChild(selectedNode);
						saver.UpdateMap(currentFolderNaksha);

						//Refresh the treeview and listview
						TreeViewRefreshState();
					}
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
				fileData = reader.GetModakData(fileId);
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
							imageData = reader.GetModakData(fileId);
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

		private void tvwDirTree_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
		{
			if (e.Button == MouseButtons.Right)
			{
				tvwDirTree.SelectedNode = (CTreeNode)e.Node;
				//Ref: https://stackoverflow.com/questions/32082280/right-click-on-node-in-treeview-and-have-a-menu-pop-up-with-the-option-of-open
				tvwContextMenu.Show(Cursor.Position);
			}
		}

		private void tvwContextMenu_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
		{
			var srcNodeType = NodeType.File.ToString();

			if (e.ClickedItem.Text == tvwMenuCut.Text) 
			{
				copyNodePath = null;
				cutNodePath = GetRelativePathForSelectedTreeNode(tvwDirTree.SelectedNode.Name);
				srcNodeType = tvwDirTree.SelectedNode.Tag.ToString();
			}
			else if (e.ClickedItem.Text == tvwMenuCopy.Text)
			{
				cutNodePath = null;
				copyNodePath = GetRelativePathForSelectedTreeNode(tvwDirTree.SelectedNode.Name);
				srcNodeType = tvwDirTree.SelectedNode.Tag.ToString();
			}
			else if (e.ClickedItem.Text == tvwMenuPaste.Text)
			{
				//Moving to parent if a File node is selected as destination of paste action
				string destNodePath = GetRelativePathForSelectedTreeNode(tvwDirTree.SelectedNode.Name);
				var nodeEndHere = destNodePath.LastIndexOf('\\') > -1 ? destNodePath.Substring(destNodePath.LastIndexOf('\\')) : destNodePath;
				var destNodeType = nodeEndHere.IndexOf('.') == -1 ? NodeType.Folder : NodeType.File;
				if (destNodeType == NodeType.File)
				{
					destNodePath = destNodePath.Substring(0, destNodePath.LastIndexOf('\\'));
					//Change dest node type
					destNodeType = NodeType.Folder;
				}
				string destNodeName = destNodePath.Substring(destNodePath.LastIndexOf('\\') + 1);
				string srcNodeName = null;

				if (cutNodePath != null)
				{
					//This is Cut and Paste operation
					srcNodeName = cutNodePath.Substring(cutNodePath.LastIndexOf('\\') + 1);
					var result = WinFormsConfirm.ShowDialog($"Are you sure you want to paste Cut Node \"{srcNodeName}\" at \"{destNodeName}\"?", "File/Folder Move");
					int resourceDbId = 0;
					if (result == DialogResult.Yes)
					{
						DirectoryMapDbSaver saver = new DirectoryMapDbSaver();
						DirectoryMapDbReader reader = new DirectoryMapDbReader();

						if (srcNodeType == NodeType.File.ToString())
						{
							//Step 1: Remove xml node from its original position
							var filterClause = GenerateXPathFilterClauseFromRelativeFolderPath(cutNodePath, NodeType.File);
							var cutXmlNode = currentFolderNaksha.SelectSingleNode(filterClause);
							if (cutXmlNode != null)
							{
								resourceDbId = Convert.ToInt32(cutXmlNode.Attributes["DbId"].Value);
								var parentNode = cutXmlNode.ParentNode;
								if (parentNode != null)
								{
									parentNode.RemoveChild(cutXmlNode);
								}
							}

							//Step 2: Append xml node at its new position
							filterClause = GenerateXPathFilterClauseFromRelativeFolderPath(destNodePath, NodeType.Folder);
							var destXmlNode = currentFolderNaksha.SelectSingleNode(filterClause);
							if (destXmlNode != null )
							{
								if (destXmlNode.Name == "folder")
								{
									destXmlNode.AppendChild(cutXmlNode);
								}
								else 
								{
									destXmlNode = destXmlNode.ParentNode; //Get to the parent folder of selected destination file
									destXmlNode.AppendChild(cutXmlNode); //Transfer the moved child to the new parent
								}
							}
							saver.UpdateMap(currentFolderNaksha);

							//Step 3: Update the relative path in Modak DB object
							Modak updModak = reader.GetModak(resourceDbId);
							updModak.RelativePath = destNodePath.Substring(destNodePath.IndexOf('\\', 1)) + "\\" + srcNodeName;
							saver.UpdateModak(updModak);
						}
						else
						{
							MessageBox.Show("Folder Cut and Paste is not yet supported");
						}

						//Refresh the treeview and listview
						TreeViewRefreshState();
					}
				}
				else if (copyNodePath != null)
				{
					//This is copy paste operation
					srcNodeName = copyNodePath.Substring(copyNodePath.LastIndexOf('\\') + 1);
					var result = WinFormsConfirm.ShowDialog($"Are you sure you want to paste Copied Node \"{srcNodeName} at \"{destNodeName}\"?", "File/Folder Move");
					if (result == DialogResult.Yes)
					{
						XmlElement newElm;
						DirectoryMapDbSaver saver = new DirectoryMapDbSaver();
						DirectoryMapDbReader reader = new DirectoryMapDbReader();

						if (srcNodeType == NodeType.File.ToString())
						{
							//Step 1: Get XML node corresponding to the treenode being copied
							var filterClause = GenerateXPathFilterClauseFromRelativeFolderPath(copyNodePath, NodeType.File);
							var selectedNode = currentFolderNaksha.SelectSingleNode(filterClause);

							//Step 2: Create a copy of the existing node for duplication
							newElm = currentFolderNaksha.CreateElement("file");
							newElm.SetAttribute("name", selectedNode.Attributes["name"].Value);
							newElm.SetAttribute("extension", selectedNode.Attributes["extension"].Value);
							newElm.SetAttribute("creationDate", selectedNode.Attributes["creationDate"].Value);
							newElm.SetAttribute("size", selectedNode.Attributes["size"].Value);

							//Step 3: Create a copy of existing modak (filedata) object for storing in DB
							var	resourceDbId = Convert.ToInt32(selectedNode.Attributes["DbId"].Value);
							var copyModak = reader.GetModak(resourceDbId);

							filterClause = GenerateXPathFilterClauseFromRelativeFolderPath(destNodePath, NodeType.Folder);
							//Strip off top folder in hierarchy for storing relative path in DB and append the new file name to relative dest folder path
							var modakRelPath = destNodePath.Substring(destNodePath.IndexOf('\\') + 1) + "\\" + srcNodeName;
							//var fileName = selectedNode.Name;
							var destParentNode = currentFolderNaksha.SelectSingleNode(filterClause);

							Modak modak = new Modak()
							{
								Title = copyModak.Title,
								PicData = copyModak.PicData,
								RelativePath = modakRelPath
							};
							saver.InsertModakIntoDb(modak);
							newElm.SetAttribute("DbId", modak.Id.ToString());
							destParentNode.AppendChild(newElm);
							saver.UpdateMap(currentFolderNaksha);
						}
						else
						{
							MessageBox.Show("Folder Copy and Paste is not yet supported");
						}

						//Refresh the treeview and listview
						TreeViewRefreshState();
					}
				}
			}
		}

		private void tvwDirTree_AfterSelect(object sender, TreeViewEventArgs e)
		{
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
			currentHierarchyParentPath = GetRelativePathForSelectedTreeNode(nazaraNode.Name);

			currentFiltererdNodes.Clear();
			foreach (CTreeNode child in nazaraNode.Nodes)
			{
				if (!child.Hidden)
					currentFiltererdNodes.Add(child);
			}

			PopulateFirstLevelChildrenInThumViewer();
		}

		private string GetRelativePathForSelectedTreeNode(string nodeName)
		{
			var rootParentDirPath = new DirectoryInfo(txtFileLocation.Text.Trim()).Parent.FullName;
			var offset = rootParentDirPath.Length + 1; //We include the // suffix of parent folder hierarchy for calculating string omission offset
			var relativePath = nodeName.Substring(offset);
			return relativePath;
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
