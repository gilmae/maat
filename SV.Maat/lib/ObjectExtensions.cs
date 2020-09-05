using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace SV.Maat.lib
{
    public static class ObjectExtensions
    {
        public static bool IsNull(this object o)
        {
            return o == null;
        }

        public static bool HasContent<T>(this IEnumerable<T> o)
        {
            return o != null && o.Any();
        }
    }
}
