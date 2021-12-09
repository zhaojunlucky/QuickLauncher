using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickLauncher
{
    class FileUtil
    {
        public static string getDirectoryOfFile(string path)
        {
            int i;
            for (i = path.Length - 1; i > 0; i--)
            {
                if (path[i] == '/' || path[i] == '\\')
                {
                    return path.Substring(0, i);
                }
            }
            return path;
        }

        public static string getFileNameNoExt(string filePath)
        {

            FileInfo fInfo = new FileInfo(filePath);
            var fileName = fInfo.Name;

            if (fileName.Contains("."))
            {
                return fileName.Substring(0, fileName.IndexOf("."));
            }
            return fileName;
        }
    }
}
