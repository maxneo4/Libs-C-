using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Neo.JsonSerializer
{
    public class JSonPropertyName : Attribute
    {
        public string Name { get; set; }

        public JSonPropertyName(string name)
        {
            Name = name;
        }
    }
}
