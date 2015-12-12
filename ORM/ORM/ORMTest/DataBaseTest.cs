using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ORM;

namespace ORMTest
{
    [TestClass]
    public class DataBaseTest
    {

        private string _stringConnection;

        [TestInitialize]
        public void Initialize()
        {
            _stringConnection = "Persist Security Info=True;Integrated Security=SSPI;Data Source=DEV-EDWINB\\BIZAGIDATA;Initial Catalog=BizagiVsPro;";
        }

        [TestMethod]
        public void ConnectionTest()
        {
            //Given
            DataBase dataBase = new DataBaseImp(_stringConnection);
            //When
            List<BaCatalog> baCatalogs = dataBase.RunQuery<BaCatalog>("Select * from babizagiCatalog");
        }

        [TestMethod]
        public void UpdateTest()
        {
            //Given
            DataBase dataBase = new DataBaseImp(_stringConnection);
            //When
            dataBase.BeginTransaction();
            dataBase.RunMerge("update babizagiCatalog set objname = 'RELOL' where guidObject = '8DB96F36-6751-4881-9228-0058307BD2EF';");
            dataBase.CommitTransaction();
        }

        [TestMethod]
        public void RollBackTest()
        {
            //Given
            DataBase dataBase = new DataBaseImp(_stringConnection);
            //When
            dataBase.BeginTransaction();
            dataBase.RunMerge("update babizagiCatalog set objname = 'DONT UPDATE' where guidObject = '8DB96F36-6751-4881-9228-0058307BD2EF';");
            dataBase.RollBackTransaction();
            
        }

        class BaCatalog
        {
            public Guid GuidObject { get; set; }
            public string ObjName { get; set; }
            public int ObjType { get; set; }
        }
    }
}
