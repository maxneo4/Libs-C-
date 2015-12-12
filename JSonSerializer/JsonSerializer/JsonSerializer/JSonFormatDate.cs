using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Neo.JsonSerializer
{
    public class JSonFormatDate : JSonFormatValue
    {
        public string DateFormat { get; private set; }

        public JSonFormatDate(string dateFormat)
        {
            DateFormat = dateFormat;
        }

        public override void FormatValue(object value, StringBuilder stringBuilder)
        {
            DateTime date = (DateTime)value;
            stringBuilder.Append(JSonExtent.ToJson(date.ToString(DateFormat)));
        }
    }
}
