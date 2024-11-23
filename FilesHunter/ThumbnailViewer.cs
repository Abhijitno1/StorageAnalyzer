using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FilesHunter
{
    public partial class ThumbnailViewer : UserControl
    {
        public string RootFolderPath { get; set; }

        public delegate void GetDataDelegate(string itemName, string itemPath, frmMediaPreview.MediaType itemType, out object fileData);

        public event GetDataDelegate GetPreviewData;

        public ThumbnailViewer()
        {
            InitializeComponent();
        }

        public static Image BinaryToImage(byte[] binaryData)
        {
            if (binaryData == null) return null;
            byte[] buffer = binaryData.ToArray();
            MemoryStream memStream = new MemoryStream();
            memStream.Write(buffer, 0, buffer.Length);
            return Image.FromStream(memStream);
        }

        public static Byte[] ImageToBinary(Image input)
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

        public void AddImageItem(byte[] binary, string imgName, string relativeFolderPath)
        {
            this.Cursor = Cursors.WaitCursor;

            // Create a Thumnail of Image and add Thumbnail to Panel
            MakeThumbnail(binary, imgName, relativeFolderPath);

            GC.GetTotalMemory(true);

            this.Cursor = Cursors.Default;
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

        private void MakeThumbnail(byte[] binary, string imgName, string folderPath)
        {

            // Set thumbnail image
            MemoryStream ms = new MemoryStream();
            var thumbImage = Image.FromStream(new MemoryStream(binary))
                .GetThumbnailImage(imlTiles.ImageSize.Width - 2, imlTiles.ImageSize.Height - 2, null, new IntPtr());
            ms.Close();

            // Add to Imagelist and thereafter to listview
            imlTiles.Images.Add(thumbImage);

            var listItem = new ListViewItem();
            listItem.Name = folderPath;
            listItem.Text = imgName;
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
				PreviewMedia();
            }
        }

        private void PreviewMedia()
        {
			int index = lvwTiles.SelectedIndices[0];
            var fileName = lvwTiles.SelectedItems[0].Text;
            var itemRelativePath = lvwTiles.SelectedItems[0].Name;
			var extn = Path.GetExtension(fileName);
            frmMediaPreview.MediaType curMediaType = frmMediaPreview.MediaType.Other;
            if ((new string[] { ".rtf", ".txt" }).Contains(extn))
                curMediaType = frmMediaPreview.MediaType.Text;
			else if ((new string[] { ".jpg", ".jpeg", ".png", ".gif", ".avif", ".webp", ".tiff", ".bmp", ".svg" }).Contains(extn))
				curMediaType = frmMediaPreview.MediaType.Image;
			else if ((new string[] { ".mp4", ".wmv", ".asf", ".mp3", ".wma" }).Contains(extn))
				curMediaType = frmMediaPreview.MediaType.Video;

            object fileData = null;
            if (GetPreviewData != null) GetPreviewData(fileName, $"{RootFolderPath}\\{itemRelativePath}", curMediaType, out fileData);
            frmMediaPreview previewForm = new frmMediaPreview();
            previewForm.ShowMedia(curMediaType, fileName, fileData);
		}

		//private void OldPreivewForm()
  //      {
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
