using StorageAnalyzerService.DbModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using static StorageAnalyzerService.DirectoryMapComparer;

namespace StorageAnalyzerService
{
    public class DBFilesComparer
    {
        public string FirstInputFilePathName { get; set; }

        public string SecondInputFilePathName { get; set; }

        public List<ModakV2> LookupNonMatchingFiles()
        {
            var firstDoc = XDocument.Load(FirstInputFilePathName);
            var secondDoc = XDocument.Load(SecondInputFilePathName);
            var secondDocFiles = secondDoc.Root.Elements("file");
            var myComparer = new FileNameEqualityComparer();
            var nonMatchingfiles = firstDoc.Root.Elements("file").Where(file1 =>
                !secondDocFiles.Contains(file1, myComparer)
            );
            var output = nonMatchingfiles.Select(fileNode => new ModakV2
            {
                Id = fileNode.Attribute("Id").Value,
                Title = fileNode.Attribute("Title").Value,
                RelativePath = fileNode.Attribute("RelativePath").Value
            });
            return output.ToList();
        }

        public void WriteComparisonResults2File(string outputFilePathName)
        {
            var nonMatchingFiles = LookupNonMatchingFiles();
            var writerSettings = new XmlWriterSettings()
            {
                Encoding = Encoding.UTF8,
                Indent = true,
                IndentChars = "\t",
                NewLineHandling = NewLineHandling.Replace,
                NewLineChars = "\r\n"
            };
            using (var outWriter = XmlWriter.Create(outputFilePathName, writerSettings))
            {
                outWriter.WriteStartDocument();
                outWriter.WriteStartElement("ModakV2_NonMatchingFiles");
                foreach (var file in nonMatchingFiles)
                {
                    outWriter.WriteStartElement("file");
                    outWriter.WriteAttributeString("Id", file.Id);
                    outWriter.WriteAttributeString("Title", file.Title);
                    outWriter.WriteAttributeString("RelativePath", file.RelativePath);
                    outWriter.WriteEndElement();
                }
                outWriter.WriteEndElement();
                outWriter.WriteEndDocument();
                outWriter.Close();
            }
        }

        public class FileNameEqualityComparer : IEqualityComparer<XElement>
        {
            public bool Equals(XElement x, XElement y)
            {
                bool result = x.Attribute("Title").ToString() == y.Attribute("Title").ToString();
                return result;
            }
            public int GetHashCode(XElement obj)
            {
                return obj.GetHashCode();
            }
        }   
    }
}
