using StorageAnalyzerService.DbModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace StorageAnalyzerService
{
	public class DirectoryMapDbReader
	{
		public string RootFolderPath { get; set; }
		ApplicationDbContext dbContext = new ApplicationDbContext();

		public XmlDocument GetMap()
		{
			string folderMapText = null;
			var folderMap = dbContext.FolderMaps.Where(naksha => naksha.AbsolutePath == RootFolderPath);
			if (folderMap.Any()) folderMapText = folderMap.First().DirectoryXml;
			var retDoc = new XmlDocument();
			retDoc.LoadXml(folderMapText);
			return retDoc;
		}

		public IEnumerable<string> GetAllFolderMapsList()
		{
			return dbContext.FolderMaps.Select(k => k.AbsolutePath).ToList();
		}

		public Byte[] GetModakData(int dbId)
		{
			byte[] output = null;
			var foundModak = dbContext.Modaks.Find(dbId);
			if (foundModak != null) output = foundModak.PicData;
			return output;
		}

		public Modak GetModak(int dbId)
		{
			return dbContext.Modaks.Find(dbId);
		}
	}
}
