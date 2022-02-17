using System;
using System.IO;

namespace QuickLauncher
{
    class FileUtil
    {
        public static string GetDirectoryOfFile(string path)
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

        public static string GetFileNameNoExt(string filePath)
        {

            FileInfo fInfo = new FileInfo(filePath);
            var fileName = fInfo.Name;

            if (fileName.Contains("."))
            {
                return fileName.Substring(0, fileName.IndexOf(".", StringComparison.CurrentCulture));
            }
            return fileName;
        }
    }
}
