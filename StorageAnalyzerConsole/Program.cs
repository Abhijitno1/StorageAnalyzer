using System;
using System.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StorageAnalyzerService;
using System.IO;

namespace StorageAnalyzerConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            CopyCompareResultFilesToDest();
        }

        static void WriteDataFile()
        {
            DirectoryMapSaver traverser = new DirectoryMapSaver();
            traverser.RootFolderPath = ConfigurationManager.AppSettings["rootFolder"];
            traverser.OutputFilePathName = ConfigurationManager.AppSettings["dataFilePathAndName"];
            traverser.SaveMap();
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
