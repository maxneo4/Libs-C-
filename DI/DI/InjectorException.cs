using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DI
{
    public class InjectorException : ApplicationException
    {        
        public InjectorException(string message) : base(message)
        {
        }
    }
}
