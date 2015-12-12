using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection.Emit;
using System.Reflection;
using System.Threading;

namespace Proxy
{
    public class ProxyGenerator
    {

        private static ProxyGenerator _instance;

        private ProxyGenerator()
        {
        }

        public static ProxyGenerator Getinstance()
        {
            return _instance?? (_instance = new ProxyGenerator());
        }

        public T Generate<T>()
        {            
            AssemblyName assemblyName = new AssemblyName();
            assemblyName.Name = "DerivedAssembly";
            AssemblyBuilder assemblyBuilder = Thread.GetDomain().DefineDynamicAssembly(
assemblyName, AssemblyBuilderAccess.Run);
            
            ModuleBuilder moduleBuilder = assemblyBuilder.DefineDynamicModule
        ("MDerivedAssembly");
            
            TypeBuilder typeBuilder = moduleBuilder.DefineType("CDerived",
TypeAttributes.Public);
            typeBuilder.AddInterfaceImplementation(typeof(T));

            Type t = typeBuilder.CreateType();
            return (T)assemblyBuilder.CreateInstance("CDerived");
        }
    }
}
