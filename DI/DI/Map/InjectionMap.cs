using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace DI.Map
{
    public class InjectionMap
    {

        public InformationType InterfaceType { get; set; }

        public InformationType ClassType { get; set; }

        public InjectionMap[] ConstructorDependency { get; set; }
               
        [XmlIgnore]
        public List<string> RegisterAssembly { get; set; }        
    }
}
