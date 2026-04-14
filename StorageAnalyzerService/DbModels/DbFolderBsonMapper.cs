using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;

namespace StorageAnalyzerService.DbModels
{
    public static class DbFolderBsonMapper
    {
		public static FileSystemItem FromBsonDocument(BsonDocument doc, string parentPath = null)
		{
			FileSystemItem currentItem = null;
			if (doc == null) return null;
			if (doc.GetValue("IsFolder", false).AsBoolean)
			{
                currentItem = new FileSystemItem()
                {
                    IsFolder = true,
                    ItemName = doc.GetValue("Name", BsonNull.Value).AsString,
                    FullPath = string.IsNullOrEmpty(parentPath) ? GetString(doc, "FullPath") : parentPath + "\\" + doc.GetValue("Name", BsonNull.Value).AsString,
                    CreationDate = GetDateTime(doc, "CreationDate"),
                    Children = new List<FileSystemItem>()
                };

				if (doc.Contains("Children") && doc["Children"].IsBsonArray)
				{
					foreach (var child in doc["Children"].AsBsonArray)
					{
						if (child.IsBsonDocument)
							currentItem.Children.Add(FromBsonDocument(child.AsBsonDocument, currentItem.FullPath));
					}
				}
			}
			else
			{
				currentItem = new FileSystemItem()
				{
					IsFolder = false,
					ItemName = doc.GetValue("Name", BsonNull.Value).AsString,
                    FullPath = string.IsNullOrEmpty(parentPath) ? GetString(doc, "FullPath") : parentPath + "\\" + doc.GetValue("Name", BsonNull.Value).AsString,
					Extension = doc.GetValue("Extension", BsonNull.Value).AsString,
					Size = GetLong(doc, "Size"),
					CreationDate = GetDateTime(doc, "CreationDate"),
					DbId = doc.GetValue("DbId", BsonNull.Value).AsString
				};
			}

			return currentItem;
		}

		private static DateTime GetDateTime(BsonDocument doc, string field)
        {
            if (!doc.Contains(field)) return DateTime.MinValue;
            var val = doc[field];
            try
            {
                if (val.IsBsonDateTime) return val.ToUniversalTime();
                if (val.IsBsonDocument && val.AsBsonDocument.Contains("$date"))
                {
                    var d = val.AsBsonDocument["$date"];
                    if (d.IsBsonDateTime) return d.ToUniversalTime();
                    if (d.IsString && DateTime.TryParse(d.AsString, out var dt)) return dt;
                    if (d.IsBsonDocument && d.AsBsonDocument.Contains("$numberLong") && long.TryParse(d.AsBsonDocument["$numberLong"].AsString, out var ms))
                    {
                        return DateTimeOffset.FromUnixTimeMilliseconds(ms).UtcDateTime;
                    }
                }
                if (val.IsString && DateTime.TryParse(val.AsString, out var parsed)) return parsed;
            }
            catch { }
            return DateTime.MinValue;
        }

        private static long GetLong(BsonDocument doc, string field)
        {
            if (!doc.Contains(field)) return 0;
            var v = doc[field];
            try
            {
                if (v.IsInt32) return v.AsInt32;
                if (v.IsInt64) return v.AsInt64;
                if (v.IsBsonDocument && v.AsBsonDocument.Contains("$numberLong") && long.TryParse(v.AsBsonDocument["$numberLong"].AsString, out var ln)) return ln;
                if (v.IsString && long.TryParse(v.AsString, out var p)) return p;
            }
            catch { }
            return 0;
        }

        private static string GetString(BsonDocument doc, string field)
        {
            if (doc == null) return string.Empty;
            if (!doc.Contains(field)) return string.Empty;
            var v = doc[field];
            if (v == null || v.IsBsonNull) return string.Empty;
            if (v.IsString) return v.AsString;
            return v.ToString() ?? string.Empty;
        }
    }
}
