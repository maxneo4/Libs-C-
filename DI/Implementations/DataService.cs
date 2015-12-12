using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Interfaces;

namespace Implementations
{
    public class DataService : IDataService<String>
    {
        #region Miembros de IDataService<T>

        public String ReadObject(string query)
        {
            return null;
        }

        #endregion
    }
}
