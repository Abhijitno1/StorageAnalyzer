using System;
using System.Collections.Generic;
using System.Linq;
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

    }
}
