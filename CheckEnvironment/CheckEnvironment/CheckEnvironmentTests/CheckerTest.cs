using CheckEnvironment;
using NUnit.Framework;

namespace CheckEnvironmentTests
{
    public class Tests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void TestStoreOneEvent()
        {
            //When
            Checker.RegisterEvent("default", "llego aqui");
            //Then
            var result = Checker.GetEventsByCategory("default");
            Assert.IsTrue(result.Count == 1);
            var ev = result[0];
            Assert.AreEqual("llego aqui", ev.Value);
        }
    }
}