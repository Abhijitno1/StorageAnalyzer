using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StorageAnalyzerService.DbModels
{
	[BsonIgnoreExtraElements]
	public class FileSystemElement
	{
		[BsonId]
		[BsonRepresentation(BsonType.ObjectId)]
		public string Id { get; set; }

		[BsonRepresentation(BsonType.ObjectId)] 
		public string parentId { get; set; }

		[BsonRepresentation(BsonType.ObjectId)]
		public string rootId { get; set; }
		public bool IsFolder { get; set; } = false;
		public string ItemName { get; set; }
		[BsonIgnoreIfNull]
		public string FullPath { get; set; }
		public string Extension { get; set; }
		public long Size { get; set; }
		public int Order { get; set; }
		public string DbId { get; set; }
		public DateTime CreationDate { get; set; }
		[BsonIgnoreIfNull]
		public string DataHash { get; set; }
	}
}
