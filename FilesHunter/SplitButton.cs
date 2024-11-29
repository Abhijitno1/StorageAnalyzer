using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FilesHunter
{
	public partial class SplitButton : UserControl
	{
		public delegate void MenuItemClickDelegate(object sender, EventArgs e);
		public event MenuItemClickDelegate MenuItemClick;

		public SplitButton()
		{
			InitializeComponent();
		}

		private void btnMain_Click(object sender, EventArgs e)
		{
				//var clickPos = this.PointToClient(new System.Drawing.Point(MousePosition.X, MousePosition.Y));
				//	// If click is over the right-hand portion of the button show the menu
				//if (clickPos.X >= (Size.Width - Image.Width))
				ShowMenuUnderControl();			
		}

		/*
		 // If you want right-mouse click to invoke the menu override the mouse up event
			protected override void OnMouseUp(MouseEventArgs mevent)
			{
				if ((mevent.Button & MouseButtons.Right) != 0)
					ShowMenuUnderControl();
				else
					base.OnMouseUp(mevent);
			}
		 */

		public void ShowMenuUnderControl()
		{
			splitMenuStrip.Show(this, new Point(0, this.Height), ToolStripDropDownDirection.AboveRight);
		}

		private void insertToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (MenuItemClick!=null)
			{
				MenuItemClick(sender, e);
			}
		}

		private void addAtEndToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (MenuItemClick != null)
			{
				MenuItemClick(sender, e);
			}
		}

		private void insertAfterToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (MenuItemClick != null)
			{
				MenuItemClick(sender, e);
			}
		}
	}
}
