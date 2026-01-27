using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace StorageAnalyzerService.DbModels
{
    public class MongoDbRepository
    {
        private readonly IMongoCollection<FolderMapV2> _collectFolderMaps;
        private readonly IMongoCollection<ModakV2> _collectModaks;
        XmlDocument xmlDoc = new XmlDocument();

        public string RootFolderPath { get; set; }


        public MongoDbRepository()
        {
            var client = new MongoClient("mongodb://localhost:27017");
            var database = client.GetDatabase("DirectoryMap");
            _collectModaks = database.GetCollection<ModakV2>("Modaks");
            _collectFolderMaps = database.GetCollection<FolderMapV2>("FolderMaps");
        }

        public List<FolderMapV2> GetAllFolderMaps() =>
            _collectFolderMaps.Find(_ => true).ToList();

        public XmlDocument GetMap()
        {
            string folderMapText = null;
            var folderMap = _collectFolderMaps.Find(naksha => naksha.AbsolutePath == RootFolderPath).FirstOrDefault();
            if (folderMap != null) folderMapText = folderMap.DirectoryXml;
            var retDoc = new XmlDocument();
            retDoc.LoadXml(folderMapText);
            return retDoc;
        }


        public void SaveMap()
        {
            DirectoryInfo rootFolder = new DirectoryInfo(RootFolderPath);
            xmlDoc = new XmlDocument();
            TraverseFolder(rootFolder, null);
            var xml2save = xmlDoc.OuterXml;
            FolderMapV2 folderMap = new FolderMapV2()
            {
                AbsolutePath = RootFolderPath,
                DirectoryXml = xml2save
            };
            _collectFolderMaps.InsertOne(folderMap);
        }

        public void GenerateChildNodeTree(string childFolderPath, XmlNode destXmlNode)
        {
            DirectoryInfo rootFolder = new DirectoryInfo(childFolderPath);
            xmlDoc = destXmlNode.OwnerDocument;
            TraverseFolder(rootFolder, destXmlNode);
            UpdateMap(xmlDoc);
        }


        public void UpdateMap(XmlDocument editedDoc)
        {
            var identifier = editedDoc.DocumentElement.Attributes["fullPath"].Value;
            if (identifier != null)
            {
                var foundRec = _collectFolderMaps.Find(folderMap => folderMap.AbsolutePath == identifier).FirstOrDefault();
                if (foundRec != null)
                {
                    foundRec.DirectoryXml = editedDoc.DocumentElement.OuterXml;
                    _collectFolderMaps.ReplaceOne(x => x.Id == foundRec.Id, foundRec);
                }
            }
        }

        public bool DeleteFolderMap(string absolutePath)
        {
            var foundFolderMap = _collectFolderMaps.Find(k => k.AbsolutePath == absolutePath).FirstOrDefault();
            if (foundFolderMap != null)
            {
                //First remove any stored file dependencies in database
                DeleteModaksForNaksha(absolutePath);
                //Then remove folder map from Xml maps
                var deleteResult = _collectFolderMaps.DeleteOne(k => k.Id == foundFolderMap.Id);
                return deleteResult.DeletedCount > 0;
            }
            return false;
        }

        public bool DeleteModaksForNaksha(string absolutePath)
        {
            var foundFolderMap = _collectFolderMaps.Find(k => k.AbsolutePath == absolutePath).FirstOrDefault();
            if (foundFolderMap != null)
            {
                var xml = foundFolderMap.DirectoryXml;
                var xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(xml);
                DeleteFiles4Node(xmlDoc.DocumentElement);
            }
            return true;
        }

        public bool DeleteFiles4Node(XmlNode node)
        {
            foreach (XmlNode child in node.ChildNodes)
            {
                if (child.Name == "file")
                {
                    var modakId = child.Attributes["DbId"].Value;
                    DeleteModak(modakId);
                }
                else
                {
                    DeleteFiles4Node(child);
                }
            }
            return true;
        }

        private void TraverseFolder(DirectoryInfo currentFolder, XmlNode parentNode)
        {
            var folderNode = xmlDoc.CreateElement("folder");
            folderNode.SetAttribute("name", currentFolder.Name);
            folderNode.SetAttribute("creationDate", currentFolder.CreationTime.ToString("dd-MMM-yyyy"));

            if (parentNode == null)
            {
                folderNode.SetAttribute("fullPath", currentFolder.FullName);
                folderNode.SetAttribute("snapshotDate", DateTime.Now.ToString("dd-MMM-yyyy hh:mm tt"));
            }
            //Console.WriteLine(currentFolder.Name);
            //Console.WriteLine(currentFolder.FullName);
            //Console.WriteLine(currentFolder.CreationTime.ToShortDateString());

            TraverseFiles(currentFolder, folderNode);

            foreach (var childFolder in currentFolder.EnumerateDirectories())
            {
                TraverseFolder(childFolder, folderNode);
            }
            if (parentNode == null)
                xmlDoc.AppendChild(folderNode);    //First Child is the greatest ancestor
            else
                parentNode.AppendChild(folderNode);
        }

        private void TraverseFiles(DirectoryInfo currentfolder, XmlNode parentNode)
        {
            XmlNode fileNode = null;

            foreach (var childFile in currentfolder.EnumerateFiles())
            {
                fileNode = xmlDoc.CreateElement("file");
                var fileElm = fileNode as XmlElement; ;
                fileElm.SetAttribute("name", childFile.Name);
                fileElm.SetAttribute("extension", childFile.Extension);
                fileElm.SetAttribute("creationDate", childFile.CreationTime.ToString("dd-MMM-yyyy"));
                fileElm.SetAttribute("size", childFile.Length.ToString());

                //Console.WriteLine(childFile.Name);
                //Console.WriteLine(childFile.Extension);
                //Console.WriteLine(childFile.CreationTime.ToShortDateString());
                parentNode.AppendChild(fileNode);

                var cutPoint = childFile.FullName.IndexOf(this.RootFolderPath) > -1 ? this.RootFolderPath.Length : 0;
                var relativePath = childFile.FullName.Substring(cutPoint);
                //Also Add to DB
                ModakV2 modak = new ModakV2()
                {
                    Title = childFile.Name,
                    RelativePath = relativePath
                };
                var fs = childFile.OpenRead();
                var fileData = new byte[fs.Length];
                //ToDo: Optiomize this file read in future
                fs.Read(fileData, 0, (int)fs.Length);
                fs.Dispose();
                modak.PicData = fileData;
                UpsertModak(modak);
                //Ref: https://stackoverflow.com/questions/5212751/how-can-i-retrieve-id-of-inserted-entity-using-entity-framework
                fileElm.SetAttribute("DbId", modak.Id.ToString());
            }
        }

        public String UpsertModak(ModakV2 modak)
        {
            try
            {
                if (string.IsNullOrEmpty(modak.Id))
                {
                    modak.Id = MongoDB.Bson.ObjectId.GenerateNewId().ToString();
                }

                var filter = Builders<ModakV2>.Filter.Eq(m => m.Id, modak.Id);

                // 1. Use $set for fields you want to update every time (Patching)
                // 2. Use $setOnInsert for the Data field so it is ONLY written if a new doc is created
                var update = Builders<ModakV2>.Update
                    .Set(m => m.Title, modak.Title) // Example other field
                    .Set(m => m.RelativePath, modak.RelativePath) // Example other field
                    .SetOnInsert(m => m.PicData, modak.PicData); // Omit from future updates

                var options = new UpdateOptions { IsUpsert = true };

                // Change ReplaceOne to UpdateOne for partial updates
                var result = _collectModaks.UpdateOne(filter, update, options);

                if (result.UpsertedId != null)
                {
                    modak.Id = result.UpsertedId.ToString();
                }
                return modak.Id;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error updating media file with ID {modak.Id}: {ex.Message}");
                throw;
            }
        }

        public ModakV2 GetModak(string id)
        {
            try
            {
                return _collectModaks.Find(m => m.Id == id).FirstOrDefault();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error retrieving media file with ID {id}: {ex.Message}");
                throw;
            }
        }

        public Byte[] GetModakData(string id)
        {
            var modak = GetModak(id);
            return modak?.PicData;
        }

        public void DeleteModak(string id)
        {
            //if (!ObjectId.TryParse(id, out var objectId))
            try
            {
                _collectModaks.DeleteOne(x => x.Id == id);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error deleting media file with ID {id}: {ex.Message}");
                throw;
            }
        }
    }
}
