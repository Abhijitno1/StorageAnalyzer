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
		public static FileSystemItem FromXmlString(string xml)
		{
			var doc = new XmlDocument();
			doc.LoadXml(xml);
			return FromXmlNode(doc.DocumentElement);
		}

		public static FileSystemItem FromXmlNode(XmlNode node, string parentPath = null)
		{
			FileSystemItem currentItem;
			if (node == null) return null;
			if (node.Name.Equals("folder", StringComparison.OrdinalIgnoreCase))
			{
				currentItem = new FileSystemItem()
				{
					IsFolder = true,
					ItemName = node.Attributes?["name"]?.Value,
					FullPath = (parentPath == null) ? node.Attributes?["fullPath"]?.Value : parentPath + "\\" + node.Attributes?["name"]?.Value,
					CreationDate = (node.Attributes?["snapshotDate"] != null) ?
						ParseDate(node.Attributes?["snapshotDate"]?.Value) : ParseDate(node.Attributes?["creationDate"]?.Value),
					Children = new List<FileSystemItem>()
				};
				foreach (XmlNode child in node.ChildNodes)
				{
					currentItem.Children.Add(FromXmlNode(child, currentItem.FullPath));
				}
			}
			else
			{
				currentItem = new FileSystemItem()
				{
					IsFolder = false,
					ItemName = node.Attributes?["name"]?.Value,
					FullPath = (parentPath == null) ? node.Attributes?["fullPath"]?.Value : parentPath + "\\" + node.Attributes?["name"]?.Value,
					Extension = node.Attributes?["extension"]?.Value,
					Size = ParseLong(node.Attributes?["size"]?.Value),
					CreationDate = ParseDate(node.Attributes?["creationDate"]?.Value),
					DbId = node.Attributes?["DbId"]?.Value ?? node.Attributes?["dbId"]?.Value
				};
			}

			return currentItem;
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
