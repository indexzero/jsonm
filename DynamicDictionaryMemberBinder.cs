using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Dynamic;

namespace Jsonm
{
    public class DynamicDictionaryMemberBinder : SetMemberBinder
    {
        public DynamicDictionaryMemberBinder(string name, bool ignoreCase)
            : base(name, ignoreCase)
        {
        }

        public override DynamicMetaObject FallbackSetMember(
            DynamicMetaObject target, 
            DynamicMetaObject value, 
            DynamicMetaObject errorSuggestion)
        {
            return null;
        }
    }
}
