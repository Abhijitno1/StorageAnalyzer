using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FilesHunter
{
	public static class WinFormsConfirm
	{
		public static DialogResult ShowDialog(string text, string caption)
		{
			Form prompt = new Form()
			{
				Width = 520,
				Height = 100,
				FormBorderStyle = FormBorderStyle.FixedDialog,
				Text = caption,
				StartPosition = FormStartPosition.CenterScreen,
				AutoScroll = true
			};
			Button confirmYes = new Button() { Text = "Yes", Left = 50, Width = 100, Top = 100, DialogResult = DialogResult.Yes };
			confirmYes.Click += (sender, e) => { prompt.Close(); };
			prompt.Controls.Add(confirmYes);
			prompt.AcceptButton = confirmYes;
			//Ref: https://stackoverflow.com/questions/1204804/word-wrap-for-a-label-in-windows-forms
			Label textLabel = new Label() { Left = 20, Top = 20, Text = text, AutoSize = true, MaximumSize= new System.Drawing.Size(450, 0) };
			prompt.Controls.Add(textLabel);
			Button confirmNo = new Button() { Text = "No", Left = 350, Width = 100, Top = 100, DialogResult = DialogResult.No };
			confirmNo.Click += (sender, e) => { prompt.Close(); };
			prompt.Controls.Add(confirmNo);
			prompt.CancelButton = confirmNo;

			//ReAdjust control heights.
			confirmYes.Top = confirmNo.Top = textLabel.Bottom + 10;
			prompt.Height = confirmYes.Bottom + 50;

			return prompt.ShowDialog();
		}
	}
}
