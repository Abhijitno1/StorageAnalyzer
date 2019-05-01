using System;
using System.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StorageAnalyzerService;

namespace StorageAnalyzerConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            CompareDataFiles();
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
                Console.WriteLine("No matching files found for given search cirteria");
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
            if (result.Any())
            {
                foreach (var entry in result)
                    Console.WriteLine(entry);
            }
            else
            {
                Console.WriteLine("No non-matching files found in compared folders");
            }
        }
    }
}
