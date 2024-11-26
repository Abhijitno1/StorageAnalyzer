using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StorageAnalyzerService.DbModels
{
	[Table("Modak")]
	public class Modak
	{
		public int Id { get; set; }
		public string Title { get; set; }
		public string RelativePath { get; set; }
		public byte[] PicData { get; set; }
	}
}
