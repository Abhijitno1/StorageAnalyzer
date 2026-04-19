using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using static System.Net.WebRequestMethods;

namespace StorageAnalyzerService.DbModels
{
    public class MongoDbRepository
    {
        private readonly IMongoCollection<FolderMapV2> _collectFolderMaps;
        private readonly IMongoCollection<ModakV2> _collectModaks;
        private readonly GridFSBucket _bucket;
        XmlDocument xmlDoc = new XmlDocument();

        public string RootFolderPath { get; set; }


        public MongoDbRepository()
        {
            var client = new MongoClient("mongodb://localhost:27017");
            var database = client.GetDatabase("DirectoryMap");
            _collectModaks = database.GetCollection<ModakV2>("Modaks");
            _collectFolderMaps = database.GetCollection<FolderMapV2>("FolderMaps");
            _bucket = new GridFSBucket(database);
        }

        public List<FolderMapV2> GetAllFolderMaps() =>
            _collectFolderMaps.Find(_ => true).ToList();

        /// <summary>
        /// Returns the hierarchical DbFolder for the FolderMap matching the absolute path.
        /// If DirectoryJson is present it will be mapped; otherwise DirectoryXml will be used as a fallback.
        /// </summary>
        public FileSystemItem GetFolderHierarchy(string absolutePath)
        {
            var found = _collectFolderMaps.Find(folderMap => folderMap.AbsolutePath == absolutePath).FirstOrDefault();
            if (found == null) return null;
            if (found.DirectoryJson != null)
            {
                return DbFolderBsonMapper.FromBsonDocument(found.DirectoryJson);
            }
            if (!string.IsNullOrEmpty(found.DirectoryXml))
            {
                return DbFolderXmlMapper.FromXmlString(found.DirectoryXml);
            }
            return null;
        }

