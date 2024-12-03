using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using FilesHunter.UserControls;

namespace StorageAnalyzerService
{
    public class CNodeTreeBuilder
    {
        public string InputFilePathName { get; set; }
        public int FolderImageIndex = -1;
        public int FileImageIndex = -1;

        public CTreeNode BuildNodesForTreeView()
        {
            var document = new XmlDocument();
            document.Load(InputFilePathName);

            var rootNode = new CTreeNode();
            AddNodesToTree(rootNode, document.DocumentElement);

            return rootNode;
        }

		public CTreeNode BuildNodesForTreeView(XmlDocument document)
		{
			var rootNode = new CTreeNode();
			AddNodesToTree(rootNode, document.DocumentElement);
			return rootNode;
		}

		private void AddNodesToTree(CTreeNode currentNode, XmlNode xmlNode)
        {
            currentNode.Name = xmlNode.ParentNode is XmlDocument ? xmlNode.Attributes["fullPath"].Value
                : currentNode.Parent.Name + "\\" + xmlNode.Attributes["name"].Value;
            currentNode.Text = xmlNode.Attributes["name"].Value;
            if (FileImageIndex != -1 && FolderImageIndex != -1)
            {
                currentNode.ImageIndex = xmlNode.Name == "file" ? FileImageIndex : FolderImageIndex;
            }
            currentNode.Tag = xmlNode.Name;
            currentNode.ToolTipText = currentNode.Name;

            for (int i = 0; i < xmlNode.ChildNodes.Count; i++)
            {
                var childXmlNode = xmlNode.ChildNodes[i];
                var childTreeNode = new CTreeNode();
                currentNode.Nodes.Add(childTreeNode);
                AddNodesToTree(childTreeNode, childXmlNode);
            }
        }

        enum SearchType
        {
            FilesAndFolders,
            FileOnly,
            FoldersOnly
        }
   }
}
