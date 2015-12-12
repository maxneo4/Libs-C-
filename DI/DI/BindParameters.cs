using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace DI
{
    public class BindParameters
    {
        #region Attributes

        private Hashtable bindingConstructorParameters;

        #endregion 

        #region Constructors

        public BindParameters()
        {
            bindingConstructorParameters = new Hashtable();
        }

        public Object[] this[String fullNameType]
        {
            get
            {
                return (Object[])bindingConstructorParameters[fullNameType];
            }            
        }

        public void AddConstructorParameters(string typeFullName, params Object[] parameters)
        {
            bindingConstructorParameters[typeFullName] = parameters;
        }

        public Object[] this[Type type]
        {
            get
            {
                return (Object[])bindingConstructorParameters[type.FullName];
            }           
        }

        public void AddConstructorParameters(Type type, params Object[] parameters)
        {
            bindingConstructorParameters[type.FullName] = parameters;
        }
           
        public long Length
        {
            get
            {
                return bindingConstructorParameters.Count;
            }
        }

        #endregion
    }
}
