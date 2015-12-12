using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Neo.JsonSerializer
{
    public abstract class JSonFormatValue : Attribute
    {
        public abstract void FormatValue(object value, StringBuilder stringBuilder);
    }
}
