using StorageAnalyzerService;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
//using static System.Net.Mime.MediaTypeNames;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace FilesHunter
{
    public partial class ThumbnailViewer : UserControl
    {
        public string RootFolderPath { get; set; }
        public Image DefaultFileImage { get; set; }

        public delegate void GetDataDelegate(string itemName, string itemPath, frmMediaPreview.MediaType itemType, out object fileData);
        public event GetDataDelegate GetPreviewData;

        public delegate void OpenFolderDelegate(string itemName, string itemPath);
        public event OpenFolderDelegate OpenFolderToViewContents;

        public delegate void DeleteResourceDelegate(string itemName, string itemPath, string nodeType);
		public event DeleteResourceDelegate DeleteResource;

		public delegate void SaveResourceDelegate(string itemName, string itemPath, string nodeType);
		public event SaveResourceDelegate SaveResource;

		public delegate void RenameResourceDelegate(string itemName, string itemPath, string nodeType);
		public event RenameResourceDelegate RenameResource;

		public ThumbnailViewer()
        {
			//Ref: https://stackoverflow.com/questions/4710145/how-can-i-get-scrollbars-on-picturebox
			InitializeComponent();
        }

        public string SelectedNodePath
        {  
            get => txtSelectedNodePath.Text; 
            set => txtSelectedNodePath.Text = value;
        }
        public static Image BinaryToImage(byte[] binaryData)
        {
            if (binaryData == null) return null;
            MemoryStream memStream = new MemoryStream();
            int cursor = 0, chunk = 1024;
            memStream.Position = cursor;
            while (cursor < binaryData.Length)
            {
                if (cursor + chunk > binaryData.Length)
                {
                    chunk = binaryData.Length - cursor;
                }
                memStream.Write(binaryData,cursor, chunk);
                cursor += chunk;
            }
            return Image.FromStream(memStream);
        }

        public static Byte[] ImageToBinary(System.Drawing.Image input)
        {
            var ms = new MemoryStream();
            ImageCodecInfo pngCodec = GetEncoderInfo("image/png");
            var myEncoder = System.Drawing.Imaging.Encoder.Quality;
            var encoderParam = new EncoderParameter(myEncoder, 90L);    //Quality level 75
            var myEncoderParameters = new EncoderParameters(1);
            myEncoderParameters.Param[0] = encoderParam;
            input.Save(ms, pngCodec, myEncoderParameters);
            return ms.ToArray();
        }

        public void AddImageItem(NodeType nodeType, byte[] binary, string imgName, string relativeFolderPath)
        {
            this.Cursor = Cursors.WaitCursor;

            // Create a Thumnail of Image and add Thumbnail to Panel
            MakeThumbnail(nodeType, binary, imgName, relativeFolderPath);

            GC.GetTotalMemory(true);

            this.Cursor = Cursors.Default;
        }

        public string GetSelectedItemPath()
        {
            if (lvwTiles.SelectedItems.Count > 0)
            {
                var fileName = lvwTiles.SelectedItems[0].Text;
                var itemRelativePath = lvwTiles.SelectedItems[0].Name;
                return itemRelativePath + "\\" + fileName;
            }
            else
            { 
                return null;
            }
		}

		public void ClearImages()
        {
            imlTiles.Images.Clear();
            lvwTiles.Items.Clear();
        }

        private static ImageCodecInfo GetEncoderInfo(string mimeType)
        {
            foreach (ImageCodecInfo codec in ImageCodecInfo.GetImageEncoders())
                if (codec.MimeType == mimeType)
                    return codec;

            return null;
        }

        private void MakeThumbnail(NodeType nodeType, byte[] binary, string imgName, string folderPath)
        {
            Image thumbImage;
            try
            {
                //Set thumbnail image
                using (MemoryStream ms = new MemoryStream(binary))
                {
                    var originalImage = System.Drawing.Image.FromStream(ms);
                    thumbImage = originalImage.GetThumbnailImage(imlTiles.ImageSize.Width - 2, imlTiles.ImageSize.Height - 2, null, new IntPtr());
                    //Add to Imagelist and thereafter to listview
                    imlTiles.Images.Add(thumbImage);
                    ms.Close();
                }
            }
            catch (Exception ex)
            {
                //ToDo: Add check for specific exception type (incompatible format) while image reading
                if (nodeType == NodeType.File)
                {
                    imlTiles.Images.Add(this.DefaultFileImage);
                }
            }
            var listItem = new ListViewItem();
            listItem.Name = folderPath;
            listItem.Text = imgName;
            listItem.Tag = nodeType.ToString();
            listItem.ImageIndex = imlTiles.Images.Count - 1;
            lvwTiles.Items.Add(listItem);
        }

        private void lvwTiles_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            ListViewItem clickedItem = null;
            for (int itemIndex = 0; itemIndex < lvwTiles.Items.Count; itemIndex++)
            {
                ListViewItem item = lvwTiles.Items[itemIndex];
                Rectangle itemRect = item.GetBounds(ItemBoundsPortion.ItemOnly);
                if (itemRect.Contains(e.Location))
                {
                    clickedItem = item;
                    break;
                }
            }
            if (clickedItem != null)
            {
                if (lvwTiles.SelectedItems[0].Tag.ToString() == NodeType.File.ToString())
                    PreviewMedia();
                else
                    if (OpenFolderToViewContents != null) OpenFolderToViewContents(clickedItem.Text, clickedItem.Name);
            }
        }

		private void PreviewMedia()
        {
			int index = lvwTiles.SelectedIndices[0];
            var fileName = lvwTiles.SelectedItems[0].Text;
            var itemRelativePath = lvwTiles.SelectedItems[0].Name;
			var extn = Path.GetExtension(fileName);
            frmMediaPreview.MediaType curMediaType = GetMediaTypeFromFileName(fileName);
            object fileData = null;
            if (GetPreviewData != null)
            {
				GetPreviewData(fileName, itemRelativePath, curMediaType, out fileData);
				frmMediaPreview previewForm = new frmMediaPreview();
				previewForm.ShowMedia(curMediaType, fileName, fileData);
			}
		}

        private frmMediaPreview.MediaType GetMediaTypeFromFileName(string fileName)
        {
			var extn = Path.GetExtension(fileName);
			frmMediaPreview.MediaType curMediaType = frmMediaPreview.MediaType.Other;
			if ((new string[] { ".rtf", ".txt" }).Contains(extn))
				curMediaType = frmMediaPreview.MediaType.Text;
			//Note: Below we include only image types supported by picturebox for viewing 
			else if ((new string[] { ".jpg", ".jpeg", ".png", ".gif", ".avif", ".webp", ".tiff", ".bmp" }).Contains(extn))
				curMediaType = frmMediaPreview.MediaType.Image;
			else if ((new string[] { ".mp4", ".wmv", ".asf", ".mp3", ".wma" }).Contains(extn))
				curMediaType = frmMediaPreview.MediaType.Video;
            return curMediaType;
		}

        private void tsMnuItmDeleteFromDB_Click(object sender, EventArgs e)
        {
            var selectedListItem = lvwTiles.SelectedItems[0];
			var fileName = selectedListItem.Text;
            var itemRelativePath = selectedListItem.Name;
            DeleteResource?.Invoke(fileName, itemRelativePath, selectedListItem.Tag.ToString());
        }

		private void lvwTiles_MouseClick(object sender, MouseEventArgs e)
		{
			//Ref: https://stackoverflow.com/questions/13437889/showing-a-context-menu-for-an-item-in-a-listview
			if (e.Button == MouseButtons.Right)
			{
				var focusedItem = lvwTiles.FocusedItem;
				if (focusedItem != null && focusedItem.Bounds.Contains(e.Location))
				{
					listViewContextMenu.Show(Cursor.Position);
				}
			}
		}

		private void tsMnuItmSaveToDisk_Click(object sender, EventArgs e)
		{
            var selectedListItem = lvwTiles.SelectedItems[0];
            var fileName = selectedListItem.Text;
			var itemRelativePath = selectedListItem.Name;
            if (SaveResource != null)
            {
				SaveResource(fileName, itemRelativePath, selectedListItem.Tag.ToString());
			}
		}

		private void tsMnuItmRenameIt_Click(object sender, EventArgs e)
		{
            var selectedListItem = lvwTiles.SelectedItems[0];
            var fileName = selectedListItem.Text;
			var itemRelativePath = selectedListItem.Name;
			if (RenameResource != null)
			{
				RenameResource(fileName, itemRelativePath, selectedListItem.Tag.ToString());
			}
		}

        private void ThumbnailViewer_Resize(object sender, EventArgs e)
        {
            lvwTiles.Height = this.ClientSize.Height - txtSelectedNodePath.Height - 2;
            //Debug.WriteLine($"lvwTiles.Height = {lvwTiles.Height}, ClientSize.Height = {this.ClientSize.Height}, txtSelectedNodePath.Height = {txtSelectedNodePath.Height}");
        }

        //private void OldPreivewForm()
        //{
        //	Form previewForm = new Form();
        //	previewForm.FormBorderStyle = FormBorderStyle.SizableToolWindow;
        //	previewForm.MinimizeBox = false;
        //	previewForm.Size = new System.Drawing.Size(1000, 860);
        //	previewForm.StartPosition = FormStartPosition.CenterScreen;
        //	previewForm.AutoScroll = true;

        //	PictureBox view = new PictureBox();
        //	view.Dock = DockStyle.Fill;

        //	int index = lvwTiles.SelectedIndices[0];
        //	view.Image = BinaryToImage(ImageList[index]);

        //	view.SizeMode = PictureBoxSizeMode.Zoom;
        //	previewForm.Controls.Add(view);
        //	previewForm.ShowDialog();
        //}

    }
}
