using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;
using StorageAnalyzerService.DbModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace StorageAnalyzerService
{
    public class ModakV2Migrator
    {
        public async Task MigrateExistingDataAsync()
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
                            string calculatedHash = CalculateSha256Async(stream);

                            // FIX: Use the specific file's ID in the filter
                            var updFilter = Builders<BsonDocument>.Filter.Eq("_id", file.Id);
                            var update = Builders<BsonDocument>.Update.Set("metadata.DataHash", calculatedHash);

                            await filesCollection.UpdateOneAsync(updFilter, update);
                        }
                    }
                }
            }
        }

        private string CalculateSha256Async(Stream stream)
        {
            using (var sha256 = SHA256.Create())
            {
                byte[] hashBytes = sha256.ComputeHash(stream);
                return ToHexString(hashBytes).ToLowerInvariant();
            }
        }

        private string ToHexString(byte[] bytes)
        {
            // BitConverter.ToString creates "XX-XX-XX"
            // Replace("-", "") removes the hyphens
            // ToLower() ensures it matches your ModakV2 style
            return BitConverter.ToString(bytes).Replace("-", "").ToLower();
        }

        public void SaveDBFilesList(string outputFilePathName)
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

    }
}
