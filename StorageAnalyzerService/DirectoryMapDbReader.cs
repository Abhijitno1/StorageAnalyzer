using StorageAnalyzerService.DbModels;
using System;
using System.Collections.Generic;
using System.Data.Entity.Core;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
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
			try
			{
                return dbContext.FolderMaps.Select(k => k.AbsolutePath).ToList();
            }
            catch (Exception ex)
			{
                if (ex.GetType() == typeof(EntityException))
                {
                    if (ex.Message.CompareTo("The underlying provider failed on Open.") == 0)
                    {
                        MessageBox.Show("Could not connect to database. Please check if the database is running and try again.");
						throw;
					}
                }
				return new string[0];
            }
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
