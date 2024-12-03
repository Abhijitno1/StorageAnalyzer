using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace FilesHunter.UserControls
{
	public class CTreeNode : TreeNode
	{
		internal CTreeNodeCollection _MyTreeNodeCollection;

		public new CTreeNodeCollection Nodes;

		public CTreeNode() : base()
		{
			Nodes = new CTreeNodeCollection(base.Nodes);
		}

		public CTreeNode(string Text) : base(Text)
		{
		}

		public CTreeNode(string Text, CTreeNode[] Children) : base(Text)
		{
			foreach (var N in Children)
				this.Nodes.Add(N);
		}
		public new CTreeView TreeView
		{
			get
			{
				return (CTreeView)base.TreeView;
			}
		}

		private bool _Hidden;
		public bool Hidden
		{
			get
			{
				return _Hidden;
			}
			set
			{
				//if (CascadeUp)
				//{
				//	CTreeNode P = (CTreeNode)base.Parent;


				//	while (P != null)
				//	{
				//		var Res = TreeView.CascadeNodeEventRaiser(this, P);
				//		if (Res.CancelCascade)
				//			break;
				//		if (Res.Handled == false)
				//		{
				//			// we set CascadeUp to false and cycle through all parents manually to be able to pass in CascadeNodeEventRaiser the real originating node of those callings
				//			P.Hidden = value;
				//			P = P.Parent;
				//		}
				//	}
				//}

				//if (CascadeDown)
				//	CascadeCollection(this.Nodes, value);

				// Do nothing if value didn't really changed, to increase performance.
				if (_Hidden != value)
				{
					_Hidden = value;

					if (this.InCollection)
					{
						if (_Hidden == true)
							_MyTreeNodeCollection._VisibleNodes.Remove(this);
						else
							// if we making the node visible, put it after closer visible node
							if (this.PreviousUnHidenNode == null)
							_MyTreeNodeCollection._VisibleNodes.Insert(0, this);
						else
							_MyTreeNodeCollection._VisibleNodes.Insert(this.PreviousUnHidenNode.VisibilityIndex + 1, this);
					}
				}
			}
		}
		// This is a recruceve procedure that will cascade down all nodes
		private void CascadeCollection(CTreeNodeCollection NDC, bool IsHidden)
		{
			foreach (CTreeNode N in NDC)
			{
				var Res = TreeView.CascadeNodeEventRaiser(this, N);
				if (Res.CancelCascade)
					break;
				if (Res.Handled == false)
				{
					N.Hidden = IsHidden;
					if (N.Nodes.Count > 0)
						CascadeCollection(N.Nodes, IsHidden);
				}
			}
		}


		// This will return the closest previous unHidden node
		internal CTreeNode PreviousUnHidenNode
		{
			get
			{
				if (this.Index == 0)
					return null/* TODO Change to default(_) if this is not a reference type */;
				else if (this.InCollection == false)
					throw new Exception("This node is not assigned to a TreeNodeCollection yet.");
				else
				{
					for (var i = this.Index - 1; i >= 0; i += -1)
					{
						if (_MyTreeNodeCollection[i].Hidden == false)
							return _MyTreeNodeCollection[i];
					}
					return null;
				}
			}
		}

		// This will return node index as it visible on TreeView
		public int VisibilityIndex
		{
			get
			{
				return base.Index;
			}
		}


		// This will return node index as it is in the Actual node list
		public new int Index
		{
			get
			{
				if (this.InCollection)
					return _MyTreeNodeCollection._ActualNodes.IndexOf(this);
				else
					throw new Exception("This node is not assigned to a TreeNodeCollection yet.");
			}
		}


		// just a help property to check if tree node assigned to some tree collection
		internal bool InCollection
		{
			get
			{
				return _MyTreeNodeCollection != null;
			}
		}

	}
}