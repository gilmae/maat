using System;
using System.Text;
using System.Text.Json;

namespace SV.Maat.lib
{
    public static class StringExtensions
    {
        public static string ToBase64String<T>(this T obj)
        {
            return Convert.ToBase64String(
                 Encoding.ASCII.GetBytes(
                     JsonSerializer.Serialize(obj)));
        }
        // TODO Needs TryFrom variant, something like
        // public static bool TryFromBase64String(this string obj, out T result)
        public static T FromBase64String<T>(this string obj)
        {
            return System.Text.Json.JsonSerializer.Deserialize<T>(Convert.FromBase64String(obj));
        }
    }
}
