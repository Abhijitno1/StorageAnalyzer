using CsvHelper;
using Microsoft.VisualBasic.FileIO;
using StorageAnalyzerService.DbModels;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StorageAnalyzerService
{
	public class CSVUtils
	{
		public DataTable GetDataTableFromCSVFile(string csv_file_path, bool hasFieldsEnclosedInQuotes = false)
		{
			var csvData = new DataTable();
			try
			{
				using (TextFieldParser csvReader = new TextFieldParser(csv_file_path))
				{
					csvReader.SetDelimiters(new string[] { "," });
					csvReader.HasFieldsEnclosedInQuotes = hasFieldsEnclosedInQuotes;
					string[] colFields = csvReader.ReadFields();
					foreach (string column in colFields)
					{
						DataColumn datecolumn = new DataColumn(column);
						datecolumn.AllowDBNull = true;
						csvData.Columns.Add(datecolumn);
					}

					while (!csvReader.EndOfData)
					{
						string[] fieldData = csvReader.ReadFields();
						csvData.Rows.Add(fieldData);
					}
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine("Exception:{0}", ex.Message);
			}
			return csvData;
		}
		public void CreateCsvFile(string filePath, IEnumerable<FileSystemItem> dataRecords)
		{
			using (StreamWriter textWriter = new StreamWriter(filePath))
			using (var writer = new CsvWriter(textWriter, CultureInfo.InvariantCulture))
			{
				writer.WriteField("ItemName");
				writer.WriteField("FullPath");
				writer.WriteField("DbId");
				writer.WriteField("Size");
				writer.WriteField("CreationDate");
				writer.WriteHeader<FileSystemItem>();
				writer.NextRecord();
				foreach (FileSystemItem item in dataRecords)
				{
					writer.WriteField(item.ItemName);
					writer.WriteField(item.FullPath);
					writer.WriteField(item.DbId);
					writer.WriteField(item.Size);
					writer.WriteField(item.Children);
					writer.NextRecord();
				}
			}
		}

		public List<string> ConvertFromDataTable2StringList(DataTable input)
		{
			var output = new List<string>();
			for (var i = 0; i < input.Rows.Count; i++)
			{
				var dataRow = input.Rows[i];
				output.Add(Convert.ToString(dataRow[0]));
			}
			return output;
		}


		public void WriteCSVFileFromArray(IEnumerable<IEnumerable<string>> inputlist, string csv_file_path, string[] columnHeaders)
		{
			using (FileStream fileStream = new FileStream(csv_file_path, FileMode.Create, FileAccess.Write))
			{
				using (var outputFile = new StreamWriter(fileStream))
				{
					CleanupData4Csv(columnHeaders);
					var columnHeadersJoined = string.Join(",", columnHeaders);
					outputFile.WriteLine(columnHeadersJoined);
					for (int i = 0; i< inputlist.Count(); i++)
					{
						var currentRow = inputlist.ElementAt(i).ToArray();
						CleanupData4Csv(currentRow);
						outputFile.WriteLine(string.Join(",", currentRow));
					}
				}
			}
		}

		private void CleanupData4Csv(string[] items)
		{
			for (int j=0; j<items.Length; j++)
			{
				if (items[j].Contains(',') || items[j].Contains('\r'))
					items[j] = "\"" + items[j] + "\"";
			}
		}
	}
}
