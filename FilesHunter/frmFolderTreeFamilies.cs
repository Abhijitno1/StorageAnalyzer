using StorageAnalyzerService;
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
	public partial class frmFolderTreeFamilies : Form
	{
		public string SelectedFolderTree {  get; private set; }
		public frmFolderTreeFamilies()
		{
			InitializeComponent();
		}

		private void frmFolderTreeFamilies_Load(object sender, EventArgs e)
		{
			fbdFolderLocation.RootFolder = Environment.SpecialFolder.MyComputer;

			DirectoryMapDbReader reader = new DirectoryMapDbReader();
			var mapNames = reader.GetAllFolderMapsList();
			foreach (var mapName in mapNames)
			{
				lstFolderHrchies.Items.Add(mapName);
			}
		}

		private void btnOpenSelected_Click(object sender, EventArgs e)
		{
			if (lstFolderHrchies.Items.Count == 0 || lstFolderHrchies.SelectedItems.Count == 0)
				return;
			this.SelectedFolderTree = lstFolderHrchies.SelectedItem.ToString();
			this.DialogResult = DialogResult.OK;
			this.Close();
		}

		private void btnOpenDialog_Click(object sender, EventArgs e)
		{
			DialogResult result = fbdFolderLocation.ShowDialog();
			if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbdFolderLocation.SelectedPath))
			{
				txtFileLocation.Text = fbdFolderLocation.SelectedPath;
			}
		}

		private void btnSaveFolderData_Click(object sender, EventArgs e)
		{
			DirectoryMapDbSaver saver = new DirectoryMapDbSaver();
			saver.RootFolderPath = txtFileLocation.Text.Trim();
			saver.SaveMap();
			//Also add item to the list
			lstFolderHrchies.Items.Add(saver.RootFolderPath);
		}

		private void btnDeleteSelected_Click(object sender, EventArgs e)
		{
			if (lstFolderHrchies.Items.Count == 0 || lstFolderHrchies.SelectedItems.Count == 0)
				return;
			DirectoryMapDbSaver saver = new DirectoryMapDbSaver();
			saver.DeleteFolderMap(lstFolderHrchies.SelectedItem.ToString());
			//Also remove item from the list
			lstFolderHrchies.Items.RemoveAt(lstFolderHrchies.SelectedIndex);
		}

		private void lstFolderHrchies_MouseDoubleClick(object sender, MouseEventArgs e)
		{
			if (lstFolderHrchies.SelectedItem == null) return;
			this.SelectedFolderTree = lstFolderHrchies.SelectedItem.ToString();
			this.DialogResult = DialogResult.OK;
			this.Close();
		}
	}
}
