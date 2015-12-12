using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Interfaces
{
    public interface IDataService<T>
    {
        T ReadObject(string query);
    }
}
