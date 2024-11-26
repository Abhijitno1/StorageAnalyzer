using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace StorageAnalyzerService.DbModels
{
	public class ApplicationDbContext: DbContext
	{
		public ApplicationDbContext() 
			: base("DefaultConnection")
		{             
			//https://stackoverflow.com/questions/9230053/stop-entity-framework-from-modifying-database
			//Preventing the accidental creation of database
			Database.SetInitializer<ApplicationDbContext>(null);
		}

		public DbSet<Modak> Modaks { get; set; }

		public DbSet<FolderMap> FolderMaps { get; set; }
	}
}
