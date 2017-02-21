using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Collections;
using System.Xml.Serialization;
using System.IO;
using DI.App;

namespace DI.Map
{
    public class DependencyMap
    {
        private static Hashtable _hashMapConcretTypes = new Hashtable();
        private static string[] _fullNameInterfacesToExclude = null;

        public InjectionMap Root { get; set; }
        public List<string> UsedAssemblies { get; set; }

        public DependencyMap()
        {
            UsedAssemblies = new List<string>();            
        }

        public static DependencyMap GetDependencyMap<T>(params string[] fullNameInterfacesToExclude)
        {
            Assembly[] loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies();
            loadedAssemblies = loadedAssemblies.Where(a => !a.GlobalAssemblyCache).ToArray();
            IndexImplementations(loadedAssemblies);
            _fullNameInterfacesToExclude = fullNameInterfacesToExclude;
            foreach (string classToExclude in fullNameInterfacesToExclude)
            {
                _hashMapConcretTypes.Remove(classToExclude);
            }
            Type type = typeof(T);
            DependencyMap dependencyMap = new DependencyMap();
            dependencyMap.Root = new InjectionMap();
            dependencyMap.Root.InterfaceType = new InformationType(type);
            dependencyMap.Root.RegisterAssembly = dependencyMap.UsedAssemblies;

            ResolveImplementation(dependencyMap.Root);

            GetDependencyMap(dependencyMap.Root);
            return dependencyMap;
        }

        public static string GetDependencyMapAsXml(DependencyMap dependencyMap)
        {
            XmlSerializer xmlSerializer = new XmlSerializer(dependencyMap.GetType());
            using (StringWriter textWriter = new StringWriter())
            {
                xmlSerializer.Serialize(textWriter, dependencyMap);
                return textWriter.ToString();
            }
        }

        public static FileInfo[] LoadAllAssembliesInSamePath()
        {
            IModule moduleTest = new DI.App.Module("allDlls");
            string path = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            DirectoryInfo di = new DirectoryInfo(path);
            FileInfo[] fls = di.GetFiles("*.dll");
            foreach (FileInfo fis in fls)
                moduleTest.RegisterAssembly(fis.Name);
            IApplication application = new Application();
            application.RegisterModule(moduleTest);
            application.Load();
            return fls;
        }

        private static void GetDependencyMap(InjectionMap implementation)
        {
            if (implementation.InterfaceType.Type.IsInterface)
                ResolveImplementation(implementation);
            else
                ResolveConstructor(implementation);

            if (!implementation.RegisterAssembly.Contains(implementation.InterfaceType.Assembly))
                implementation.RegisterAssembly.Add(implementation.InterfaceType.Assembly);
            if (implementation.ClassType!=null && !implementation.RegisterAssembly.Contains(implementation.ClassType.Assembly))
                implementation.RegisterAssembly.Add(implementation.ClassType.Assembly);
        }       

        private static void ResolveImplementation(InjectionMap implementation)
        {
            ResolveImplementationFromOwnAssembly(implementation);
            if (implementation.ClassType == null)
            {               
                Type type = implementation.InterfaceType.Type;
                Type implementationType = (Type)_hashMapConcretTypes[type.FullName];                
                if(implementationType!=null)
                    implementation.ClassType = new InformationType(implementationType);
            }
            ResolveConstructor(implementation);
        }

        private static void IndexImplementations(Assembly[] loadedAssemblies)
        {
            foreach (Assembly assembly in loadedAssemblies)
                foreach (Type type in Injector.TryGetTypes(assembly))
                    if (!type.IsAbstract && !type.IsInterface)
                    {
                        Type[] interfaces = type.GetInterfaces();
                        foreach (Type typeImplementsInterface in interfaces)
                            if (typeImplementsInterface.FullName != null && _hashMapConcretTypes[typeImplementsInterface.FullName] == null)
                                _hashMapConcretTypes[typeImplementsInterface.FullName] = type;
                    }
        }

        private static void ResolveImplementationFromOwnAssembly(InjectionMap implementation)
        {
            Type type = implementation.InterfaceType.Type;
            Assembly typeAssembly = Assembly.GetAssembly(type);
            Type[] assemblyTypes = typeAssembly.GetTypes();
            foreach (Type t in assemblyTypes)
            {
                if (type.IsAssignableFrom(t) && !t.IsAbstract && !t.IsInterface && !_fullNameInterfacesToExclude.Contains(type.FullName))
                {
                    implementation.ClassType = new InformationType(t);
                    break;
                }
            }            
        }

        private static void ResolveConstructor(InjectionMap implementation)
        {
            if (implementation.ClassType == null)
                return;
            implementation.ConstructorDependency = GetImplementationParameters(implementation);
        }

        private static InjectionMap[] GetImplementationParameters(InjectionMap implementation)
        {
            Type type = implementation.ClassType.Type;
            ConstructorInfo constructorInfo = Injector.GetConstructorInfo(type);
            ParameterInfo[] parametersInfo = constructorInfo.GetParameters();
            int parametersCount = parametersInfo.Count();
            InjectionMap[] implementationParameters = new InjectionMap[parametersCount];
            for (int i = 0; i < parametersCount; i++)
            {
                implementationParameters[i] = new InjectionMap() 
                { InterfaceType = new InformationType(parametersInfo[i].ParameterType), RegisterAssembly = implementation.RegisterAssembly };
                GetDependencyMap(implementationParameters[i]);
            }
            return implementationParameters;
        }
    }    
}
