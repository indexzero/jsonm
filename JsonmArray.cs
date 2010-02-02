using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace jsonm
{
    public class JsonmArray : List<object>
    {
        public JsonmArray(params object[] values)
        {
            this.AddRange(values);
        }
    }
}
