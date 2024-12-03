using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FilesHunter.UserControls
{
	public class CTreeNodeCollection : IEnumerable
	{
		internal TreeNodeCollection _VisibleNodes;
		internal List<CTreeNode> _ActualNodes = new List<CTreeNode>();

		public CTreeNodeCollection(TreeNodeCollection TreeNodeCollection)
		{
			_VisibleNodes = TreeNodeCollection;
		}

		// This is to mimic original TreeNodeCollection behavior
		public CTreeNode this[int Index]
		{
			get
			{
				return _ActualNodes[Index];
			}
		}
		public void Add(CTreeNode Node)
		{
			Node._MyTreeNodeCollection = this;
			_ActualNodes.Add(Node);
			// if the node is Hidden, there is no need to add it to TreeNodeCollection
			if (Node.Hidden == false)
				_VisibleNodes.Add(Node);
		}

		public void Clear()
		{
			//Clear both nodes collections
			_ActualNodes.Clear();
			_VisibleNodes.Clear(); 
		}


		public void AddRange(CTreeNode[] Nodes)
		{
			foreach (var N in Nodes)
				Add(N);
		}

		public void Remove(CTreeNode Node)
		{
			Node._MyTreeNodeCollection = null;
			_ActualNodes.Remove(Node);
			_VisibleNodes.Remove(Node);
		}

		public int Count
		{
			get
			{
				return _ActualNodes.Count;
			}
		}
		public int VisibleCount
		{
			get
			{
				return _VisibleNodes.Count;
			}
		}

		public void Insert(CTreeNode Node, int Index)
		{
			_ActualNodes.Insert(Index, Node);
			if (Node.Hidden == false)
			{
				// If there is no unHidden nodes at same level, add it just at the beginning
				if (Node.PreviousUnHidenNode == null)
					_VisibleNodes.Insert(0, Node);
				else
					_VisibleNodes.Insert(Node.PreviousUnHidenNode.VisibilityIndex, Node);
			}
		}

		#region "Implementing IEnumerable"
		public IEnumerator GetEnumerator()
		{
			return new CMyEnumerator(_ActualNodes);
		}

		private class CMyEnumerator : IEnumerator
		{
			public List<CTreeNode> Nodes;
			private int position = -1;

			// constructor
			public CMyEnumerator(List<CTreeNode> ActualNodes)
			{
				Nodes = ActualNodes;
			}

			private IEnumerator getEnumerator()
			{
				return (IEnumerator)this;
			}

			// IEnumerator
			public bool MoveNext()
			{
				position += 1;
				return (position < Nodes.Count);
			}

			// IEnumerator
			public void Reset()
			{
				position = -1;
			}

			// IEnumerator
			public object Current
			{
				get
				{
					try
					{
						return Nodes[position];
					}
					catch (IndexOutOfRangeException e1)
					{
						throw new InvalidOperationException();
					}
				}
			}
		}

		#endregion "Implementing IEnumerable"

	}
}
