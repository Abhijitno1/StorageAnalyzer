using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using StorageAnalyzerService;
using StorageAnalyzerService.DbModels;

namespace StorageAnalyzerService
{
	public class DirectoryMapDbSaver
	{
		public string RootFolderPath { get; set; }
		ApplicationDbContext dbContext = new ApplicationDbContext();
		XmlDocument xmlDoc = new XmlDocument();

		public void SaveMap()
		{
			DirectoryInfo rootFolder = new DirectoryInfo(RootFolderPath);
			xmlDoc = new XmlDocument();
			TraverseFolder(rootFolder, null);
			var xml2save = xmlDoc.OuterXml;
			FolderMap folderMap = new FolderMap()
			{
				AbsolutePath = RootFolderPath,
				DirectoryXml = xml2save
			};
			dbContext.FolderMaps.Add(folderMap);
			dbContext.SaveChanges();
		}

		public void UpdateMap(XmlDocument editedDoc)
		{
			var identifier = editedDoc.DocumentElement.Attributes["fullPath"].Value;
			if (identifier != null)
			{
				var foundRec = dbContext.FolderMaps.Where(folderMap=> folderMap.AbsolutePath == identifier);
				if (foundRec.Any())
				{
					var rec2Update = foundRec.First();
					rec2Update.DirectoryXml = editedDoc.DocumentElement.OuterXml;
					dbContext.SaveChanges();
				}
			}
		}

		public bool DeleteFolderMap(string absolutePath)
		{
			var foundFolderMaps = dbContext.FolderMaps.Where(k => k.AbsolutePath == absolutePath);
			if (foundFolderMaps.Any())
			{
				//First remove any stored file dependencies in database
				DeleteModaksForNaksha(absolutePath);
				//Then remove folder map from Xml maps
				dbContext.FolderMaps.Remove(foundFolderMaps.First());
				return dbContext.SaveChanges() > 0;
			}
			return false;
		}

		private void TraverseFolder(DirectoryInfo currentFolder, XmlNode parentNode)
		{
			var folderNode = xmlDoc.CreateElement("folder");
			folderNode.SetAttribute("name",currentFolder.Name);
			folderNode.SetAttribute("creationDate", currentFolder.CreationTime.ToString("dd-MMM-yyyy"));

			if (parentNode == null)
			{
				folderNode.SetAttribute("fullPath", currentFolder.FullName);
				folderNode.SetAttribute("snapshotDate", DateTime.Now.ToString("dd-MMM-yyyy hh:mm tt"));
			}
			//Console.WriteLine(currentFolder.Name);
			//Console.WriteLine(currentFolder.FullName);
			//Console.WriteLine(currentFolder.CreationTime.ToShortDateString());

			TraverseFiles(currentFolder, folderNode);

			foreach (var childFolder in currentFolder.EnumerateDirectories())
			{
				TraverseFolder(childFolder, folderNode);
			}
			if (parentNode == null)
				xmlDoc.AppendChild(folderNode);    //First Child is the greatest ancestor
			else
				parentNode.AppendChild(folderNode);
		}

		private void TraverseFiles(DirectoryInfo currentfolder, XmlNode parentNode)
		{
			XmlNode fileNode = null;

			foreach (var childFile in currentfolder.EnumerateFiles())
			{
				fileNode = xmlDoc.CreateElement("file");
				var fileElm = fileNode as XmlElement; ;
				fileElm.SetAttribute("name", childFile.Name);
				fileElm.SetAttribute("extension", childFile.Extension);
				fileElm.SetAttribute("creationDate", childFile.CreationTime.ToString("dd-MMM-yyyy"));
				fileElm.SetAttribute("size", childFile.Length.ToString());

				//Console.WriteLine(childFile.Name);
				//Console.WriteLine(childFile.Extension);
				//Console.WriteLine(childFile.CreationTime.ToShortDateString());
				parentNode.AppendChild(fileNode);

				var cutPoint = childFile.FullName.IndexOf(this.RootFolderPath) > -1 ? this.RootFolderPath.Length : 0;
				var relativePath = childFile.FullName.Substring(cutPoint);
				//Also Add to DB
				Modak modak = new Modak()
				{
					Title = childFile.Name,
					RelativePath = relativePath
				};
				var fs = childFile.OpenRead();
				var fileData = new byte[fs.Length];
				//ToDo: Optiomize this file read in future
				fs.Read(fileData, 0, (int)fs.Length);
				fs.Dispose();
				modak.PicData = fileData;
				InsertModakIntoDb(modak);
				//Ref: https://stackoverflow.com/questions/5212751/how-can-i-retrieve-id-of-inserted-entity-using-entity-framework
				fileElm.SetAttribute("DbId", modak.Id.ToString());
			}
		}

		public bool DeleteModaksForNaksha(string absolutePath)
		{
			var foundFolderMaps = dbContext.FolderMaps.Where(k => k.AbsolutePath == absolutePath);
			if (foundFolderMaps.Any())
			{
				var xml = foundFolderMaps.First().DirectoryXml;
				var xmlDoc = new XmlDocument();
				xmlDoc.LoadXml(xml);
				DeleteFiles(xmlDoc.DocumentElement);
			}
			return true;
		}

		private bool DeleteFiles(XmlNode node)
		{
			foreach (XmlNode child in node.ChildNodes)
			{
				if (child.Name == "file")
				{
					var modakId = Convert.ToInt32(child.Attributes["DbId"].Value);
					DeleteModak(modakId);
				}
				else
				{
					DeleteFiles(child);
				}
			}
			return true;
		}

		public bool UpdateModak(Modak modak)
		{
			var foundModak = dbContext.Modaks.Find(modak.Id);
			if (foundModak != null)
			{
				foundModak.Title = modak.Title;
				foundModak.RelativePath = modak.RelativePath;
				return dbContext.SaveChanges() > 0;
			}
			return false;
		}

		public bool DeleteModak(int dbId)
		{
			var foundModak = dbContext.Modaks.Find(dbId);
			if (foundModak != null)
			{
				dbContext.Modaks.Remove(foundModak);
				dbContext.SaveChanges();
				return true;
			}
			return false;
		}

		public bool InsertModakIntoDb(Modak modak)
		{
			dbContext.Modaks.Add(modak);
			dbContext.SaveChanges();
			return true;
		}

	}
}
