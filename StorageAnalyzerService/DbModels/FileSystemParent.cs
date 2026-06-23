using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StorageAnalyzerService.DbModels
{
	public class FileSystemParent
	{
		[BsonId]
		[BsonRepresentation(BsonType.ObjectId)]
		public string Id { get; set; }
		public string ItemName { get; set; }
		[BsonIgnoreIfNull]
		public string FullPath { get; set; }
		public DateTime CreationDate { get; set; } = DateTime.Now;

	}
}
