using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using System.Threading.Tasks;

namespace StorageAnalyzerService
{
    public class DirectoryMapComparer
    {
        public string FirstInputFilePathName { get; set; }

        public string SecondInputFilePathName { get; set; }

        public List<string> LookupNonMatchingFiles()
        {
            List<string> output = new List<string>();
            var firstDoc = XDocument.Load(FirstInputFilePathName);
            var secondDoc = XDocument.Load(SecondInputFilePathName);
            var secondDocFiles = secondDoc.Root.Descendants("file");
            var nonMatchingfiles = firstDoc.Root.Descendants("file").Where(file1 => 
                !secondDocFiles.Contains(file1, new FileNameEqualityComparer())
            );
            output = nonMatchingfiles.Select(fileNode=> CommonMethods.GetFilePath(fileNode)).ToList();
            return output;
        }

        public List<string> LookupAddedDeletedFiles()
        {
            List<string> output = new List<string>();
            var firstDoc = XDocument.Load(FirstInputFilePathName);
            var secondDoc = XDocument.Load(SecondInputFilePathName);

            var matchingFiles = from file1 in firstDoc.Root.Descendants("file")
                                //from file2 in secondDoc.Root.Descendants("file")
                                join file2 in secondDoc.Root.Descendants("file")
                                on ((string)file1.Attribute("name")).ToLower() equals ((string)file2.Attribute("name")).ToLower()
                                //where file1.Attribute("name").Value.ToLower() != file2.Attribute("name").Value.ToLower()
                                select file1;

            var firstDocFiles = firstDoc.Root.Descendants("file");
            output = firstDocFiles.Except(matchingFiles, new FileNameEqualityComparer()).Select(fileNode => CommonMethods.GetFilePath(fileNode)).ToList();
            return output;
        }

        public class FileNameEqualityComparer : IEqualityComparer<XElement>
        {
            public bool Equals(XElement x, XElement y)
            {
                bool result = x.Attribute("name").ToString() == y.Attribute("name").ToString();
                return result;
            }
            public int GetHashCode(XElement obj)
            {
                return obj.GetHashCode();
            }
        } 

    }
}
