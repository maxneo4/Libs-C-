using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Collections;

namespace Neo.JsonSerializer
{
    public static class JSonExtent
    {

        #region Constants
        internal const string BeginBracket = "{";
        internal const string EndBracket = "}";
        internal const string Separator = ":";
        internal const string Quote = "\"";
        internal const string SeparatorProperty = ",";
        internal const string BeginArray = "[";
        internal const string EndArray = "]";
        internal const string Null = "null";
        #endregion

        #region Properties

        public static bool PropertyNamesAsFirstLowerChar { get; set; }
        public static bool IgnorePropertyWhenValueIsNull { get; set; }

        #endregion

        public static string ToJson(this object source)
        {
            if (source == null)
                return Null;

            Type type = source.GetType();
            TypeProcess typeProcess = TypeProcess.GetTypeProcess(source, type);
            return typeProcess.ConvertType(source, type);
        }

        public static void ToJson(this PropertyInfo propertyInfo, object value, object[] attributes, StringBuilder stringBuilder)
        {
            stringBuilder.Append(Quote)
                .Append(GetPropertyToRename(attributes) ??
                (PropertyNamesAsFirstLowerChar ? FirstCharToLower(propertyInfo.Name) : propertyInfo.Name))
                .Append(Quote).Append(Separator);
            if (!AppendFormatValue(value, attributes, stringBuilder))
                stringBuilder.Append(value.ToJson());
        }

        private static string FirstCharToLower(string source)
        {
            return Char.IsLower(source[0]) ? source : Char.ToLowerInvariant(source[0]) + source.Substring(1);
        }

        #region Attributes modifications


        private static string GetPropertyToRename(object[] attributes)
        {
            foreach (object attribute in attributes)
                if (attribute is JSonPropertyName)
                    return ((JSonPropertyName)attribute).Name;
            return null;
        }

        private static bool AppendFormatValue(object value, object[] attributes, StringBuilder stringBuilder)
        {
            foreach (object attribute in attributes)
                if (attribute is JSonFormatValue)
                {
                    ((JSonFormatValue)attribute).FormatValue(value, stringBuilder);
                    return true;
                }
            return false;
        }

        #endregion

    }
}
