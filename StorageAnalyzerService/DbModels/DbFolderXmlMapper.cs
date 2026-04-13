using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace StorageAnalyzerService.DbModels
{
	public static class DbFolderXmlMapper
	{
		public static DbFolder FromXmlString(string xml)
		{
			var doc = new XmlDocument();
			doc.LoadXml(xml);
			return FromXmlNode(doc.DocumentElement);
		}

		public static DbFolder FromXmlNode(XmlNode node)
		{
			if (node == null) return null;
			var folder = new DbFolder()
			{
				Name = node.Attributes?["name"]?.Value,
				FullPath = node.Attributes?["fullPath"]?.Value,
				SnapshotDate = ParseDate(node.Attributes?["snapshotDate"]?.Value),
				CreationDate = ParseDate(node.Attributes?["creationDate"]?.Value),
				SubFolders = new List<DbFolder>(),
				Files = new List<DbFile>()
			};

			foreach (XmlNode child in node.ChildNodes)
			{
				if (child.Name.Equals("folder", StringComparison.OrdinalIgnoreCase))
				{
					folder.SubFolders.Add(FromXmlNode(child));
				}
				else if (child.Name.Equals("file", StringComparison.OrdinalIgnoreCase))
				{
					var f = new DbFile()
					{
						Name = child.Attributes?["name"]?.Value,
						Extension = child.Attributes?["extension"]?.Value,
						Size = ParseLong(child.Attributes?["size"]?.Value),
						CreationDate = ParseDate(child.Attributes?["creationDate"]?.Value),
						DbId = child.Attributes?["DbId"]?.Value ?? child.Attributes?["dbId"]?.Value
					};
					folder.Files.Add(f);
				}
			}

			return folder;
		}

		private static DateTime ParseDate(string input)
		{
			if (string.IsNullOrWhiteSpace(input)) return DateTime.MinValue;
			DateTime dt;
			if (DateTime.TryParse(input, CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces, out dt))
				return dt;
			string[] fmts = new[] { "dd-MMM-yyyy", "dd-MMM-yyyy hh:mm tt", "dd-MMM-yyyy h:mm tt", "dd-MMM-yyyy HH:mm:ss" };
			if (DateTime.TryParseExact(input, fmts, CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces, out dt))
				return dt;
			return DateTime.MinValue;
		}

		private static long ParseLong(string input)
		{
			if (long.TryParse(input, out var v)) return v;
			return 0;
		}
	}
}
