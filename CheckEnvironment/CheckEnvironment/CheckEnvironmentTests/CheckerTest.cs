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
            Checker.RegisterEvent("default", "llego aqui");
            //Then
            var result = Checker.GetEventsByCategory("default");
            Assert.IsTrue(result.Count == 1);
            var ev = result[0];
            Assert.AreEqual("llego aqui", ev.Value);
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
    }
}