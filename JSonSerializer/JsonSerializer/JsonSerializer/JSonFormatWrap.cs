using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Neo.JsonSerializer
{
    public class JSonFormatWrap : JSonFormatValue
    {
        public string BeginWrap { get; private set; }
        public string EndWrap { get; private set; }

        public JSonFormatWrap(string beginWrap, string endWrap)
        {
            BeginWrap = beginWrap;
            EndWrap = endWrap;
        }

        public override void FormatValue(object value, StringBuilder stringBuilder)
        {
            stringBuilder.Append(BeginWrap).Append(JSonExtent.ToJson(value)).Append(EndWrap);
        }
    }
}
