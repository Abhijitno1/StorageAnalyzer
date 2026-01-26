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

        public async Task<List<FolderMapV2>> GetAllFolderMapsAsync() =>
            await _collectFolderMaps.Find(_ => true).ToListAsync();

        public async Task<XmlDocument> GetMap()
        {
            string folderMapText = null;
            var folderMap = await _collectFolderMaps.Find(naksha => naksha.AbsolutePath == RootFolderPath).FirstOrDefaultAsync();
            if (folderMap !=null) folderMapText = folderMap.DirectoryXml;
            var retDoc = new XmlDocument();
            retDoc.LoadXml(folderMapText);
            return retDoc;
        }


        public async Task SaveMap()
        {
            DirectoryInfo rootFolder = new DirectoryInfo(RootFolderPath);
            xmlDoc = new XmlDocument();
            await TraverseFolder(rootFolder, null);
            var xml2save = xmlDoc.OuterXml;
            FolderMapV2 folderMap = new FolderMapV2()
            {
                AbsolutePath = RootFolderPath,
                DirectoryXml = xml2save
            };
            await _collectFolderMaps.InsertOneAsync(folderMap);
        }

        public async Task UpdateMap(XmlDocument editedDoc)
        {
            using (ApplicationDbContext dbContext = new ApplicationDbContext())
            {
                var identifier = editedDoc.DocumentElement.Attributes["fullPath"].Value;
                if (identifier != null)
                {
                    var foundRec = await _collectFolderMaps.Find(folderMap => folderMap.AbsolutePath == identifier).FirstOrDefaultAsync();
                    if (foundRec != null)
                    {
                        foundRec.DirectoryXml = editedDoc.DocumentElement.OuterXml;
                        await _collectFolderMaps.ReplaceOneAsync(x => x.Id == foundRec.Id, foundRec);
                    }
                }
            }
        }


        private async Task TraverseFolder(DirectoryInfo currentFolder, XmlNode parentNode)
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

            await TraverseFiles(currentFolder, folderNode);

            foreach (var childFolder in currentFolder.EnumerateDirectories())
            {
                await TraverseFolder(childFolder, folderNode);
            }
            if (parentNode == null)
                xmlDoc.AppendChild(folderNode);    //First Child is the greatest ancestor
            else
                parentNode.AppendChild(folderNode);
        }

        private async Task TraverseFiles(DirectoryInfo currentfolder, XmlNode parentNode)
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
                await UpsertModakAsync(modak);
                //Ref: https://stackoverflow.com/questions/5212751/how-can-i-retrieve-id-of-inserted-entity-using-entity-framework
                fileElm.SetAttribute("DbId", modak.Id.ToString());
            }
        }



        public async Task<String> UpsertModakAsync(ModakV2 modak)
        {
            try
            {
                // Ensure the object has an ID before creating the filter
                if (string.IsNullOrEmpty(modak.Id))
                {
                    // Manually generate a new ObjectId string if you want to be safe
                    modak.Id = MongoDB.Bson.ObjectId.GenerateNewId().ToString();
                }
                
                // Filter to find the existing document by its ID
                var filter = Builders<ModakV2>.Filter.Eq(m => m.Id, modak.Id);

                // Replace the document if found; otherwise, insert it
                var options = new ReplaceOptions { IsUpsert = true };

                var result = await _collectModaks.ReplaceOneAsync(filter, modak, options);
                // 1. If we inserted a new doc, return the new ID generated by Mongo
                if (result.UpsertedId != null)
                {
                    return result.UpsertedId.ToString();
                }
                return modak.Id;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error inserting/updating media file with ID {modak.Id}: {ex.Message}");
                throw;
            }
        }

        public async Task<ModakV2> GetModakAsync(string id)
        {
            try
            {
                return await _collectModaks.Find(m => m.Id == id).FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error retrieving media file with ID {id}: {ex.Message}");
                throw;
            }
        }

        public async Task<Byte[]> GetModakDataAsync(string id)
        {
            var modak = await GetModakAsync(id);
            return modak?.PicData;
        }

        public async Task DeleteModakAsync(string id)
        {
            //if (!ObjectId.TryParse(id, out var objectId))
            try
            {
                await _collectModaks.DeleteOneAsync(x => x.Id == id);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error deleting media file with ID {id}: {ex.Message}");
                throw;
            }
        }
    }
}
