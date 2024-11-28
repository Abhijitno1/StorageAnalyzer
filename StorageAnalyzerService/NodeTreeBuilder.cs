using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace StorageAnalyzerService
{
    public class NodeTreeBuilder
    {
        public string InputFilePathName { get; set; }
        public int FolderImageIndex = -1;
        public int FileImageIndex = -1;

        public TreeNode BuildNodesForTreeView()
        {
            var document = new XmlDocument();
            document.Load(InputFilePathName);

            var rootNode = new TreeNode();
            AddNodesToTree(rootNode, document.DocumentElement);

            return rootNode;
        }

		public TreeNode BuildNodesForTreeView(XmlDocument document)
		{
			var rootNode = new TreeNode();
			AddNodesToTree(rootNode, document.DocumentElement);
			return rootNode;
		}

		private void AddNodesToTree(TreeNode currentNode, XmlNode xmlNode)
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
                var childTreeNode = new TreeNode();
                currentNode.Nodes.Add(childTreeNode);
                AddNodesToTree(childTreeNode, childXmlNode);
            }
        }        
   }
}
