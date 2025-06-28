using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Xml.Linq;

namespace StorageAnalyzerService
{
	public class DirectoryMapTextSaver
	{
        public string RootFolderPath { get; set; }
		public string OutputFilePathName { get; set; }

		private TextWriter outWriter;

		public void SaveMap()
		{
			DirectoryInfo rootFolder = new DirectoryInfo(RootFolderPath);
			outWriter = new StreamWriter(OutputFilePathName, false, Encoding.UTF8);
			outWriter.Write("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
			TraverseFolder(rootFolder, true);
			outWriter.Close();
			outWriter.Dispose();
		}

		private void TraverseFolder(DirectoryInfo currentFolder, bool isRoot)
		{
			outWriter.Write("<folder");
			var attributeString = new StringBuilder();
			attributeString.Append($" name = \"{HttpUtility.HtmlEncode(currentFolder.Name)}\"");
			attributeString.Append($" creationDate = \"{currentFolder.CreationTime.ToString("dd-MMM-yyyy")}\"");
			if (isRoot)
			{
				attributeString.Append($" fullPath = \"{HttpUtility.HtmlEncode(currentFolder.FullName)}\"");
				attributeString.Append($" snapshotDate = \"{HttpUtility.HtmlEncode(DateTime.Now.ToString("dd-MMM-yyyy hh:mm tt"))}\"");
			}
			outWriter.Write(attributeString.ToString() + ">");
			//Console.WriteLine(currentFolder.Name);
			//Console.WriteLine(currentFolder.FullName);
			//Console.WriteLine(currentFolder.CreationTime.ToShortDateString());

			TraverseFiles(currentFolder);

			foreach (var childFolder in currentFolder.EnumerateDirectories())
			{
				TraverseFolder(childFolder, false);
			}
			outWriter.Write("</folder>");
		}

		private void TraverseFiles(DirectoryInfo currentfolder)
		{
			Func<string, bool> tryParseXml = (string word) =>
			{
				try
				{
					XDocument.Parse($"<word>{word}</word>");
				}
				catch (Exception)
				{
					Console.WriteLine($"File: {word} will not be included.");
					return false;
				}
				return true;
			};

			foreach (var childFile in currentfolder.EnumerateFiles())
			{
				try
				{
					var fileName = HttpUtility.HtmlEncode(childFile.Name);
					//Console.WriteLine(childFile.Name);
					if (!tryParseXml(fileName)) continue;
					outWriter.Write("<file");
					var attributeString = new StringBuilder();
					attributeString.Append($" name = \"{fileName}\"");
					attributeString.Append($" extension = \"{HttpUtility.HtmlEncode(childFile.Extension)}\"");
					attributeString.Append($" creationDate = \"{HttpUtility.HtmlEncode(childFile.CreationTime.ToString("dd-MMM-yyyy"))}\"");
					attributeString.Append($" size = \"{HttpUtility.HtmlEncode(childFile.Length.ToString())}\"");
					//Console.WriteLine(childFile.Name);
					//Console.WriteLine(childFile.Extension);
					//Console.WriteLine(childFile.CreationTime.ToShortDateString());
					outWriter.Write(attributeString.ToString() + ">");
					outWriter.Write("</file>");
				}
				catch (Exception ex)
				{
					Console.WriteLine($"File: {childFile.FullName} could not be included.");
					Console.WriteLine(ex);
				}
			}
		}
	}

}
