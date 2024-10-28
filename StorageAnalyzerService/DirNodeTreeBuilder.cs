using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace StorageAnalyzerService
{
    public class DirNodeTreeBuilder
    {
        public string InputFolderPath { get; set; }
        public int FolderImageIndex = -1;
        public int FileImageIndex = -1;

        public TreeNode BuildNodesForTreeView()
        {
            if (Directory.Exists(InputFolderPath))
            {
                DirectoryInfo targetDir = new DirectoryInfo(InputFolderPath);
                return AddDirectoryToTree(targetDir, null);
            }

            return null;
        }

        public static TreeNode AddDirectoryToTree(DirectoryInfo targetDir, TreeNode parentTreeNode)
        {
            var childTreeNode = new TreeNode();
            childTreeNode.Name = targetDir.Name;
            childTreeNode.Text = targetDir.Name;
            childTreeNode.ToolTipText = targetDir.FullName;

            if (parentTreeNode != null) //This is not a root directory
            {
                parentTreeNode.Nodes.Add(childTreeNode);
            }

            // Process the list of files found in the directory.
            FileInfo[] fileEntries = targetDir.GetFiles();
            foreach (var fileInfo in fileEntries)
            {
                AddFileToTree(fileInfo, childTreeNode);
            }

            // Recurse into subdirectories of this directory.
            var subdirectoryEntries = targetDir.GetDirectories();
            foreach (var subdirectory in subdirectoryEntries)
            {
                AddDirectoryToTree(subdirectory, childTreeNode);
            }
            return childTreeNode;
        }

        // Insert logic for processing found files here.
        public static void AddFileToTree(FileInfo targetFile, TreeNode parentNode)
        {
            var childTreeNode = new TreeNode();
            childTreeNode.Name = targetFile.Name;
            childTreeNode.Text = targetFile.Name;
            childTreeNode.ToolTipText = targetFile.FullName;
            parentNode.Nodes.Add(childTreeNode);
        }
    }
}
