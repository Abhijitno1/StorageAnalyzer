using FilesHunter.UserControls;
using StorageAnalyzerService;
using StorageAnalyzerService.DbModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using SD = System.Drawing;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.Remoting.Channels;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;

namespace FilesHunter
{
	public partial class frmDBFilesBrowser : Form
	{
		private List<string> ImageExtensions = new List<string> { ".JPG", ".JPE", ".BMP", ".GIF", ".PNG" };
		int panel1OrigWidth, panel2OrigWidth, formOrigHeight;
		XmlDocument currentFolderNaksha;
		string currentHierarchyParentPath, cutNodePath, copyNodePath;
		List<CTreeNode> currentFiltererdNodes= new List<CTreeNode>();
		string srcNodeType = NodeType.File.ToString().ToLower();

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

		private void ThumbViewer_RenameResource(string itemName, string itemPath, string nodeType)
		{
            var nodeTypeEnum = (NodeType)Enum.Parse(typeof(NodeType), nodeType);
            var relativeFolderPath = itemPath.TrimEnd('\\') + @"\" + itemName; //Making sure to remove trailing / in Drive letter assignment
			var filterClause = GenerateXPathFilterClauseFromRelativeFolderPath(relativeFolderPath, nodeTypeEnum);
			var selectedNode = currentFolderNaksha.SelectSingleNode(filterClause);
			if (selectedNode != null)
			{
				var newName = WinFormsPrompt.ShowDialog("Please enter new name for selected resource", "Rename Resource in DB", itemName);
				if (newName != null)
				{
					DirectoryMapDbSaver dbSaver = new DirectoryMapDbSaver();

					//change resource name in xml node
					selectedNode.Attributes["name"].Value = newName;
					dbSaver.UpdateMap(currentFolderNaksha);

					if (nodeTypeEnum == NodeType.File)
					{
                        //Strip off root folder name from relative folder path before saving Modak in DB
                        relativeFolderPath = relativeFolderPath.Substring(relativeFolderPath.IndexOf('\\'));
                        //Change the file name portion in the relative path
                        relativeFolderPath = relativeFolderPath.Substring(0, relativeFolderPath.LastIndexOf('\\') + 1) + newName;
                        Modak modak = new Modak()
                        {
                            Id = Convert.ToInt32(selectedNode.Attributes["DbId"].Value),
                            Title = newName,
                            RelativePath = relativeFolderPath
                        };
                        dbSaver.UpdateModak(modak);
                    }

                    //Refresh the treeview and listview
                    TreeViewRefreshState();

                    var foundTreeNode = FindTreeNode(tvwDirTree.Nodes[0], newName);
                    tvwDirTree.SelectedNode = foundTreeNode;
                    tvwDirTree.Focus();
                }
			}
		}
		private void ThumbViewer_SaveResource(string itemName, string itemPath, string nodeType)
		{
			var relativeFolderPath = itemPath.TrimEnd('\\');
			var rootFolderPath = currentFolderNaksha.DocumentElement.Attributes["fullPath"].Value;
            var nodeTypeEnum = (NodeType)Enum.Parse(typeof(NodeType), nodeType);
			string relativeFilePath = "", saveAbsolutePath = "", filterClause = "", fileName = "";
			if (nodeTypeEnum == NodeType.File)
			{
                relativeFilePath = relativeFolderPath + @"\" + itemName;
                filterClause = GenerateXPathFilterClauseFromRelativeFolderPath(relativeFilePath, nodeTypeEnum);
				relativeFolderPath += '\\';
                //Strip off root folder name from relativeFolderPath variable while creating absolute folder path for default save location
                relativeFolderPath = relativeFolderPath.Substring(relativeFolderPath.IndexOf('\\') + 1);
				fileName = itemName;
                //saveAbsolutePath = rootFolderPath + relativeFilePath.Substring(relativeFilePath.LastIndexOf('\\'));
            }
            else if (nodeTypeEnum == NodeType.Folder)
			{
                relativeFolderPath = relativeFolderPath + @"\" + itemName;
                filterClause = GenerateXPathFilterClauseFromRelativeFolderPath(relativeFolderPath, nodeTypeEnum);
                //Strip off root folder name from relativeFolderPath variable while creating absolute folder path for default save location
                relativeFolderPath = relativeFolderPath.Substring(relativeFolderPath.IndexOf('\\') + 1);
            }

            var selectedNode = currentFolderNaksha.SelectSingleNode(filterClause);
			if (selectedNode != null)
			{
                Action<XmlNode> saveSingleFileToDisk = (XmlNode curNode) =>
                {
                    var resourceDbId = Convert.ToInt32(curNode.Attributes["DbId"].Value);
                    DirectoryMapDbReader dbReader = new DirectoryMapDbReader();
                    var fileData = dbReader.GetModakData(resourceDbId);
                    //Debug.WriteLine("Writing file: " + saveAbsolutePath + " to disk");
                    File.WriteAllBytes(saveAbsolutePath, fileData);
					txtNewItemLocation.Text = string.Empty;
                };
                Action<XmlNode> saveFileToDisk = (XmlNode curNode) =>
                {
                    var resourceDbId = Convert.ToInt32(curNode.Attributes["DbId"].Value);
                    DirectoryMapDbReader dbReader = new DirectoryMapDbReader();
                    var fileData = dbReader.GetModakData(resourceDbId);
                    var childFilePathName = curNode.Attributes["name"].Value;
					var iNode = curNode;
					while (iNode.ParentNode != null && iNode.ParentNode.Name != "#document")
					{
						childFilePathName = iNode.ParentNode.Attributes["name"].Value + @"\" + childFilePathName;
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
                        childFolderName = iNode.ParentNode.Attributes["name"].Value + @"\" + childFolderName;
                        iNode = iNode.ParentNode;
                    }
                    childFolderName = childFolderName.Substring(childFolderName.IndexOf(@"\") + 1);
                    childFolderName = Path.Combine(rootFolderPath, childFolderName);
                    //Debug.WriteLine("creating Folder: " + childFolderName);
                    Directory.CreateDirectory(childFolderName);
                };

				if (nodeTypeEnum == NodeType.File)
				{
					var sfdFileSaver = new SaveFileDialog();
					sfdFileSaver.InitialDirectory = Path.Combine(rootFolderPath, relativeFolderPath);
                    sfdFileSaver.FileName = fileName;
                    //Allow user to change default save path
                    var dlgResult = sfdFileSaver.ShowDialog();

					if (dlgResult == DialogResult.OK)
					{
						saveAbsolutePath = sfdFileSaver.FileName;
						saveSingleFileToDisk(selectedNode);
					}
				}
				else if (nodeTypeEnum == NodeType.Folder)
				{
					fbdFolderLocation.SelectedPath = Path.Combine(rootFolderPath, relativeFolderPath); ;
					//Allow user to change default save path
                    var dialogResult = fbdFolderLocation.ShowDialog();
                    if (dialogResult == DialogResult.OK)
                    {
                        rootFolderPath = fbdFolderLocation.SelectedPath; //changing root folder path to user selection 
                        IterateThruXmlHierarchy(selectedNode, relativeFolderPath, saveFileToDisk, createFolderOnDisk);
                        txtNewItemLocation.Text = string.Empty;
                    }
                }
            }
		}
		private void IterateThruXmlHierarchy(XmlNode parentNode, string relativeFolderPath, Action<XmlNode> endNodeActionToCall, Action<XmlNode> interimNodeActionToCall)
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
                    IterateThruXmlHierarchy(childNode, relativeFolderPath, endNodeActionToCall, interimNodeActionToCall);
                }
            }
        }

