using System;
using System.Collections.Generic;
using System.Linq;

namespace SV.Maat.lib
{
    public static class MapExtensions
    {
        public static Dictionary<TKey, TValue> DuplicateOnlyKeys<TKey, TValue>(this Dictionary<TKey, TValue> dict, TKey[] keys)
        {
            if (dict == null)
            {
                return null;
            }
            Dictionary<TKey, TValue> newDict = new Dictionary<TKey, TValue>();

            foreach(TKey key in dict.Keys)
            {
                if (keys.Contains(key))
                {
                    newDict.Add(key, dict[key]);
                }
            }
            return newDict;
        }
    }
}
