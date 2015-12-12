using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Reflection;

namespace Neo.JsonSerializer
{  
    internal class TypeProcess
    {

        #region JSon constants
        private const string BackSalsh = "\\";
        private const string BackSalshJson = "\\\\";
        private const string DoubleQuote = "\"";
        private const string DoubleQuoteJson = "\\\"";
        private const string BreakLine = "\n";
        private const string BreakLineJson = "\\n";
        private const string CarrierReturn = "\r";
        private const string CarrierReturnJson = "\\r";
        private const string Tab = "\t";
        private const string TabJson = "\\t";
        private const string Back = "\b";
        private const string BackJson = "\\b";
        #endregion

        private static IDictionary typeProcessDictionary = new Dictionary<Type, TypeProcess>();
                
        public static TypeProcess GetTypeProcess(object source, Type type)
        {
            return typeProcessDictionary.Contains(type) ? (TypeProcess)typeProcessDictionary[type] :
                BuildTypeProcess(source, type);
        }

        private static TypeProcess BuildTypeProcess(object source, Type type)
        {
            TypeProcess typeProcess = new TypeProcess();

            AssignBuildMethod(source, typeProcess, type);

            typeProcessDictionary[type] = typeProcess;
            return typeProcess;
        }

        private static void AssignBuildMethod(object source, TypeProcess typeProcess, Type type)
        {
            if (source is string)
                typeProcess.ConvertType = BuildStringJSon;
            else if (source is bool)
                typeProcess.ConvertType = BuildBoolJSon;
            else if (type.IsPrimitive)
                typeProcess.ConvertType = BuildPrimitiveJSon;
            else if (type.IsValueType)
                typeProcess.ConvertType = BuildStringValueJSon;
            else if (source is IDictionary)
                typeProcess.ConvertType = BuildDictionaryJSon;
            else if (source is byte[])            
                typeProcess.ConvertType = BuildByteArrayJson;
            else if (source is IEnumerable)
                typeProcess.ConvertType = BuildEnumerableJSon;
            else
                typeProcess.ConvertType = BuildObjectJSon;            
        }

        private static string BuildEnumerableJSon(object source, Type type)
        {
            StringBuilder stringBuilderA = new StringBuilder(JSonExtent.BeginArray);
            foreach (object item in (IEnumerable)source)
            {
                if (stringBuilderA.Length > 1)
                    stringBuilderA.Append(JSonExtent.SeparatorProperty);
                stringBuilderA.Append(item.ToJson());
            }
            stringBuilderA.Append(JSonExtent.EndArray);
            return stringBuilderA.ToString();
        } 

        private static string BuildByteArrayJson(object source, Type type)
        {
            return new StringBuilder(JSonExtent.Quote).Append(Convert.ToBase64String((byte[])source)).Append(JSonExtent.Quote).ToString();
        }

        private static string BuildDictionaryJSon(object source, Type type)
        {
            StringBuilder stringBuilderA = new StringBuilder(JSonExtent.BeginBracket);
            IDictionary dictionary = (IDictionary)source;
            foreach (object item in (dictionary.Keys))
            {
                if (stringBuilderA.Length > 1)
                    stringBuilderA.Append(JSonExtent.SeparatorProperty);
                stringBuilderA.Append(JSonExtent.Quote).Append(item.ToString()).Append(JSonExtent.Quote).Append(JSonExtent.Separator).Append(dictionary[item].ToJson());
            }
            stringBuilderA.Append(JSonExtent.EndBracket);
            return stringBuilderA.ToString();
        }

        private static string BuildStringJSon(object source, Type type)
        {
            StringBuilder result = new StringBuilder((String)source);
            result.Replace(BackSalsh, BackSalshJson);
            result.Replace(DoubleQuote, DoubleQuoteJson);
            result.Replace(BreakLine, BreakLineJson);
            result.Replace(CarrierReturn, CarrierReturnJson);
            result.Replace(Tab, TabJson);
            result.Replace(Back, BackJson);
            result.Insert(0, JSonExtent.Quote);
            result.Append(JSonExtent.Quote);
            return result.ToString();
        }        

        private static string BuildStringValueJSon(object source, Type type)
        {
            return String.Concat(JSonExtent.Quote, source, JSonExtent.Quote);
        }

        private static string BuildPrimitiveJSon(object source, Type type)
        {
            return source.ToString();
        }

        private static string BuildBoolJSon(object source, Type type)
        {
            return source.ToString().ToLower();
        }

        private static string BuildObjectJSon(object source, Type type)
        {
            StringBuilder stringBuilder = new StringBuilder(JSonExtent.BeginBracket);
            PropertyInfo[] properties = type.GetProperties();
            for (int i = 0; i < properties.Length; i++)
            {
                PropertyInfo propInfo = properties[i];
                object[] attributes = propInfo.GetCustomAttributes(true);
                if (!MustToIgnore(attributes) && propInfo.CanRead)
                {
                    object value = propInfo.GetValue(source, null);
                    if (!(value == null && JSonExtent.IgnorePropertyWhenValueIsNull))
                    {
                        if (stringBuilder.Length > 1)
                            stringBuilder.Append(JSonExtent.SeparatorProperty);
                        propInfo.ToJson(value, attributes, stringBuilder);
                    }
                }
            }
            stringBuilder.Append(JSonExtent.EndBracket);
            return stringBuilder.ToString();
        }

        private static bool MustToIgnore(object[] attributes)
        {
            foreach (object attribute in attributes)
                if (attribute is JSonIgnore)
                    return true;
            return false;
        }

        #region Non static methods

        public Func<object, Type, string> ConvertType { get; set; }        

        #endregion
    }
}
