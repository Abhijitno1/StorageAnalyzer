using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FilesHunter
{
	public partial class frmMediaPreview : Form
	{
		public enum MediaType
		{
			Text,
			Image,
			Video,
			Other
		};

		public frmMediaPreview()
		{
			InitializeComponent();
			this.Load += FrmMediaPreview_Load;
			picView.AutoScrollOffset = new Point(picView.Width-50, 5);
		}

		private void FrmMediaPreview_Load(object sender, EventArgs e)
		{
		}

		public void ShowMedia(MediaType type, string fileName, object fileData)
		{
			this.Text = fileName;
			if (type == MediaType.Image)
			{
				axWMP.Visible = false;
				rtbSlate.Visible = false;
				picView.Visible = true;

				picView.Image = ThumbnailViewer.BinaryToImage((byte[])fileData);
			}
			else if (type == MediaType.Video)
			{
				axWMP.Visible = true;
				rtbSlate.Visible = false;
				picView.Visible = false;

				var tempFilePathName = Path.Combine(Path.GetTempPath(), fileName);
				//Path.GetTempFileName()																					
				//Change the extension of temp file so that media player is happy to play the file
				//var extn = Path.GetExtension(fileName);
				//var halfName = tempFilePathName.Substring(0, tempFilePathName.LastIndexOf('.'));
				//tempFilePathName = halfName + extn;
				File.WriteAllBytes(tempFilePathName, (byte[])fileData);

				axWMP.URL = tempFilePathName;
			}
			else if (type == MediaType.Text)
			{
				axWMP.Visible = false;
				rtbSlate.Visible = true;
				picView.Visible = false;

				rtbSlate.Text = fileData.ToString();
			}
			else
			{
				var tempFilePathName = Path.Combine(Path.GetTempPath(), fileName);
				File.WriteAllBytes(tempFilePathName, (byte[])fileData);

				ShellExecute("\"" + tempFilePathName + "\"");
				return;
			}
			this.ShowDialog();
		}

		private void ShellExecute(string commandText)
		{
			Process process = new Process();
			ProcessStartInfo startInfo = new ProcessStartInfo();
			startInfo.UseShellExecute = true;
			//startInfo.RedirectStandardOutput = true;
			startInfo.FileName = commandText;
			process.StartInfo = startInfo;
			process.Start();
		}
	}
}