        private void SplitButton1_MenuItemClick(object sender, EventArgs e)
		{
			//MessageBox.Show("You clicked " + (sender as ToolStripMenuItem).Tag.ToString(), "Zoomri Tallaiyah");
			var selectedTreeNode = tvwDirTree.SelectedNode;
			if (selectedTreeNode.Tag.ToString() == "file")
			{
				selectedTreeNode = selectedTreeNode.Parent as CTreeNode;
            }
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
			TreeViewRefreshState(selectedTreeNode);
		}

		private void ThumbViewer_DeleteResource(string itemName, string itemPath, string nodeType)
		{
            var selectedTreeNode = tvwDirTree.SelectedNode;
            if (selectedTreeNode.Tag.ToString() == "file")
            {
                selectedTreeNode = selectedTreeNode.Parent as CTreeNode;
            }
            var relativeFolderPath = itemPath.TrimEnd('\\') + @"\" + itemName;
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
						TreeViewRefreshState(selectedTreeNode);
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
						TreeViewRefreshState(selectedTreeNode);
					}
				}
			}
        }

		private void ThumbViewer_GetPreviewData(string itemName, string itemPath, frmMediaPreview.MediaType itemType, out object fileData)
		{
			var relativeFolderPath = itemPath.TrimEnd('\\') + @"\" + itemName;
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
			currentHierarchyParentPath = itemPath.TrimEnd('\\') + @"\" + itemName;
			PopulateFirstLevelChildrenInThumViewer();
			tvwDirTree.Focus();
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

		private void TreeViewRefreshState(CTreeNode selectedTreeNode)
		{
			TreeViewRefreshState();
            tvwDirTree.SelectedNode = selectedTreeNode;
            tvwDirTree.SelectedNode.Expand();
            tvwDirTree.Focus();
        }

        private string GenerateXPathFilterClauseFromRelativeFolderPath(string relativeFolderPath, NodeType nodeType)
		{
            var folderSegments = new String[1];
            //Regex.IsMatch(relativeFolderPath, "^[A-Za-z]:\\$") //This did not work
            if (!string.IsNullOrEmpty(relativeFolderPath) && relativeFolderPath.Length==3 && relativeFolderPath.EndsWith(@":\"))
			{
				folderSegments[0] = relativeFolderPath;
			}
			else 
			{
                folderSegments = relativeFolderPath.Split('\\');
            }
            var filterClause = "//";
			int j = 0;
			for (j = 0; j < folderSegments.Length - 1; j++)
			{
				var segment = folderSegments[j];
                if (Regex.IsMatch(segment, "^[A-Za-z]:$"))
                    segment += '\\';
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
						SD.Image folderImage = imlShowPad.Images[0];
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
						else if ((new string[] { ".jpg", ".jpeg", ".png", ".gif", ".tiff", ".bmp" }).Contains(fileExtn))
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
			if (this.formOrigHeight == 0) return;	//Don't do anything on initial form load

			var heightDiff = this.Height - this.formOrigHeight;

			tvwDirTree.Height += heightDiff;
            grpFolderDetails.Height += heightDiff;
			thumbViewer.Height += heightDiff - 2;

            //Reset the new height of form
            this.formOrigHeight = this.Height;
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
			var curSelection = tvwDirTree.SelectedNode;
            txtSearchName.Text = string.Empty;
			tvwDirTree.CollapseAll();
			tvwDirTree.Filter((node) => true);
			//Select first node of tree so that Listview gets refreshed
			tvwDirTree.SelectedNode = curSelection;
            tvwDirTree.SelectedNode.Expand();
        }

		private void tvwDirTree_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
		{
			if (e.Button == MouseButtons.Right)
			{
				var selTreeNode= (CTreeNode)e.Node;
                tvwDirTree.SelectedNode = selTreeNode;
				/*if (selTreeNode.Parent != null)
					tvwMenuUploadFldr.Visible = false;
				else
					tvwMenuUploadFldr.Visible = true;
				*/
				//Ref: https://stackoverflow.com/questions/32082280/right-click-on-node-in-treeview-and-have-a-menu-pop-up-with-the-option-of-open
				tvwContextMenu.Show(Cursor.Position);
			}
		}

        private void tvwContextMenu_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
		{

			if (e.ClickedItem.Text == tvwMenuCut.Text) 
			{
				copyNodePath = null;
				//cutNodePath = GetRelativePathForSelectedTreeNode(tvwDirTree.SelectedNode.Name);
                cutNodePath = tvwDirTree.SelectedNode.Name;
                srcNodeType = tvwDirTree.SelectedNode.Tag.ToString();
			}
			else if (e.ClickedItem.Text == tvwMenuCopy.Text)
			{
				cutNodePath = null;
                //copyNodePath = GetRelativePathForSelectedTreeNode(tvwDirTree.SelectedNode.Name);
                copyNodePath = tvwDirTree.SelectedNode.Name;
                srcNodeType = tvwDirTree.SelectedNode.Tag.ToString();
			}
			else if (e.ClickedItem.Text == tvwMenuPaste.Text)
			{
                var selectedTreeNode = tvwDirTree.SelectedNode;
                string destNodePath = selectedTreeNode.Name;
                if (selectedTreeNode.Tag.ToString() == NodeType.File.ToString().ToLower())
                {
                    //If file node is selected as destination of paste action then shift to it's parent folder as new destination
                    destNodePath = destNodePath.Substring(0, destNodePath.LastIndexOf('\\'));
                    selectedTreeNode = selectedTreeNode.Parent as CTreeNode;
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

						if (srcNodeType == NodeType.File.ToString().ToLower())
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
                                destXmlNode.AppendChild(cutXmlNode);    // We are already making sure destination node is set as a folder
                                saver.UpdateMap(currentFolderNaksha);
                            }

                            //Step 3: Update the relative path in Modak DB object
                            Modak updModak = reader.GetModak(resourceDbId);
							updModak.RelativePath = destNodePath.Substring(destNodePath.IndexOf('\\', 1)) + "\\" + srcNodeName;
							saver.UpdateModak(updModak);
						}
						else
						{
                            //Step 1: Remove xml node from its original position
                            var filterClause = GenerateXPathFilterClauseFromRelativeFolderPath(cutNodePath, NodeType.Folder);
                            var cutXmlNode = currentFolderNaksha.SelectSingleNode(filterClause);
                            if (cutXmlNode != null)
                            {
                                var parentNode = cutXmlNode.ParentNode;
                                if (parentNode != null)
                                {
                                    parentNode.RemoveChild(cutXmlNode);
                                }
                            }

                            //Step 2: Append xml node at its new position
                            filterClause = GenerateXPathFilterClauseFromRelativeFolderPath(destNodePath, NodeType.Folder);
                            var destXmlNode = currentFolderNaksha.SelectSingleNode(filterClause);
                            if (destXmlNode != null)
                            {
                                destXmlNode.AppendChild(cutXmlNode);    // We are already making sure destination node is set as a folder
                                saver.UpdateMap(currentFolderNaksha);
                            }

                            //Step 3: Recursively Update the relative path in Modak DB objects associated with all descendants of source cut node
                            Action<XmlNode> recursiveUpdatePathsOnModak = null;
                            recursiveUpdatePathsOnModak = (cutXmlNode1) =>
							{
                                foreach (XmlNode curNode in cutXmlNode1.ChildNodes)
                                {
                                    if (curNode.Name == "file")
                                    {
										string relPath = curNode.Attributes["name"].Value;
                                        var iNode = curNode;
                                        while (iNode.ParentNode != null && iNode.ParentNode.Name != "#document")
                                        {
                                            relPath = iNode.ParentNode.Attributes["name"].Value + @"\" + relPath;
                                            iNode = iNode.ParentNode;
                                        }
                                        relPath = relPath.Substring(relPath.IndexOf(@"\") + 1);
                                        var modakId = Convert.ToInt32(curNode.Attributes["DbId"].Value);
                                        var modak = reader.GetModak(modakId);
                                        modak.RelativePath = destXmlNode.Attributes["name"].Value + "\\" + relPath;
                                        saver.UpdateModak(modak);
                                    }
                                    else
                                    {
                                        recursiveUpdatePathsOnModak(curNode);
                                    }
                                }
                            };
                            recursiveUpdatePathsOnModak(cutXmlNode);

                        }

                        //Refresh the treeview and listview
                        TreeViewRefreshState(selectedTreeNode);
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

						if (srcNodeType == NodeType.File.ToString().ToLower())
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
							//MessageBox.Show("Folder Copy and Paste is not yet supported");
                            //Step 1: Get XML node corresponding to the treenode being copied
                            var filterClause = GenerateXPathFilterClauseFromRelativeFolderPath(copyNodePath, NodeType.Folder);
                            var selectedNode = currentFolderNaksha.SelectSingleNode(filterClause);

                            //Step 2: Create a copy of the existing node hierarchy for duplication
                            filterClause = GenerateXPathFilterClauseFromRelativeFolderPath(destNodePath, NodeType.Folder);
                            //Strip off top folder in hierarchy for storing relative path in DB and append the new file name to relative dest folder path
                            var modakRelPath = destNodePath.Substring(destNodePath.IndexOf('\\') + 1) + "\\" + srcNodeName;
                            //var fileName = selectedNode.Name;
                            var destParentNode = currentFolderNaksha.SelectSingleNode(filterClause);

                            Func<Modak, Modak> copyModak = (srcModak) =>
                            { 
                                return new Modak()
                                {
                                    Title = srcModak.Title,
                                    PicData = srcModak.PicData,
                                    RelativePath = srcModak.RelativePath
                                };
                            };

                            Action<XmlNode, XmlNode> insertModaks4ClinedNodes = null;
                            insertModaks4ClinedNodes = (copyXmlNode1, destXmlNode1) =>
                            {
                                foreach (XmlNode curNode in copyXmlNode1.ChildNodes)
                                {
                                    var correspondingDestNode = destXmlNode1.ChildNodes.OfType<XmlNode>().Where(x =>
                                        x.NodeType == XmlNodeType.Element && x.Attributes["name"].Value == curNode.Attributes["name"].Value)
                                        .FirstOrDefault();

                                    if (curNode.Name == "file")
                                    {
                                        string relPath = curNode.Attributes["name"].Value;
                                        var iNode = curNode;
                                        while (iNode.ParentNode != null && iNode.ParentNode.Name != "#document")
                                        {
                                            relPath = iNode.ParentNode.Attributes["name"].Value + @"\" + relPath;
                                            iNode = iNode.ParentNode;
                                        }
                                        relPath = relPath.Substring(relPath.IndexOf(@"\") + 1);
                                        var modakId = Convert.ToInt32(curNode.Attributes["DbId"].Value);
                                        var modak = reader.GetModak(modakId);				
										var copyOfModak = copyModak(modak);
                                        copyOfModak.RelativePath = destParentNode.Attributes["name"].Value + "\\" + relPath;
										//Debug.WriteLine($"Inserting Modak with  Title = {copyOfModak.Title}, Relative Path = {copyOfModak.RelativePath}");
										saver.InsertModakIntoDb(copyOfModak);
                                        (correspondingDestNode as XmlElement).SetAttribute("DbId", copyOfModak.Id.ToString());
                                    }
                                    else
                                    {
                                        insertModaks4ClinedNodes(curNode, correspondingDestNode);
                                    }
                                }
                            };
                            var copiedRoot = selectedNode.CloneNode(true);
                            //Debug.WriteLine($"Adding Xml node = {copiedRoot.Attributes["name"].Value}, Node Type = {copiedRoot.Name}, Parent Node = {destParentNode.Attributes["name"].Value}");
                            destParentNode.AppendChild(copiedRoot);
                            insertModaks4ClinedNodes(selectedNode, copiedRoot);
                            saver.UpdateMap(currentFolderNaksha);
                        }

                        //Refresh the treeview and listview
                        TreeViewRefreshState(selectedTreeNode);
                    }
                }
			}
			else if (e.ClickedItem.Text == tvwMenuUploadFldr.Text)
			{
                DirectoryMapDbSaver saver = new DirectoryMapDbSaver();
                var selectedTreeNode = tvwDirTree.SelectedNode;
                string destNodePath = selectedTreeNode.Name;
                if (selectedTreeNode.Tag.ToString() == NodeType.File.ToString().ToLower())
                {
                    //If file node is selected as destination of paste action then shift to it's parent folder as new destination
                    destNodePath = destNodePath.Substring(0, destNodePath.LastIndexOf('\\'));
                    selectedTreeNode = selectedTreeNode.Parent as CTreeNode;
                }

                //Step 2: Append xml node at its new position
                var filterClause = GenerateXPathFilterClauseFromRelativeFolderPath(destNodePath, NodeType.Folder);
                var destXmlNode = currentFolderNaksha.SelectSingleNode(filterClause);
                if (destXmlNode != null)
                {
					fbdFolderLocation.SelectedPath = destNodePath;
					var dlgResult =  fbdFolderLocation.ShowDialog();
					if (dlgResult == DialogResult.OK)
					{
                        saver.RootFolderPath = Directory.GetParent(fbdFolderLocation.SelectedPath).FullName;
                        destNodePath = fbdFolderLocation.SelectedPath;
						saver.GenerateChildNodeTree(destNodePath, destXmlNode);

                        //Refresh the treeview and listview
                        TreeViewRefreshState(selectedTreeNode);
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
			currentHierarchyParentPath = nazaraNode.Name;
			txtSelectedNodePath.Text = currentHierarchyParentPath;

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
