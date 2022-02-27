using CheckEnvironment;
using NUnit.Framework;

namespace CheckEnvironmentTests
{
    public class Tests
    {
        [SetUp]
        public void Setup()
        {
            //Given
            Checker.CleanAllEvents();
        }

        [Test]
        public void TestStoreOneEvent()
        {
            //When
            Checker.Enabled = true;
            Checker.RegisterEvent("default", "llego aqui", "CheckerTest.TestStoreEvent");
            //Then
            var result = Checker.GetEventsByCategory("default");
            Assert.IsTrue(result.Count == 1);
            var ev = result[0];
            Assert.AreEqual("llego aqui", ev.Value);
            Assert.AreEqual("CheckerTest.TestStoreEvent", ev.Source);
        }

        [Test]
        public void TestQueryNotCategoryResult()
        {
            //When
            Checker.Enabled = true;
            Checker.RegisterEvent("default", "llego aqui", "CheckerTest.TestStoreEvent");
            //Then
            var result = Checker.GetEventsByCategory("cat");
            Assert.IsTrue(result.Count == 0);           
        }

        [Test]
        public void TestQueryNullCategory()
        {
            //When
            Checker.Enabled = true;
            Checker.RegisterEvent("default", "llego aqui", "CheckerTest.TestStoreEvent");
            //Then
            var result = Checker.GetEventsByCategory(null);
            Assert.IsTrue(result.Count == 0);
        }

        [Test]
        public void TestQueryNullCondition()
        {
            //When
            Checker.Enabled = true;
            Checker.RegisterEvent("default", "llego aqui", "CheckerTest.TestStoreEvent");
            //Then
            var result = Checker.GetEventsByQueryCondition(null);
            Assert.IsTrue(result.Count == 1);
            Assert.AreEqual("error GetEventsByQueryCondition", result[0].Category);
        }


        [Test]
        public void TestDisabledStoreOneEvent()
        {
            //When
            Checker.Enabled = false;
            Checker.RegisterEvent("default", "llego aqui");
            //Then
            var result = Checker.GetEventsByCategory("default");
            Assert.IsTrue(result.Count == 0);
        }

        [Test]
        public void TestStoreOneEventAfterUpdateEnabled()
        {            
            //When
            Checker.UpdateEnabled(true);
            Checker.RegisterEvent("default", "llego aqui");
            //Then
            var result = Checker.GetEventsByCategory("default");
            Assert.IsTrue(result.Count == 1);
            var ev = result[0];
            Assert.AreEqual("llego aqui", ev.Value);
        }

        [Test]
        public void TestViewEnabledState()
        {
            //When
            Checker.Enabled = false;            
            //Then
            var result = Checker.UpdateEnabled(true);
            Assert.IsFalse(result.OldState);
            Assert.IsTrue(result.NewState);

            result = Checker.UpdateEnabled(false);
            Assert.IsTrue(result.OldState);
            Assert.IsFalse(result.NewState);

            result = Checker.UpdateEnabled(false);
            Assert.IsFalse (result.OldState);
            Assert.IsFalse(result.NewState);
        }

        [Test]
        public void TestCleanAllEvents()
        {
            //When
            Checker.UpdateEnabled(true);
            Checker.RegisterEvent("default", "llego aqui");
            Checker.CleanAllEvents();
            //Then
            var result = Checker.GetEventsByCategory("default");
            Assert.IsTrue(result.Count == 0);            
        }

        [Test]
        public void TestSizeInMemory()
        {
            //When
            Checker.UpdateEnabled(true);
            Checker.RegisterEvent("default", "llego aqui");
           
            //Then
            var result = Checker.GetMBSizeInMemory();
            Assert.AreEqual(0.0033222591362126247d, result);
        }

        [Test]
        public void TestSizeInMemoryManyValues()
        {
            //When
            Checker.UpdateEnabled(true);
            for (int i = 0; i < 1000; i++)
            {
                Checker.RegisterEvent("defaultCat", "10/7/2021 4:49:28 PM	[Error]	{\"Message\":\"The given key was not present in the dictionary.\",\"StackTrace\":\"   at BizAgi.Deployment.ExportImportWrapper.Import(CConfiguration configuration, ILightPackage contentSource, LightProgressStatus progress, ImportValidationSettings validateMetadataSettings, Boolean savePackageInformationInTarget)\r\n   at Bizagi.Deployment.RestServices.Import.Execution.ImportBex.RunImport(ParametersExecuteImport paramsImport, AsyncExecutionLightPackage result)\",\"Inner\":{\"Message\":\"The given key was not present in the dictionary.\",\"StackTrace\":\"   at Bizagi.Metadata.Deployment.Import.AdvancedPackageImporter.Import(CConfiguration configuration, ILightPackage contentSource, LightProgressStatus progress, ImportValidationSettings validateMetadataSettings, Boolean savePackageInformationInTarget, Boolean resetCache, String filename, String username)\r\n   at Bizagi.Metadata.Deployment.ExportImport.Import(CConfiguration configuration, ILightPackage contentSource, LightProgressStatus progress, ImportValidationSettings validateMetadataSettings, Boolean savePackageInformationInTarget, Boolean resetCache)\"}}");
            }            

            //Then
            var result = Checker.GetMBSizeInMemory();
            Assert.AreEqual(1.1129568106312293d, result);
        }
    }
}