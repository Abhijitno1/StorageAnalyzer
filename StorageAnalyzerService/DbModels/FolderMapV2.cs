using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StorageAnalyzerService.DbModels
{
	public class FolderMapV2
	{
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
		public string AbsolutePath { get; set; }
		public string Alias { get; set; }
		public string DirectoryXml { get; set; }
	}
}
