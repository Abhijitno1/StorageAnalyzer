using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace StorageAnalyzerService
{
    public class DirectoryMapSaver
    {
        public string RootFolderPath { get; set; }
        public string OutputFilePathName { get; set; }

        private XmlWriter outWriter;

        public void SaveMap()
        {
            DirectoryInfo rootFolder = new DirectoryInfo(RootFolderPath);
            var writerSettings = new XmlWriterSettings()
            {
                Indent = true,
                IndentChars = "\t",
                NewLineHandling = NewLineHandling.Replace,
                NewLineChars = "\r\n"
            };
            outWriter = XmlWriter.Create(OutputFilePathName, writerSettings);
            outWriter.WriteStartDocument();
            TraverseFolder(rootFolder, true);
            outWriter.WriteEndDocument();
            outWriter.Close();
            outWriter.Dispose();
        }

        private void TraverseFolder(DirectoryInfo currentFolder, bool isRoot)
        {
            outWriter.WriteStartElement("folder");
            outWriter.WriteAttributeString("name", currentFolder.Name);
            outWriter.WriteAttributeString("creationDate", currentFolder.CreationTime.ToString("dd-MMM-yyyy"));
            if (isRoot)
            {
                outWriter.WriteAttributeString("fullPath", currentFolder.FullName);
                outWriter.WriteAttributeString("snapshotDate", DateTime.Now.ToString("dd-MMM-yyyy hh:mm tt"));
            }
            //Console.WriteLine(currentFolder.Name);
            //Console.WriteLine(currentFolder.FullName);
            //Console.WriteLine(currentFolder.CreationTime.ToShortDateString());

            TraverseFiles(currentFolder);

            foreach (var childFolder in currentFolder.EnumerateDirectories())
            {
                TraverseFolder(childFolder, false);
            }
            outWriter.WriteEndElement();
        }

        private void TraverseFiles(DirectoryInfo currentfolder)
        {
            foreach (var childFile in currentfolder.EnumerateFiles())
            {
                outWriter.WriteStartElement("file");
                outWriter.WriteAttributeString("name", childFile.Name);
                outWriter.WriteAttributeString("extension", childFile.Extension);
                outWriter.WriteAttributeString("creationDate", childFile.CreationTime.ToString("dd-MMM-yyyy"));
                outWriter.WriteAttributeString("size", childFile.Length.ToString());
                //Console.WriteLine(childFile.Name);
                //Console.WriteLine(childFile.Extension);
                //Console.WriteLine(childFile.CreationTime.ToShortDateString());
                outWriter.WriteEndElement();
            }
        }
    }
}
