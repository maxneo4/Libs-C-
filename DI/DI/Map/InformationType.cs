using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace DI.Map
{
    public class InformationType
    {
        public string FullName { get; set; }
        public string Assembly { get; set; }
        [XmlIgnore]
        public Type Type { get; set; }

        public bool IsInterface { get { return Type.IsInterface; } set { } }

        public InformationType(Type type)
        {
            FullName = type.FullName;
            Assembly = type.Assembly.FullName;
            Type = type;
        }

        public InformationType()
        {
        }
    }
}
