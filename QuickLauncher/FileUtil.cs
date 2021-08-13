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
            FileInfo fInfo = new FileInfo(path);
            var isDir = (fInfo.Attributes&FileAttributes.Directory) == FileAttributes.Directory;
            return isDir ? path : fInfo.Directory.FullName;
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

        public static int findLastSep(string path)
        {
            for (int i = path.Length - 1; i >= 0; i--)
            {
                if (path[i] == '/' || path[i] == '\\')
                {
                    return i;
                }
            }
            return -1;
        }

        public static string getParentDir(string path)
        {
            return path.Substring(0, findLastSep(path));
        }
    }
}
