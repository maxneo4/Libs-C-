using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ionic.Zip;

namespace ExternalLib.File
{
    public class FileUtil : IFileUtil
    {
        public FileUtil(ZipFile zipFile)
        {
            zipFile.AddDirectory(@"C:\Users\MAX\Documents\My Publications\THE POWER OF DREAMS"); // AddDirectory recurses subdirectories
            zipFile.Save();
        }

        public string ReadFile(string path)
        {
            return "Not Referenced Assembly";
        }
    }
}
