using CsvHelper;
using CsvHelper.Configuration;
using StorageAnalyzerService.DbModels;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Globalization;
using System.IO;
using System.Linq;

namespace StorageAnalyzerService
{
    public class ExcelGenerator
    {
        public enum ExcelDataTypes
        {
            TEXT,
            INT,
            DOUBLE,
            DATETIME,
            CURRENCY,
            BOOLEAN
        }

        public void CreateExcelFileWithSchema(string filePath, string sheetName, Tuple<string, ExcelDataTypes>[] columns)
        {
            // You must have the Microsoft Access Database Engine Redistributable installed on the machine running the code.
            // 1. Define the connection string. 
            // Use 'Excel 12.0 Xml' for .xlsx files. HDR=YES indicates the first row contains headers.
            string connString = $@"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={filePath};Extended Properties=""Excel 12.0 Xml;HDR=YES;""";
            using (OleDbConnection conn = new OleDbConnection(connString))
            {
                conn.Open();
                // 2. Create a "Table" which becomes a Worksheet in Excel
                // Note: Use square brackets around the sheet name
                string createTableSql = $"CREATE TABLE [{sheetName}] (";
                for (int i = 0; i < columns.Length; i++)
                {
                    createTableSql += $"[{columns[i].Item1}] {columns[i].Item2.ToString()}";
                    if (i < columns.Length - 1)
                        createTableSql += ", ";
                }
                createTableSql += ")";
                using (OleDbCommand cmd = new OleDbCommand(createTableSql, conn))
                {
                    cmd.ExecuteNonQuery();
                }
                conn.Close();
            }
        }

        public void InsertDataIntoExcel(string filePath, string sheetName, object[][] data)
        {
            string connString = $@"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={filePath};Extended Properties=""Excel 12.0 Xml;HDR=YES;""";
            using (OleDbConnection conn = new OleDbConnection(connString))
            {
                conn.Open();
                foreach (var row in data)
                {
                    string insertSql = $"INSERT INTO [{sheetName}] VALUES (";
                    for (int i = 0; i < row.Length; i++)
                    {
                        insertSql += "?";
                        if (i < row.Length - 1)
                            insertSql += ", ";
                    }
                    insertSql += ")";
                    using (OleDbCommand insertCmd = new OleDbCommand(insertSql, conn))
                    {
                        for (int i = 0; i < row.Length; i++)
                        {
                            insertCmd.Parameters.AddWithValue("?", row[i]);
                        }
                        insertCmd.ExecuteNonQuery();
                    }
                }
                conn.Close();
            }
        }

        public void CreateCsvFile(string filePath, string[] headers, object[][] data)
        {
            using (StreamWriter writer = new StreamWriter(filePath))
            {
                // Write headers
                if (headers != null)
                    writer.WriteLine(string.Join(",", headers));

                // Write data rows
                foreach (var row in data)
                {
                    string[] stringValues = Array.ConvertAll(row, item => item?.ToString());
                    writer.WriteLine(string.Join(",", stringValues));
                }
            }
		}
            
        public List<ModakV2> ReadCsvFileToList(string filePathName)
        {
            IEnumerable<ModakV2> records;
			var config = new CsvConfiguration(CultureInfo.InvariantCulture)
			{
				// Ignore missing fields during reading
				MissingFieldFound = null,
				// Ignore headers that exist in the map but not in the CSV
				HeaderValidated = null
			};
			using (var reader = new StreamReader(filePathName))
			using (var csv = new CsvReader(reader, config))
			{
				records = csv.GetRecords<ModakV2>();
				return records?.ToList();
			}
		}

		public void CreateExcelFile2(string filePath)
        {
            // You must have the Microsoft Access Database Engine Redistributable installed on the machine running the code.
            // 1. Define the connection string. 
            // Use 'Excel 12.0 Xml' for .xlsx files. HDR=YES indicates the first row contains headers.
            string connString = $@"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={filePath};Extended Properties=""Excel 12.0 Xml;HDR=YES;""";

            using (OleDbConnection conn = new OleDbConnection(connString))
            {
                conn.Open();

                // 2. Create a "Table" which becomes a Worksheet in Excel
                // Note: Use square brackets around the sheet name
                string createTableSql = "CREATE TABLE [SalesData] ([ID] INT, [ProductName] TEXT, [Price] CURRENCY)";
                using (OleDbCommand cmd = new OleDbCommand(createTableSql, conn))
                {
                    cmd.ExecuteNonQuery();
                }

                // 3. Insert data into the worksheet
                string insertSql = "INSERT INTO [SalesData] ([ID], [ProductName], [Price]) VALUES (?, ?, ?)";
                using (OleDbCommand insertCmd = new OleDbCommand(insertSql, conn))
                {
                    // Using parameters to prevent injection and handle data types correctly
                    insertCmd.Parameters.AddWithValue("?", 1);
                    insertCmd.Parameters.AddWithValue("?", "Widget A");
                    insertCmd.Parameters.AddWithValue("?", 25.50);
                    insertCmd.ExecuteNonQuery();

                    insertCmd.Parameters.Clear();
                    insertCmd.Parameters.AddWithValue("?", 2);
                    insertCmd.Parameters.AddWithValue("?", "Widget B");
                    insertCmd.Parameters.AddWithValue("?", 40.00);
                    insertCmd.ExecuteNonQuery();
                }

                conn.Close();
            }
        }

        public DataTable ReadExcelToDataTable(string filePath, string sheetName)
        {
            // IMEX=1 tells the driver to treat mixed data types as text to avoid nulls
            string connString = $@"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={filePath};Extended Properties=""Excel 12.0 Xml;HDR=YES;IMEX=1;""";
            DataTable dt = new DataTable();

            using (OleDbConnection conn = new OleDbConnection(connString))
            {
                conn.Open();

                // Use the sheet name followed by a $ sign inside square brackets
                string query = $"SELECT * FROM [{sheetName}$]";

                using (OleDbDataAdapter adapter = new OleDbDataAdapter(query, conn))
                {
                    adapter.Fill(dt);
                }
            }
            return dt;
        }
    }

}
