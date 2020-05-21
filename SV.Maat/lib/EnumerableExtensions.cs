using System;
using System.Collections;
using System.Linq;

namespace SV.Maat.lib
{
    public static class CollectionExtensions
    {
        public static bool IsEmpty(this ICollection enumerable)
        {
            return enumerable.IsNull() || enumerable.Count == 0;
        }
    }
}
