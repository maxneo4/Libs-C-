using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.IO;


namespace DI.App
{
    public interface IModule
    {
        string Name { get; set; }

        void RegisterAssembly(String dllName);

        IEnumerable<Assembly> Assemblies { get; }

        void Load();
    }

    public class Module : IModule
    {

        private List<String> registeredAssemblies { get; set; }
        private String DllsLocation { get; set; }
        private Assembly[] LoadedAssemblies { get; set; }

        public Module(string name)
        {
            Name = name;
            registeredAssemblies = new List<string>();
            string locationDll = Assembly.GetAssembly(this.GetType()).Location;
            DllsLocation = Path.GetDirectoryName(locationDll);
        }

        #region Miembros de IModule

        public string Name
        {
            get;
            set;
        }

        public void RegisterAssembly(string dllName)
        {
            if(registeredAssemblies.Contains(dllName))
                throw new InjectorException(String.Format("The dll {0} is already registered!",dllName));
            registeredAssemblies.Add(dllName);
        }

        public IEnumerable<Assembly> Assemblies
        {
            get
            {
                if (LoadedAssemblies == null)
                    LoadedAssemblies = new Assembly[registeredAssemblies.Count];
                                    
                for (int i = 0; i < LoadedAssemblies.Length; i++)
                {
                    if (LoadedAssemblies[i] == null)
                    {
                        string pathCurremtAssembly = Path.Combine(DllsLocation, registeredAssemblies[i]);
                        LoadedAssemblies[i] = Assembly.LoadFrom(pathCurremtAssembly);
                    }
                    yield return LoadedAssemblies[i];
                }                                
            }            
        }

        #endregion

        #region Miembros de IModule


        public void Load()
        {
            Assemblies.Count();
        }

        #endregion
    }
}
