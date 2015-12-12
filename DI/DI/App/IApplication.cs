using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DI.App
{
    public interface IApplication
    {
        void RegisterModule(IModule module);

        void Load();
    }

    public class Application : IApplication
    {

        #region Properties

        public List<IModule> Modules { get; private set; }

        #endregion

        public Application()
        {
            Modules = new List<IModule>();
        }

        #region Miembros de IApplication

        public void RegisterModule(IModule module)
        {
            Modules.Add(module);
        }

        public void Load()
        {
            foreach (IModule module in Modules)            
                module.Load();          
        }

        #endregion
    }


}
