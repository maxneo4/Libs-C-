using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DI
{
    public interface IDependencyInjection
    {
        BindParameters BindParameters { get;  }

        T Create<T>();

        void BindInstance(string fullName, object instace);

        void BindLifeTimeType<T>(LifeTimeType lifeTimetype);

        Lazy<T> GetLazy<T>();

        IEnumerable<T> GetYieldEnum<T>();

        Reference<T> GetReference<T>();   
    }

    public delegate T Reference<out T>();   
}