        /// <summary>
        /// Returns all FolderMaps together with their DbFolder hierarchy (if available).
        /// </summary>
        public List<Tuple<FolderMapV2, FileSystemItem>> GetAllFolderHierarchies()
        {
            var result = new List<Tuple<FolderMapV2, FileSystemItem>>();
            var maps = GetAllFolderMaps();
            foreach (var m in maps)
            {
                FileSystemItem root = null;
                if (m.DirectoryJson != null)
                    root = DbFolderBsonMapper.FromBsonDocument(m.DirectoryJson);
                else if (!string.IsNullOrEmpty(m.DirectoryXml))
                    root = DbFolderXmlMapper.FromXmlString(m.DirectoryXml);
                result.Add(Tuple.Create(m, root));
            }
            return result;
        }

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
                    // Convert XML to JSON and then to BsonDocument for storage in DirectoryJson
                    // This ensures the repository stores the JSON representation and removes the XML
                    try
                    {
                        string jsonString = Newtonsoft.Json.JsonConvert.SerializeXmlNode(editedDoc, Newtonsoft.Json.Formatting.None, true);
                        var bsonData = BsonDocument.Parse(jsonString);
                        foundRec.DirectoryJson = bsonData;
                        // Remove the legacy XML payload as requested
                        foundRec.DirectoryXml = null;
                        _collectFolderMaps.ReplaceOne(x => x.Id == foundRec.Id, foundRec);
                    }
                    catch (Exception)
                    {
                        // Fallback: if conversion fails, preserve the XML to avoid data loss
                        foundRec.DirectoryXml = editedDoc.DocumentElement.OuterXml;
                        _collectFolderMaps.ReplaceOne(x => x.Id == foundRec.Id, foundRec);
                    }
                }
            }
        }

        public void UpdateMap(FileSystemItem folderHierarchy)
        {
            var identifier = folderHierarchy.FullPath;
            if (identifier != null)
            {
                BsonDocument bsonData = folderHierarchy.ToBsonDocument();
                var foundRec = _collectFolderMaps.Find(folderMap => folderMap.AbsolutePath == identifier).FirstOrDefault();
                if (foundRec != null)
                {
                    foundRec.DirectoryJson = bsonData;
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
                try
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
                    InsertModak(modak);
                    //Ref: https://stackoverflow.com/questions/5212751/how-can-i-retrieve-id-of-inserted-entity-using-entity-framework
                    fileElm.SetAttribute("DbId", modak.Id.ToString());
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("File db insert error: " + ex.Message);
                }
            }
        }

        public String UpsertModak11(ModakV2 modak)
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

        // INSERT with logicalFilePath in Metadata
        public ObjectId InsertModak(ModakV2 modak)
        {
            //First compute DataHash
            modak.DataHash = CommonMethods.CalculateSha256(modak.PicData);

            var options = new GridFSUploadOptions
            {
                // Store extra fields here
                Metadata = new BsonDocument {
                    { "RelativePath", modak.RelativePath },
                    { "DataHash", modak.DataHash }
                }
            };

            using (var stream = new MemoryStream(modak.PicData))
            {
                var insertedId = _bucket.UploadFromStream(modak.Title, stream, options);
                modak.Id = insertedId.ToString();
                return insertedId;
            }
        }

        public void UpdateModak(ModakV2 modak)
        {
            var fileId = new ObjectId(modak.Id);

            // Update the Metadata field specifically
            var filter = Builders<GridFSFileInfo>.Filter.Eq("_id", fileId);
            var update = Builders<GridFSFileInfo>.Update
                .Set("metadata.RelativePath", modak.RelativePath)
                .Set("filename", modak.Title); // This is what _bucket.Rename does

            // Use the files collection directly for metadata updates
            var filesCollection = _bucket.Database.GetCollection<GridFSFileInfo>("fs.files");
            filesCollection.UpdateOne(filter, update);
        }

        public ModakV2 GetModak(string id)
        {
            var fileId = new ObjectId(id);

            // 1. Find the file metadata using a filter
            var filter = Builders<GridFSFileInfo>.Filter.Eq("_id", fileId);
            var fileInfo = _bucket.Find(filter).FirstOrDefault();

            if (fileInfo == null)
            {
                return null;
            }

            // 2. Extract metadata and filename
            // Accessing the custom "LogicalFilePath" field we stored earlier
            var modak = new ModakV2
            {
                Id = fileInfo.Id.ToString(),
                Title = fileInfo.Filename,
                RelativePath = fileInfo.Metadata.Contains("RelativePath")
                               ? fileInfo.Metadata["RelativePath"].AsString
                               : string.Empty,
                DataHash = fileInfo.Metadata.Contains("DataHash")
                               ? fileInfo.Metadata["DataHash"].AsString
                               : string.Empty,
                PicData = _bucket.DownloadAsBytes(fileId)
            };

            return modak;
        }

        public ModakV2 GetModakOld(string id)
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
            var fileId = new ObjectId(id);
            _bucket.Delete(fileId);
        }

        public void DeleteModakOld(string id)
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


        public List<ModakV2> GetAllModaks(bool includeFileData = false)
        {
            // 1. Find all files in the bucket
            var filter = Builders<GridFSFileInfo>.Filter.Empty;
            var files = _bucket.Find(filter).ToList();
            var modakList = new List<ModakV2>();

            foreach (var file in files)
            {
                // 2. Retrieve metadata and basic info
                var relativePath = file.Metadata != null && file.Metadata.Contains("RelativePath")
                                   ? file.Metadata["RelativePath"].AsString
                                   : "N/A";
                // 2. Retrieve metadata and basic info
                var dataHash = file.Metadata != null && file.Metadata.Contains("DataHash")
                                   ? file.Metadata["DataHash"].AsString
                                   : null;

                // Handle the Hash (MD5)
                // Older servers stored this in the 'md5' property.Not useful for newer servers.
                //string hash = file.BackingDocument.Contains("md5")
                //              ? file.BackingDocument["md5"].ToString()
                //              : "Hash_Not_Stored";

                var chocolate = new ModakV2
                {
                    Id = file.Id.ToString(),
                    Title = file.Filename,
                    RelativePath = relativePath,
                    DataHash = dataHash
                };
                if (includeFileData)
                {
                    chocolate.PicData = _bucket.DownloadAsBytes(file.Id); // Exclude file data if not requested
                }

                modakList.Add(chocolate);
            }

            return modakList;
        }

        public List<ModakV2> GetDuplicateModaksList()
        {
            List<ModakV2> output = new List<ModakV2>();
            // 1. Find all files in the bucket
            var filter = Builders<GridFSFileInfo>.Filter.Empty;
            var files = _bucket.Find(filter).ToList();
            // 2. Group by DataHash and find duplicates
            var duplicates = files
                .Where(f => f.Metadata != null && f.Metadata.Contains("DataHash"))
                .GroupBy(f => f.Metadata["DataHash"].AsString)
                .Where(g => g.Count() > 1)
                .Select(g => new
                {
                    DataHash = g.Key,
                    Count = g.Count(),
                    Files = g.Select(f =>
                    new {
                        f.Filename,
                        Id = f.Id.ToString(),
                        RelativePath = f.Metadata.Contains("RelativePath") ? f.Metadata["RelativePath"].AsString : null
                    }).ToList()
                })
                .ToList();
            // Output duplicates
            foreach (var group in duplicates)
            {
                foreach (var file in group.Files)
                {
                    output.Add(new ModakV2
                    {
                        Id = file.Id,
                        Title = file.Filename,
                        RelativePath = file.RelativePath,
                        DataHash = group.DataHash
                    });
                }
            }
            return output;
        }

        public List<string> GetAllDbIdsFromFolderMap()
        {
            var output = new List<string>();
            var foundFolderMap = _collectFolderMaps.Find(k => k.AbsolutePath == RootFolderPath).FirstOrDefault();
            if (foundFolderMap != null)
            {
                GetAllDbIdsFromFolderMap(foundFolderMap.DirectoryJson, output);
			}
            return output;
        }

		public void GetAllDbIdsFromFolderMap(BsonDocument doc, List<string> output)
        {
			// If this node is a folder, traverse children
			if (doc.GetValue("IsFolder", false).AsBoolean)
			{
				if (doc.Contains("Children") && doc["Children"].IsBsonArray)
				{
					foreach (var child in doc["Children"].AsBsonArray)
					{
						if (child.IsBsonDocument)
						{
							GetAllDbIdsFromFolderMap(child.AsBsonDocument, output);
						}
					}
				}
			}
			else
			{
				string dbId = doc.GetValue("DbId", BsonNull.Value).AsString;
                output.Add(dbId);
			}
		}
	}
}
