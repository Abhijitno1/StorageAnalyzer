using Microsoft.SqlServer.Server;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;
using StorageAnalyzerService.DbModels;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace StorageAnalyzerService
{
    public class ModakV2Migrator
    {
        public async Task HashExistingDataAsync()
        {
            var client = new MongoClient("mongodb://localhost:27017");
            var database = client.GetDatabase("DirectoryMap");
            var bucket = new GridFSBucket(database);

            // Access the underlying collection to perform metadata updates
            var filesCollection = database.GetCollection<BsonDocument>("fs.files");

            using (var cursor = await bucket.FindAsync(Builders<GridFSFileInfo>.Filter.Empty))
            {
                while (await cursor.MoveNextAsync())
                {
                    foreach (var file in cursor.Current)
                    {
                        // Stream the file instead of downloading all bytes at once to save RAM
                        using (var stream = await bucket.OpenDownloadStreamAsync(file.Id))
                        {
                            string calculatedHash = CommonMethods.CalculateSha256(stream);

                            // FIX: Use the specific file's ID in the filter
                            var updFilter = Builders<BsonDocument>.Filter.Eq("_id", file.Id);
                            var update = Builders<BsonDocument>.Update.Set("metadata.DataHash", calculatedHash);

                            await filesCollection.UpdateOneAsync(updFilter, update);
                        }
                    }
                }
            }
        }

        public void SaveMongoDBFilesList(string outputFilePathName)
        {
            var client = new MongoClient("mongodb://localhost:27017");
            var database = client.GetDatabase("DirectoryMap");
            var bucket = new GridFSBucket(database);
            var writerSettings = new XmlWriterSettings()
            {
                Indent = true,
                IndentChars = "\t",
                NewLineHandling = NewLineHandling.Replace,
                NewLineChars = "\r\n"
            };
            using (var outWriter = XmlWriter.Create(outputFilePathName, writerSettings))
            {
                outWriter.WriteStartDocument();
                outWriter.WriteStartElement("FolderMap");
                using (var cursor = bucket.Find(Builders<GridFSFileInfo>.Filter.Empty))
                {
                    while (cursor.MoveNext())
                    {
                        foreach (var file in cursor.Current)
                        {
                            var relativePath = file.Metadata != null && file.Metadata.Contains("RelativePath")
                                   ? file.Metadata["RelativePath"].AsString
                                   : "N/A";
                            var dataHash = file.Metadata != null && file.Metadata.Contains("DataHash")
                               ? file.Metadata["DataHash"].AsString
                               : null;

                            outWriter.WriteStartElement("file");
                            outWriter.WriteAttributeString("Title", file.Filename);
                            outWriter.WriteAttributeString("RelativePath", relativePath);
                            outWriter.WriteAttributeString("Size", file.Length.ToString());
                            outWriter.WriteAttributeString("DataHash", dataHash);
                            //Console.WriteLine(childFile.CreationTime.ToShortDateString());
                            outWriter.WriteEndElement();

                        }
                    }
                }
                outWriter.WriteEndElement();
                outWriter.WriteEndDocument();
                outWriter.Close();
            }
        }

        public void ConvertDirectoryJson2FlatData()
        {
            var client = new MongoClient("mongodb://localhost:27017");
            var database = client.GetDatabase("DirectoryMap");
            var _driveMaps = database.GetCollection<FileSystemElement>("DriveMaps");
            var _collectFolderMaps = database.GetCollection<FolderMapV2>("FolderMaps");
            var folderMaps = _collectFolderMaps.Find(Builders<FolderMapV2>.Filter.Empty).ToList();
            foreach (var folderMap in folderMaps)
            {
				var folderHierarchy = folderMap.DirectoryJson != null ? DbFolderBsonMapper.FromBsonDocument(folderMap.DirectoryJson) : null;
				FlattenFolderHierarchy(folderHierarchy, _driveMaps, null);
			}
        }

        private void FlattenFolderHierarchy(FileSystemItem item, IMongoCollection<FileSystemElement> driveMaps, FileSystemElement parent)
		{
			if (item == null)
				return;

			var fileSystemElement = new FileSystemElement
			{
				ItemName = item.ItemName,
				FullPath = (parent == null)? item.FullPath : Path.Combine(parent.FullPath, item.ItemName),
				Extension = item.Extension,
				CreationDate = item.CreationDate,
				DbId = item.DbId,
                Size = item.Size,
				IsFolder = item.IsFolder,
				parentId = parent?.Id
			};
			driveMaps.InsertOne(fileSystemElement);

			if (item.IsFolder && item.Children != null)
			{
				foreach (var child in item.Children)
				{
					FlattenFolderHierarchy(child, driveMaps, fileSystemElement);
				}
			}
		}

        public void SeparateOutDiskMapRoots()
		{
			var client = new MongoClient("mongodb://localhost:27017");
			var database = client.GetDatabase("DirectoryMap");
			var _driveMaps = database.GetCollection<FileSystemElement>("DriveMaps");
			var mapRoots = _driveMaps.Find(elm => elm.parentId == null).ToList();
			var _driveMapRoots = database.GetCollection<FileSystemParent>("DriveMapRoots");
			foreach (var root in mapRoots)
			{
				var extRoot = new FileSystemParent
				{
                    Id = root.Id,
					ItemName = root.ItemName,
					FullPath = root.FullPath,
					CreationDate = root.CreationDate
				};
				_driveMapRoots.InsertOne(extRoot);

				UpdateRootReferenceInTreeNodes(_driveMaps, root, extRoot.Id);
			}
		}

		private void UpdateRootReferenceInTreeNodes(IMongoCollection<FileSystemElement> _driveMaps, FileSystemElement curNode, string rootId)
		{
			_driveMaps.UpdateOne(x => x.Id == curNode.Id, Builders<FileSystemElement>.Update.Set(elm => elm.rootId, rootId));

			var children = _driveMaps.Find(elm => elm.parentId == curNode.Id).ToList();
			foreach (var child in children)
			{
				UpdateRootReferenceInTreeNodes(_driveMaps, child, rootId);
			}
		}

		public void SetPositionFolderHierarchy()
		{
			var client = new MongoClient("mongodb://localhost:27017");
			var database = client.GetDatabase("DirectoryMap");
			var _driveMaps = database.GetCollection<FileSystemElement>("DriveMaps");
			var mapRoots = _driveMaps.Find(elm => elm.parentId == null).ToList();
            foreach (var root in mapRoots)
			{
                root.Order = 0;
                _driveMaps.UpdateOne(elm => elm.Id == root.Id, Builders<FileSystemElement>.Update.Set(elm => elm.Order, root.Order));
                UpdateChildrenPosition(root.Id, _driveMaps);
			}
		}

        public void UpdateChildrenPosition(string parentId, IMongoCollection<FileSystemElement> driveMaps)
		{
			var children = driveMaps.Find(elm => elm.parentId == parentId).ToList();
			foreach (var child in children)
			{
				child.Order = children.IndexOf(child);
				driveMaps.UpdateOne(elm => elm.Id == child.Id, Builders<FileSystemElement>.Update.Set(elm => elm.Order, child.Order));
				if (child.IsFolder)
				{
					UpdateChildrenPosition(child.Id, driveMaps);
				}
			}
		}

		public void ProcessDuplicatesFromCsv(string dataFilePathName, string mapInputPath, string mapOutputPath)
        {
            MongoDbRepository mongoRepo = new MongoDbRepository();  //Future params: "mongodb://localhost:27017", "DirectoryMap"
                                                                    //mongoRepo.RootFolderPath = mapInputPath;
                                                                    //var xmlDoc = mongoRepo.GetMap();
                                                                    //var xDoc = XDocument.Parse(xmlDoc.OuterXml);
            var xDoc = XDocument.Load(mapInputPath);

            ExcelGenerator excelGen = new ExcelGenerator();
            var duplicates = excelGen.ReadCsvFileToList(dataFilePathName);
            string prevFileHash = null;
            int inner = -1;
            for (int outer = 0; outer < duplicates.Count(); outer++)
            {
                var outerDup = duplicates[outer];
                if (prevFileHash != outerDup.DataHash)
                {
                    prevFileHash = outerDup.DataHash;
                    inner = outer;
                    Console.WriteLine("*** Processing files with hash {0}", outerDup.DataHash);
                }
                else
                {
                    if (inner != outer)
                    {
                        var innerDup = duplicates[inner];
                        // do some processing here
                        //Console.WriteLine("file '{0}' with id {1} -", innerDup.RelativePath, innerDup.Id);
                        //Console.WriteLine("- will be replaced with file '{0}' with id {1}", outerDup.RelativePath, outerDup.Id);
                        ReplaceFileIdInXmlNodes(xDoc, innerDup.Id, outerDup.Id);
                        try
                        {
                            mongoRepo.DeleteModak(innerDup.Id);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("modak with id {0} not found", innerDup.Id);
                            Console.WriteLine(ex.ToString());
                        }
                    }
                }

            }

            xDoc.Save(mapOutputPath);
        }

        public void ListMissingFilesFromDb(string mapInputPath, string mapOutputPath)
        {
            var xDoc = XDocument.Load(mapInputPath);
            var fileNodes = xDoc.Descendants("file");
            var mongoRepo = new MongoDbRepository();
            mongoRepo.RootFolderPath = "";
            var modaks = mongoRepo.GetAllModaks();
            var missingFiles = fileNodes.Where(fileNode => (fileNode.Attribute("DbId") == null)
                || !modaks.Exists(modak => modak.Id == fileNode.Attribute("DbId").Value)).ToList();
            var missingFilepaths = missingFiles.Select(mf => new { Id = mf.Attribute("DbId")?.Value, FilePath = BuildPath(mf) }).ToList();
            var csvFileHeaders = new string[] { "Id", "FilePath" };
            var csvData = missingFilepaths.Select(mf => new object[] { mf.Id, mf.FilePath }).ToArray();
            var xlGen = new ExcelGenerator();
            xlGen.CreateCsvFile(mapOutputPath, csvFileHeaders, csvData);
        }

        public void GetMissingFileDetails(string mapInputPath)
        {
            MongoDbRepository mongoRepo = new MongoDbRepository();
            var folderMapPath = "E:\\TestGround\\DirectoryMap";
			var hierarchyRoot = mongoRepo.GetFolderHierarchy(folderMapPath);
            Func<FileSystemItem, object, bool> childrenSelector = (curItem, criteria) =>
            {
                return !curItem.IsFolder && curItem.DbId == criteria.ToString();
            };
			CSVUtils csvUtils = new CSVUtils();
            var missingDbIdData = csvUtils.GetDataTableFromCSVFile(mapInputPath, true);
            var missingDbIds = csvUtils.ConvertFromDataTable2StringList(missingDbIdData);
            foreach (var dbId in missingDbIds)
            {
                var matchingFiles = hierarchyRoot.Descendants(dbId, childrenSelector).ToList();
				if (matchingFiles.Count > 0)
                {
                    var file = matchingFiles[0];
                    Console.WriteLine($"Missing file with Id: {dbId}, FullPath: {file.FullPath}, Creation Date: {file.CreationDate}");
                }
                else
                {
                    Console.WriteLine($"Missing file with Id: {dbId} not found in folder hierarchy");
				}
			}
        }

        public void ScavageOrphanFiles(string outputDir)
        {
            MongoDbRepository mongoRepo = new MongoDbRepository();
            CSVUtils csvUtils = new CSVUtils(); 
            var mapInputPath = "D:\\Playground\\OrphanDbIds.csv";

			var orphanDbIdData = csvUtils.GetDataTableFromCSVFile(mapInputPath, true);
            var orphanDbIds = csvUtils.ConvertFromDataTable2StringList(orphanDbIdData);
            foreach (var dbId in orphanDbIds)
            {
                var modak = mongoRepo.GetModak(dbId);
                if (modak != null)
                {
					Console.WriteLine($"Orphan file with Id: {dbId}, Title: {modak.Title}, Relative Path: {modak.RelativePath}");
                    var filePathName = Path.Combine(outputDir, $"{modak.Title}");
					File.WriteAllBytes(filePathName, modak.PicData);
					mongoRepo.DeleteModak(dbId);
				}
				else
                {
                    Console.WriteLine($"Orphan file with Id: {dbId} not found in database");
                }
            }
		}

		private string BuildPath(XElement fileNode)
        {
            var path = fileNode.Attribute("name").Value;
            while (fileNode.Parent != null)
            {
                fileNode = fileNode.Parent;
                if (fileNode.Parent != null)
                    path = fileNode.Attribute("name").Value + "\\" + path;
            }

            path = fileNode.Attribute("fullPath").Value + "\\" + path;
            return path;
        }

        private void ReplaceFileIdInXmlNodes(XDocument xDoc, string idOriginal, string idNew)
        {
            xDoc.Descendants("file")
                .Where(fn => fn.Attribute("DbId") != null && fn.Attribute("DbId").Value == idOriginal)
                .ToList()
                .ForEach(fn => fn.SetAttributeValue("DbId", idNew));
        }

        public void SaveSqlDbFilesList(string connectionString, string outputFilePathName)
        {
            var sqlConn = new SqlConnection(connectionString);
            sqlConn.Open();
            var cmd = sqlConn.CreateCommand();
            cmd.CommandText = "Select Id, Title, RelativePath from Modak";
            using (var reader = cmd.ExecuteReader())
            {
                var writerSettings = new XmlWriterSettings()
                {
                    Indent = true,
                    IndentChars = "\t",
                    NewLineHandling = NewLineHandling.Replace,
                    NewLineChars = "\r\n"
                };
                using (var outWriter = XmlWriter.Create(outputFilePathName, writerSettings))
                {
                    outWriter.WriteStartDocument();
                    outWriter.WriteStartElement("DBFiles");

                    while (reader.Read())
                    {
                        outWriter.WriteStartElement("file");
                        outWriter.WriteAttributeString("Id", reader["Id"].ToString());
                        outWriter.WriteAttributeString("Title", reader["Title"].ToString());
                        outWriter.WriteAttributeString("RelativePath", reader["RelativePath"].ToString());
                        outWriter.WriteEndElement(); // file
                    }
                    outWriter.WriteEndElement(); // FolderMap
                    outWriter.WriteEndDocument();
                }
            }

            sqlConn.Close();

        }

        public void CheckMissingSqlDatabaseFiles(string connectionString)
        {
            using (var sqlConn = new SqlConnection(connectionString))
            {
                sqlConn.Open();
                var cmd = sqlConn.CreateCommand();
                cmd.CommandText = "Select DirectoryXml from FolderMap where Id=1";
                var dirXml = cmd.ExecuteScalar() as string;
                var dirDoc = XDocument.Parse(dirXml);
                var fileNodes = dirDoc.Root.Descendants("file");
                foreach (var fileNode in fileNodes)
                {
                    var fileId = fileNode.Attribute("DbId").Value;
                    var checkCmd = sqlConn.CreateCommand();
                    checkCmd.CommandText = "Select Id from Modak where Id=@Id";
                    checkCmd.Parameters.AddWithValue("@Id", fileId);
                    var id = checkCmd.ExecuteScalar();
                    if (id == null)
                    {
                        Console.WriteLine($"Missing file with Id: {fileId}, Title: {fileNode.Attribute("name").Value}, Extn: {fileNode.Attribute("extension").Value}, Creation Date: {fileNode.Attribute("creationDate").Value}");
                    }
                }


                sqlConn.Close();
            }

        }

        public async Task ConvertFolderMapXml2JsonB()
        {
            try
            {
                var repo = new MongoDbRepository();

                var documents = repo.GetAllFolderMaps();
                foreach (var doc in documents)
                {
                    try
                    {
                        // Convert XML to a JSON string using Newtonsoft.Json
                        // Formatting.None keeps the BSON compact
                        FileSystemItem folderHierarchy = DbFolderXmlMapper.FromXmlString(doc.DirectoryXml);
                        repo.UpdateMap(folderHierarchy);

                        Console.WriteLine($"Successfully migrated ID: {doc.Id}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error migrating ID {doc.Id}: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                // Log and swallow the exception so the caller/debugger doesn't abruptly jump to the end of the
                // method/state machine when an unexpected error occurs during the async operation.
                Console.WriteLine($"ConvertFolderMapXml2JsonB failed: {ex}");
                return;
            }

        }

        public void DisplayMissingFilesInMongoDb(string mapInputPath)
        {
            var mongoRepo = new MongoDbRepository();
            mongoRepo.RootFolderPath = mapInputPath;
            var fileNodeDbIds = mongoRepo.GetAllDbIdsFromFolderMap();
            var modaks = mongoRepo.GetAllModaks();
            var modakIds = modaks.Select(m => m.Id);
            var missingFiles = fileNodeDbIds.Except(modakIds);
            var orphanFiles = modakIds.Except(fileNodeDbIds);
			Console.WriteLine("MissingFileIds");
			foreach (var missingFileId in missingFiles)
            {
                Console.WriteLine(missingFileId);
            }
			Console.WriteLine("OrphanFileIds");
			foreach (var orphanFileId in orphanFiles)
			{
				Console.WriteLine(orphanFileId);
			}
		}
	}
}
