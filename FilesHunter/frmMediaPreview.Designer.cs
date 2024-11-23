namespace FilesHunter
{
	partial class frmMediaPreview
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmMediaPreview));
			this.axWMP = new AxWMPLib.AxWindowsMediaPlayer();
			this.picView = new System.Windows.Forms.PictureBox();
			this.rtbSlate = new System.Windows.Forms.RichTextBox();
			((System.ComponentModel.ISupportInitialize)(this.axWMP)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.picView)).BeginInit();
			this.SuspendLayout();
			// 
			// axWMP
			// 
			this.axWMP.Enabled = true;
			this.axWMP.Location = new System.Drawing.Point(0, 0);
			this.axWMP.Name = "axWMP";
			this.axWMP.OcxState = ((System.Windows.Forms.AxHost.State)(resources.GetObject("axWMP.OcxState")));
			this.axWMP.Size = new System.Drawing.Size(1202, 733);
			this.axWMP.TabIndex = 0;
			this.axWMP.Visible = false;
			// 
			// picView
			// 
			this.picView.Location = new System.Drawing.Point(-9, 0);
			this.picView.Name = "picView";
			this.picView.Size = new System.Drawing.Size(1211, 733);
			this.picView.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
			this.picView.TabIndex = 1;
			this.picView.TabStop = false;
			this.picView.Visible = false;
			// 
			// rtbSlate
			// 
			this.rtbSlate.Location = new System.Drawing.Point(0, 0);
			this.rtbSlate.Name = "rtbSlate";
			this.rtbSlate.Size = new System.Drawing.Size(1202, 733);
			this.rtbSlate.TabIndex = 2;
			this.rtbSlate.Text = "";
			this.rtbSlate.Visible = false;
			// 
			// frmMediaPreview
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.AutoScroll = true;
			this.ClientSize = new System.Drawing.Size(1202, 733);
			this.Controls.Add(this.rtbSlate);
			this.Controls.Add(this.picView);
			this.Controls.Add(this.axWMP);
			this.Name = "frmMediaPreview";
			this.Text = "Media Preview";
			((System.ComponentModel.ISupportInitialize)(this.axWMP)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.picView)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private AxWMPLib.AxWindowsMediaPlayer axWMP;
		private System.Windows.Forms.PictureBox picView;
		private System.Windows.Forms.RichTextBox rtbSlate;
	}
}