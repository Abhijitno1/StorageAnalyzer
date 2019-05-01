using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using System.Threading.Tasks;

namespace StorageAnalyzerService
{
    public class DirectoryMapReader
    {
        public string InputFilePathName { get; set; }

        public List<string> SearchFileByName(string fileName, string orderBy)
        {
            var document = XDocument.Load(InputFilePathName);
            var output = from file in document.Root.Descendants("file")
                         where file.Attribute("name").Value.ToLower().Contains(fileName.Trim().ToLower())
                         select new
                         {
                             FilePath = GetFilePath(file),
                             Extension = file.Attribute("extension").Value,
                             Size = file.Attribute("size").Value,
                             CreationDate = file.Attribute("creationDate")
                         };
            output = output.OrderByFieldName(orderBy);
            return output.Select(file => file.FilePath).ToList(); // + "|" + file.Size + "|" + file.CreationDate).ToList();
        }

        public List<string> SearchFileByExtensions(string[] extensions)
        {
            var document = XDocument.Load(InputFilePathName);
            var output = from file in document.Root.Descendants("file")
                         where extensions.Any(extn => extn.Trim().CompareTo(file.Attribute("extension").Value.ToLower()) == 0)
                         let filePath = GetFilePath(file)
                         orderby filePath
                         select filePath;
            return output.ToList();
        }

        public List<string> SearchFileUsingRegEx(string fileName, out string errorMessage)
        {            
            List<string> output;
            errorMessage = string.Empty;
            try
            {
                var document = XDocument.Load(InputFilePathName);
                output = (from file in document.Root.Descendants("file")
                         where Regex.IsMatch(file.Attribute("name").Value, fileName, RegexOptions.IgnoreCase)
                         select GetFilePath(file)).ToList();
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                output = new List<string>();
            }
            return output;
        }

        public Dictionary<string, List<string>> SearchXactDuplicates()
        {
            var document = XDocument.Load(InputFilePathName);
            var pairs = from file in document.Root.Descendants("file")
                        group file by file.Attribute("name").Value.ToLower() into duplicate
                        where duplicate.Count() > 1
                        select duplicate;
            var result = new Dictionary<string, List<string>>();
            foreach (var dupPair in pairs)
            {
                List<string> duplicate = new List<string>();
                foreach (var value in dupPair)
                {
                    duplicate.Add(GetFilePath(value));
                }
                result.Add(dupPair.Key, duplicate);
            }
            return result;
        }


        private string GetFilePath(XElement currentElm)
        {
            var elmPath= string.Empty;
            var element = currentElm;
            while (element != null)
            {
                if (elmPath == string.Empty)
                    elmPath = element.Attribute("name").Value;
                else
                    elmPath = element.Parent == null ?
                        element.Attribute("fullPath").Value + "\\" + elmPath :
                        element.Attribute("name").Value + "\\" + elmPath;

                element = element.Parent;
            }
            return  elmPath;
        }

        private string GetFullFilePath(XElement currentElm)
        {
            var elmPath = currentElm.Attribute("name").Value;
            var element = currentElm;
            while (element != null)
            {
                if (element.Parent == null)
                {
                    elmPath = element.Attribute("fullPath").Value + "\\" + elmPath;
                }
                else if (element.Parent.Parent != null) //Dont add foldername for immediate child of root node
                {
                    elmPath = element.Parent.Attribute("name").Value + "\\" + elmPath;
                }

                element = element.Parent;
            }
            return elmPath;
        }
    }
}
