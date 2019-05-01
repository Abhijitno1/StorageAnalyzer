using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FilesHunter
{
    static class TreeViewHelper
    {
        private static TreeNode initialNodeTree;

        public static void PrepareForFiltering(this TreeView myTreeView)
        {
            if (myTreeView.Nodes.Count > 0)
                initialNodeTree = myTreeView.Nodes[0];
        }

        public static void FilterNodes(this TreeView myTreeView, List<string> nodeNames)
        {
            TreeNode output = null;

            foreach (var anode in nodeNames)
            {
                var foundNode = initialNodeTree.Nodes.Find(anode, true)[0];
                output = BuildTree(foundNode, output);
            }
            myTreeView.Nodes.Clear();
            if (output != null)
                myTreeView.Nodes.Add(output);
        }

        public static TreeNode BuildTree(TreeNode foundNode, TreeNode output)
        {
            List<TreeNode> nodeFamily = new List<TreeNode>();
            var currentNode = foundNode;
            do
            {
                nodeFamily.Add(currentNode);
                currentNode = currentNode.Parent;
            } while (currentNode != null);

            TreeNode prevNode = null;
            for (var i = nodeFamily.Count - 1; i >= 0; i--)
            {
                currentNode = nodeFamily[i];
                //If output already contains given node then select it as new node to be added to output
                TreeNode foundOutput = null;
                if (output != null)
                {
                    if (output.Name.Equals(currentNode.Name))
                        foundOutput = output;
                    else
                    {
                        var tmp = output.Nodes.Find(currentNode.Name, true);
                        if (tmp.Any())
                            foundOutput = tmp[0];
                    }
                }
                TreeNode newNode;
                if (foundOutput != null)
                    newNode = foundOutput;
                else
                {
                    //Create new node for output
                    newNode = new TreeNode
                    {
                        Name = currentNode.Name,
                        Text = currentNode.Text,
                        ToolTipText = currentNode.ToolTipText,
                        ImageIndex = currentNode.ImageIndex
                    };
                    if (output == null)
                    {
                        output = newNode;
                    }
                    //else if (i == nodeFamily.Count - 1)
                    //{
                    //    output.Nodes.Add(newNode);
                    //}
                    else
                    {
                        prevNode.Nodes.Add(newNode);
                    }
                }
                prevNode = newNode;
            }
            return output;
        }

        public static void ClearFilters(this TreeView myTreeView)
        {
            myTreeView.Nodes.Clear();
            myTreeView.Nodes.Add(initialNodeTree);
        }
    }
}
