using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DI;

namespace AnotherProject
{
    [Annotations.LifeTime( Type = LifeTimeType.Singleton )]
    public interface IService
    {
        string GetStatus();

        bool Assigned { get; set; }
    }
}
