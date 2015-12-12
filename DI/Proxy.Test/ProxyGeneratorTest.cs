using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Proxy.Test
{
    [TestClass]
    public class ProxyGeneratorTest
    {
        [TestMethod]
        public void GetImplementationEmptyInterfaceTest()
        {
            //Given
            ProxyGenerator proxyGenerator = ProxyGenerator.Getinstance();
            //When
            IEmptyInterface emptyImplementation = proxyGenerator.Generate<IEmptyInterface>();
            //Then
            Assert.AreNotEqual(emptyImplementation, null);
        }

        [TestMethod]
        public void GetImplementationSimpleInterfaceTest()
        {
            //Given
            ProxyGenerator proxyGenerator = ProxyGenerator.Getinstance();
            //When
            //IFactorial emptyImplementation = proxyGenerator.Generate<IFactorial>();
            //Then
            //Assert.AreNotEqual(emptyImplementation, null);
        }


        public interface IEmptyInterface
        {
        }

        public interface IFactorial
        {
            int CalculateFactorial(int value);
        }
    }
}
