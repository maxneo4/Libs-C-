using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ExternalLib.File;

namespace AnotherProject
{
    internal class ServicioWindows : IService
    {
        internal ServicioWindows(IFileUtil fileUtil)
        {
           string readedText = fileUtil.ReadFile(null);
           Console.WriteLine(readedText);
        }

        #region Miembros de IService

        public string GetStatus()
        {
            return "Success";
        }

        public bool Assigned { get; set; }

        #endregion
    }
}
