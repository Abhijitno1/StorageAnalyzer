using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Core.Configuration;
using MongoDB.Driver.GridFS;
using StorageAnalyzerService.DbModels;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Contexts;
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

    }
}
