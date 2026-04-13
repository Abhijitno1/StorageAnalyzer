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
	//[Table("FolderMap")]
	public class FolderMap
	{
		[Key]
		public int Id { get; set; }
		public string AbsolutePath { get; set; }
		public string Alias { get; set; }
		// Existing XML data
		public string DirectoryXml { get; set; }
	}
}
