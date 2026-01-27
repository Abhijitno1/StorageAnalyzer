using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StorageAnalyzerService.DbModels
{
	public class ModakV2
	{
        [BsonId]	// Maps this property to the MongoDB "_id" field
        [BsonRepresentation(BsonType.ObjectId)]
        [BsonIgnoreIfDefault] // This allows auto-generation when Id is null
        public string Id { get; set; }
		public string Title { get; set; }
		public string RelativePath { get; set; }
		public byte[] PicData { get; set; } // Binary data < 16MB
    }
}
