using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FilesHunter.UserControls
{
	public partial class CTreeView : TreeView
	{
		public event CascadeNodeEventHandler CascadeNode;

		public delegate void CascadeNodeEventHandler(object sender, EventArgs e);

		internal CascadeNodeEventArgs CascadeNodeEventRaiser(CTreeNode NDSource, CTreeNode NDCurrent)
		{
			var EA = new CascadeNodeEventArgs(NDSource, NDCurrent);
			CascadeNode?.Invoke(this, EA);
			return EA;
		}

		public class CascadeNodeEventArgs : EventArgs
		{
			public CascadeNodeEventArgs(CTreeNode NDSource, CTreeNode NDCurrent)
			{
				_CascadeNode = NDCurrent;
				_TrigerNode = NDSource;
			}

			// If this set to True, the whole cascading operation will be canceled.
			private bool _CancelCascade;
			public bool CancelCascade
			{
				get
				{
					return _CancelCascade;
				}
				set
				{
					_CancelCascade = value;
				}
			}

			private CTreeNode _CascadeNode;
			public CTreeNode CascadeNode
			{
				get
				{
					return _CascadeNode;
				}
			}

			private CTreeNode _TrigerNode;
			public CTreeNode TrigerNode
			{
				get
				{
					return _TrigerNode;
				}
			}

			private bool _Handled;
			public bool Handled
			{
				get
				{
					return _Handled;
				}
				set
				{
					_Handled = value;
				}
			}
		}

		public CTreeView()
		{
			Nodes = new CTreeNodeCollection(base.Nodes);
		}


		// Redefining native Node collection
		public new readonly CTreeNodeCollection Nodes;


		// delegate that will decide what to keep visible
		public delegate bool Selector(CTreeNode Node);


		public new CTreeNode SelectedNode
		{
			get
			{
				return (CTreeNode)base.SelectedNode;
			}
			set
			{
				base.SelectedNode = value;
			}
		}

		public TreeNodeCollection CurrentlyFilteredNodes
		{
			get
			{
				return this.Nodes._VisibleNodes;
			}
		}


		public void Filter(Selector Filter)
		{
			_Filter(Filter, this.Nodes);
		}


		// The recursive Filtering procedure
		private void _Filter(Selector Filter, CTreeNodeCollection NDC)
		{
			foreach (var ND in NDC._ActualNodes)
			{
				// Before anything else, if we have subNodes, go and filter them first
				if (ND.Nodes._ActualNodes.Count > 0)
					_Filter(Filter, ND.Nodes);

				if (Filter(ND))
					ND.Hidden = false;
				else
					// if current node has at least one visible node after filtering, make the parent visible too
					ND.Hidden = ND.Nodes._VisibleNodes.Count == 0;
			}
		}
	}
}
