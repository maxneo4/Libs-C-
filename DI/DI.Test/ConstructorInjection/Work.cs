using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AnotherProject;

namespace DI.Test.ConstructorInjection
{
    internal class Work : IWork
    {

        private IService _service;

        public Work(IService service)
        {
            _service = service;
            _service.Assigned = true;
        }

        #region Miembros de IWork

        public void PrintServiceStatus()
        {
            System.IO.File.WriteAllText(@"C:\status.txt", _service.GetStatus());
        }

        #endregion

       
    }
}
