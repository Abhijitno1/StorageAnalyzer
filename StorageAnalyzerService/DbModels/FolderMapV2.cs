using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using System.Xml;
using System.Globalization;

namespace StorageAnalyzerService.DbModels
{
	[BsonIgnoreExtraElements]
	public class FolderMapV2
	{
		[BsonId]
		[BsonRepresentation(BsonType.ObjectId)]
		public string Id { get; set; }
		public string AbsolutePath { get; set; }
		public string Alias { get; set; }

		// Existing XML data
		public string DirectoryXml { get; set; }

		// New JSON data (stored as a BsonDocument for native MongoDB querying)
		[BsonIgnoreIfNull]
		public BsonDocument DirectoryJson { get; set; }

	}

	public class FileSystemItem
	{
		public bool IsFolder { get; set; } = false;
		public string ItemName { get; set; }
		public string FullPath { get; set; }
		public string Extension { get; set; }
		public long Size { get; set; }
		public string DbId { get; set; }
		public DateTime CreationDate { get; set; }
		public List<FileSystemItem> Children { get; set; } = new List<FileSystemItem>();
	}

}
