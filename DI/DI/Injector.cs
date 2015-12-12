using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Collections;

namespace DI
{
    public class Injector : IDependencyInjection
    {
        #region Attributes

        private static Injector _instance;

        private Hashtable _hashInstances;

        private Hashtable _hashMapConcretTypes;

        private Dictionary<string, LifeTimeType> _dictionaryLifeTimeType;

        private Type _lifeTimeType = typeof(Annotations.LifeTime);        

        #endregion

        #region Singleton

        private Injector()
        {
            _hashInstances = new Hashtable();
            _hashMapConcretTypes = new Hashtable();
            _dictionaryLifeTimeType = new Dictionary<string, LifeTimeType>();
            BindParameters = new BindParameters();
        }

        public static IDependencyInjection GetInstance()
        {
            if (_instance == null)
                _instance = new Injector();
            return _instance;
        }

        #endregion

        #region Properties

        public BindParameters BindParameters { get; private set; }

        #endregion

        #region Miembros de IDependencyInjection

        public T Create<T>()
        {
            Type type = typeof(T);
            Object createdObject = Create(type);
            return (T)createdObject;
        }

        public void BindInstance(string fullName,object instance)
        {            
            _dictionaryLifeTimeType[fullName] = LifeTimeType.Singleton;
            string keyHash = GetKeyHash(fullName, LifeTimeType.Singleton);
            _hashInstances[keyHash] = instance;
        }

        public void BindLifeTimeType<T>(LifeTimeType lifeTimetype)
        {
            Type type = typeof(T);
            _dictionaryLifeTimeType[type.FullName] = lifeTimetype;
        }

        public Reference<T> GetReference<T>()
        {
            return Create<T>;
        }

        public Lazy<T> GetLazy<T>()
        {
            return new Lazy<T>(Create<T>);
        }

        public IEnumerable<T> GetYieldEnum<T>()
        {
            yield return Create<T>();
        }  

        #endregion

        #region Private methods

        private object Create(Type type)
        {
            Object createdObject = null;
            LifeTimeType lifeTimeType = GetLifeTimeAttribute(type);
            if (lifeTimeType != LifeTimeType.InstanceByCall)
                createdObject = CreateFromHashInstance(type, lifeTimeType);
            else
                createdObject = ResolveObject(type);
            return createdObject;
        }

        private Object ResolveObject(Type type)
        {
            Object createdObject = null;

            if (type.IsInterface)
                createdObject = CreateFromInterface(type);
            else
              createdObject = ResolveConstructor(type);            

            return createdObject;
        }

        private Object CreateFromInterface(Type type)
        {
            Type concretType = ResolveImplementationFromHashMapTypes(type);      
            if(concretType==null)
                throw new InjectorException(String.Format("Concret Type for {0} can not found!", type));
            Object createdObject = ResolveConstructor(concretType);
            return createdObject;
        }

        private Type ResolveImplementationFromHashMapTypes(Type type)
        {
            Type concretedType = null;
            if (_hashMapConcretTypes[type.FullName] != null)
                concretedType = (Type)_hashMapConcretTypes[type.FullName];
            else
            {
                concretedType = ResolveImplementation(type);
                _hashMapConcretTypes[type.FullName] = concretedType;
            }
            return concretedType;
        }

        private Type ResolveImplementation(Type type)
        {
            Type implementationType = ResolveImplementationFromOwnedAssembly(type);
            if (implementationType == null)
            {
                Assembly[] loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies();
                loadedAssemblies = loadedAssemblies.Where(a => !a.GlobalAssemblyCache).ToArray();
                IndexImplementations(loadedAssemblies);
                implementationType = (Type)_hashMapConcretTypes[type.FullName];
            }
            return implementationType;
        }

