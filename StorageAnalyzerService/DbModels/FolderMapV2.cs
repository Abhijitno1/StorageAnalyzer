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

	[JsonObject("folder")]
	public class DbFolder
	{
		[JsonProperty("@name")]
		public string Name { get; set; }
		[JsonProperty("@fullPath")]
		public string FullPath { get; set; }
		[JsonProperty("@snapshotDate")]
		public DateTime SnapshotDate { get; set; }
		[JsonProperty("@CreationDate")]
		public DateTime CreationDate { get; set; }
		public List<DbFolder> SubFolders { get; set; }
		public List<DbFile> Files { get; set; }
	}

	[JsonObject("file")]
	public class DbFile
	{
		[JsonProperty("@name")]
		public string Name { get; set; }
		[JsonProperty("@extension")]
		public string Extension { get; set; }
		[JsonProperty("@size")]
		public long Size { get; set; }
		[JsonProperty("@creationDate")]
		public DateTime CreationDate { get; set; }
		[JsonProperty("@dbId")]
		public string DbId { get; set; }
	}

}
