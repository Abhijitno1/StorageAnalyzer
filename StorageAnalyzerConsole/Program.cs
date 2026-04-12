using StorageAnalyzerService;
using StorageAnalyzerService.DbModels;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StorageAnalyzerConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            string dataFilePathName = ConfigurationManager.AppSettings["dataFilePathAndName"];
            string mapInputPath = ConfigurationManager.AppSettings["inputFilePathAndName"];
            string mapOutputPath = ConfigurationManager.AppSettings["outputFilePathAndName"];
            var migrator = new ModakV2Migrator();
			migrator.ListMissingFilesFromDb(mapInputPath, mapOutputPath);
        }

        static void MigrationTask()
        {
            ModakV2Migrator mercator = new ModakV2Migrator();
            mercator.CheckMissingSqlDatabaseFiles(ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString);
        }

        static void WriteExcelFile()
        {
            var columns = new Tuple<string, ExcelGenerator.ExcelDataTypes>[]
            {
                new Tuple<string, ExcelGenerator.ExcelDataTypes>("FilePath", ExcelGenerator.ExcelDataTypes.TEXT),
                new Tuple<string, ExcelGenerator.ExcelDataTypes>("FileSizeInBytes", ExcelGenerator.ExcelDataTypes.INT),
                new Tuple<string, ExcelGenerator.ExcelDataTypes>("LastModified", ExcelGenerator.ExcelDataTypes.DATETIME)
            };
            ExcelGenerator excelGen = new ExcelGenerator();
            var inputFilePathName = ConfigurationManager.AppSettings["excelFilePathAndName"];
            excelGen.CreateExcelFileWithSchema(inputFilePathName, "Output", columns);

        }

        static void WriteDataFile()
        {
            DirectoryMapSaver traverser = new DirectoryMapSaver();
            traverser.RootFolderPath = ConfigurationManager.AppSettings["rootFolder"];
            traverser.OutputFilePathName = ConfigurationManager.AppSettings["dataFilePathAndName"];
            traverser.SaveMap();
        }

        static void WriteDuplicatesFile()
        {
            MongoDbRepository mongoRepo = new MongoDbRepository();
            var duplicates = mongoRepo.GetDuplicateModaksList();
            ExcelGenerator excelGen = new ExcelGenerator();
            string[] headers = new string[] { "Id", "Title", "RelativePath", "DataHash" };
            object[][] data = duplicates.Select(dup => new object[] { dup.Id, dup.Title, dup.RelativePath, dup.DataHash }).ToArray();
            var outputFilePathName = ConfigurationManager.AppSettings["outputFilePathAndName"];
            excelGen.CreateCsvFile(outputFilePathName, headers, data);
        }

        static void ReadDataFile()
        {
            DirectoryMapReader searcher = new DirectoryMapReader();
            searcher.InputFilePathName = ConfigurationManager.AppSettings["dataFilePathAndName"];
            Console.Write("Enter a file name to search: ");
            string what2Search = Console.ReadLine();
            var result = searcher.SearchFileByName(what2Search, "FilePath");
            if (result.Any())
            {
                foreach (var entry in result)
                    Console.WriteLine(entry);
            }
            else
            {
                Console.WriteLine("No matching files found for given search criteria");
            }
            Console.WriteLine("Searching for duplicates now");
            var result2 = searcher.SearchXactDuplicates();
            if (result2.Any())
            {
                var iterator = result2.GetEnumerator();
                while (iterator.MoveNext())
                {
                    Console.WriteLine("Duplicate group {0}:", iterator.Current.Key);
                    iterator.Current.Value.ForEach(file => Console.WriteLine(file));
                }
            }
            else
            {
                Console.WriteLine("No duplicates found");
            }
        }

        static void CompareDataFiles()
        {
            DirectoryMapComparer dirComparer = new DirectoryMapComparer()
            {
                FirstInputFilePathName = ConfigurationManager.AppSettings["dataFilePathAndName"],
                SecondInputFilePathName = ConfigurationManager.AppSettings["compareFilePathAndName"]
            };
            var result = dirComparer.LookupNonMatchingFiles();
            var resultFilePathName = ConfigurationManager.AppSettings["compareResultsFilePathAndName"];
            var resultFile = File.CreateText(resultFilePathName);

            if (result.Any())
            {
                foreach (var entry in result)
                {
                    Console.WriteLine(entry);
                    resultFile.WriteLine(entry);
                }
            }
            else
            {
                Console.WriteLine("No non-matching files found in compared folders");
                resultFile.WriteLine("No non-matching files found in compared folders");
            }
            resultFile.Close();
            resultFile.Dispose();
        }

        static void SyncUpNonMatchingFiles()
        {
            var rootFolder = ConfigurationManager.AppSettings["rootFolder"];
            var syncDestRootFolder = ConfigurationManager.AppSettings["syncDestRootFolder"];
            DirectoryMapComparer dirComparer = new DirectoryMapComparer()
            {
                FirstInputFilePathName = ConfigurationManager.AppSettings["dataFilePathAndName"],
                SecondInputFilePathName = ConfigurationManager.AppSettings["compareFilePathAndName"]
            };
            var result = dirComparer.LookupNonMatchingFiles();
            if (result.Any())
            {
                foreach (var entry in result)
                {
                    var destFilePath = entry.Replace(rootFolder, syncDestRootFolder);
                    Console.WriteLine(destFilePath);
                    var destDir = Path.GetDirectoryName(destFilePath);
                    if (!Directory.Exists(destDir))
                        Directory.CreateDirectory(destDir);
                    File.Copy(entry, destFilePath, true);
                }
            }
            else
            {
                Console.WriteLine("No non-matching files found in compared folders");
            }
        }

        static void CopyCompareResultFilesToDest()
        {
            var rootFolder = ConfigurationManager.AppSettings["rootFolder"];
            var resultFilePathName = ConfigurationManager.AppSettings["compareResultsFilePathAndName"];
            var resultFileReader = File.OpenText(resultFilePathName);
            var syncDestRootFolder = ConfigurationManager.AppSettings["syncDestRootFolder"];

            while (!resultFileReader.EndOfStream)
            {
                var entry = resultFileReader.ReadLine();
                var destFilePath = entry.Replace(rootFolder, syncDestRootFolder);
                Console.WriteLine(destFilePath);
                var destDir = Path.GetDirectoryName(destFilePath);
                if (!Directory.Exists(destDir))
                    Directory.CreateDirectory(destDir);
                File.Copy(entry, destFilePath, true);
            }
        }

    }
}

