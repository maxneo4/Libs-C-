using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DI
{
    public enum LifeTimeType
    {
        Singleton,
        InstanceByThread,
        InstanceByCall
    }

    public class Annotations
    {
        public class LifeTime : Attribute
        {
            public LifeTimeType Type { get; set; }
        }
    }
}
