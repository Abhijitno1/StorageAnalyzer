using StorageAnalyzerService.DbModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace StorageAnalyzerService
{
    internal static class CommonMethods
    {
        public static string GetFilePath(XElement currentElm)
        {
            var elmPath = string.Empty;
            var element = currentElm;
            while (element != null)
            {
                if (elmPath == string.Empty)
                    elmPath = element.Attribute("name").Value;
                else
                    elmPath = element.Parent == null ?
                        element.Attribute("fullPath").Value + "\\" + elmPath :
                        element.Attribute("name").Value + "\\" + elmPath;

                element = element.Parent;
            }
            return elmPath;
        }
        public static string CalculateSha256(Byte[] buffer)
        {
            using (var sha256 = SHA256.Create())
            {
                byte[] hashBytes = sha256.ComputeHash(buffer);
                return ToHexString(hashBytes).ToLowerInvariant();
            }
        }

        public static string CalculateSha256(Stream stream)
        {
            //Late wisdom: return Convert.ToBase64String(System.Security.Cryptography.MD5.Create().ComputeHash(stream))

            using (var sha256 = SHA256.Create())
            {
                byte[] hashBytes = sha256.ComputeHash(stream);
                return ToHexString(hashBytes).ToLowerInvariant();
            }
        }

        private static string ToHexString(byte[] bytes)
        {
            // BitConverter.ToString creates "XX-XX-XX"
            // Replace("-", "") removes the hyphens
            // ToLower() ensures it matches your ModakV2 style
            return BitConverter.ToString(bytes).Replace("-", "").ToLower();
        }

		public static IEnumerable<FileSystemItem> Descendants(this FileSystemItem root, object criteria, Func<FileSystemItem, object, bool> matchSelector)
		{
			var isMatch = matchSelector(root, criteria);
			foreach (var child in root.Children)
			{
                var descendants = Descendants(child, criteria, matchSelector);
				foreach (var descendant in descendants)
				{
					yield return descendant;
				}
			}
            if (isMatch)
                yield return root;
		}
	}
}