        private void IndexImplementations(Assembly[] loadedAssemblies)
        {
            foreach (Assembly assembly in loadedAssemblies)
                foreach (Type type in TryGetTypes(assembly))
                    if (!type.IsAbstract && !type.IsInterface)
                    {
                        Type[] interfaces = type.GetInterfaces();
                        foreach (Type typeImplementsInterface in interfaces)
                            if (typeImplementsInterface.FullName!=null && _hashMapConcretTypes[typeImplementsInterface.FullName] == null)
                                _hashMapConcretTypes[typeImplementsInterface.FullName] = type;
                    }
        }

        public static Type[] TryGetTypes(Assembly assembly)
        {
            Type[] types = null;
            try
            { 
                types = assembly.GetTypes();
            }catch
            {
                types = new Type[0];
            }
            return types;
        }

        private Type ResolveImplementationFromOwnedAssembly(Type type)
        {
            Type implementationType = null;
            Assembly typeAssembly = Assembly.GetAssembly(type);
            Type[] assemblyTypes = typeAssembly.GetTypes();
            foreach (Type t in assemblyTypes)
            {
                if (type.IsAssignableFrom(t) && !t.IsAbstract && !t.IsInterface)
                {
                    implementationType = t;
                    break;
                }
            }
            return implementationType;
        }

        private Object CreateFromHashInstance(Type type, LifeTimeType lifeTimeType)
        {
            string keyHash = GetKeyHash(type.FullName, lifeTimeType);
            Object createdObject = null;
            if (_hashInstances.ContainsKey(keyHash))
                createdObject = _hashInstances[keyHash];
            else 
            {
                createdObject = ResolveObject(type);
                _hashInstances[keyHash] = createdObject;
            }
            return createdObject;
        }
               
        private Object ResolveConstructor(Type type)
        {            
            Object[] instanceParameters = GetInstanceParameters(type);
            return GetInstanceInternalOrPublic(type, instanceParameters);
        }

        private static object GetInstanceInternalOrPublic(Type type, Object[] instanceParameters)
        {
            try
            {
                return Activator.CreateInstance(type, instanceParameters);
            }
            catch
            {
                return Activator.CreateInstance(type, BindingFlags.NonPublic | BindingFlags.Instance, null, instanceParameters, null);
            }
        }

        private Object[] GetInstanceParameters(Type type)
        {
            Object[] instanceParameters = BindParameters[type];
            if (instanceParameters == null)
            {
                ConstructorInfo constructorInfo = GetConstructorInfo(type);
                ParameterInfo[] parametersInfo = constructorInfo.GetParameters();
                int parametersCount = parametersInfo.Count();
                instanceParameters = new Object[parametersCount];               
                for (int i = 0; i < parametersCount; i++)
                {                    
                    instanceParameters[i] = Create(parametersInfo[i].ParameterType);
                }
            }            
            return instanceParameters;
        }

        public static ConstructorInfo GetConstructorInfo(Type type)
        {
            ConstructorInfo[] constructorsInfo = type.GetConstructors();
            if (constructorsInfo.Length < 1)
                constructorsInfo = type.GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance);
            if (constructorsInfo.Length < 1)
                throw new InjectorException(String.Format(
                    "Type {0} has not accessible any constructor!", type));
            return constructorsInfo[0];
        }

        private LifeTimeType GetLifeTimeAttribute(Type type)
        {
            LifeTimeType lifeTimeType = LifeTimeType.InstanceByCall;
            if (_dictionaryLifeTimeType.ContainsKey(type.FullName))
                lifeTimeType = _dictionaryLifeTimeType[type.FullName];
            else
            {
                Object[] attributes = type.GetCustomAttributes(_lifeTimeType, false);
                lifeTimeType = attributes.Length > 0 ?
                    ((Annotations.LifeTime)attributes[0]).Type :
                    LifeTimeType.InstanceByCall;
            }
            return lifeTimeType;
        }

        private String GetKeyHash(string fullName, LifeTimeType lifeTimeType)
        {
            return lifeTimeType == LifeTimeType.Singleton ?
                fullName : fullName + System.Threading.Thread.CurrentThread.ManagedThreadId;
        }

        #endregion


        #region Miembros de IDependencyInjection
                  
        #endregion
    }
}
