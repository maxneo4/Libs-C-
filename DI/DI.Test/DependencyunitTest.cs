using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using DI.Test.SimpleClass;
using DI.Test.ControlInversion;
using AnotherProject;
using DI.Test.ConstructorInjection;
using System.IO;
using Interfaces;
using System.Reflection;
using DI.App;
using Module = DI.App.Module;
using DI.Map;

namespace DI.Test
{
    [TestClass]
    public class DependencyunitTest
    {
        private IDependencyInjection injector;

        [TestInitialize]
        public void Begin()
        {
            injector = Injector.GetInstance();
            injector.BindParameters.AddConstructorParameters("Ionic.Zip.ZipFile", "ZipFileTest.zip");            
        }

        [TestMethod]
        public void TestCreationWithReflection()
        {
            //given
            
            //when
            A a = injector.Create<A>();
            //then
            Assert.AreNotEqual(a, null);
        }

        [TestMethod]
        public void TestCreationFromInterface()
        {
            //given
            
            //when
            IPc pc = injector.Create<IPc>();
            IService service = injector.Create<IService>();
            //then
            Assert.AreNotEqual(pc, null);
            Assert.AreNotEqual(service, null);
            Assert.AreEqual(service.GetStatus(), "Success");
        }

        [TestMethod]
        public void TestCreationWithParametersInConstructor()
        {
            //given
            
            //when
            IWork work = injector.Create<IWork>();
            work.PrintServiceStatus();
            //then
            Assert.AreNotEqual(work, null);
            Assert.AreEqual("Success", File.ReadAllText(@"C:\status.txt"));
        }

        [TestMethod]
        public void TestCreationLazyObject()
        {
            //given
            
            //when
            IEnumerable<IWork> work = injector.GetYieldEnum<IWork>();

            IService singletonService = injector.Create<IService>();
            //Assert.AreEqual(singletonService.Assigned, false);

            work.First().PrintServiceStatus();

            //Assert.AreEqual(singletonService.Assigned, true);

            Reference<IWork> instance =  injector.GetReference<IWork>();
            instance().PrintServiceStatus();

            Lazy<IWork> lwork = injector.GetLazy<IWork>();
            lwork.Value.PrintServiceStatus();
            //then
            Assert.AreNotEqual(work.First(), null);
            Assert.AreEqual("Success", File.ReadAllText(@"C:\status.txt"));
            File.Delete(@"C:\status.txt");
        }


        [TestMethod]
        public void TestExceptionWhenDllIsRegisterTwoTimes()
        {
            //given
            IModule module = new Module("Test");
            bool exceptionIsThrow = false;
            //when
            module.RegisterAssembly("m.unit.dll");
            module.RegisterAssembly("Dummy.dll");
            try
            {
                module.RegisterAssembly("m.unit.dll");
            }
            catch { exceptionIsThrow = true; }
            //Then
            Assert.AreEqual(exceptionIsThrow, true);
        }

        //[TestMethod]
        //public void TestSameAssembly()
        //{
        //    Module moduleA = new Module("A");
        //    Module moduleB = new Module("B");
        //    moduleA.RegisterAssembly("Implementations.dll");
        //    moduleB.RegisterAssembly("Implementations.dll");

        //    moduleA.Load();
        //    moduleB.Load();

        //    Assembly[] asms = AppDomain.CurrentDomain.GetAssemblies();

        //    foreach (var item in asms)
        //    {

        //    }

        //}

        [TestMethod]
        public void TestCreationWhenInterfacesAndImplementationsAreIndifferentDll()
        {
            //given
            IModule moduleTest = new Module("test");
            moduleTest.RegisterAssembly("ImplementationsAnalysers.dll");
            IApplication application = new Application();
            application.RegisterModule(moduleTest);
            application.Load();
            //when
            IAnalyzer analizer = injector.Create<IAnalyzer>();
            string result = analizer.Analyze("anything");
            //Then
            Assert.AreNotEqual(result, null);
        }

        [TestMethod]
        public void TestBindNullInstance()
        {
            //Given
            Injector.GetInstance().BindInstance("DI.Test.InterfaceWithoutImplementation", null);
            //When
            IUseInterfaceWithoutImplementation useI = Injector.GetInstance().Create<IUseInterfaceWithoutImplementation>();
            //Then
            Assert.IsNotNull(useI);
        }

        [TestMethod]
        public void TestInternalConstructorAndDependencyNotImplemented()
        {
            //Given
            IModule moduleTest = new Module("test");
            moduleTest.RegisterAssembly("ImplementationsAnalysers.dll");
            IApplication application = new Application();
            application.RegisterModule(moduleTest);            
            injector.BindInstance("AnotherProject.IQueryBuilder", null);
            application.Load();
            //When
            IQueryService queryService = injector.Create<IQueryService>();
            //Then
            Assert.IsNotNull(queryService);
        }

        [TestMethod]
        public void TestMap()
        {
            //given
            IModule moduleTest = new Module("test");
            moduleTest.RegisterAssembly("ImplementationsAnalysers.dll");
            IApplication application = new Application();
            application.RegisterModule(moduleTest);
            application.Load();

            //When
            DependencyMap depMap = DependencyMap.GetDependencyMap<IAnalyzer>();
            DependencyMap depMapService = DependencyMap.GetDependencyMap<IService>();
            string xml = DependencyMap.GetDependencyMapAsXml(depMapService);
                          
            //Then
            Assert.IsNotNull(depMap);
            Assert.IsNotNull(depMapService);
            Assert.IsNotNull(xml);
        }
    }
}
