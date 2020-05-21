using System;
namespace SV.Maat.lib
{
    public static class ObjectExtensions
    {
        public static bool IsNull(this object o)
        {
            return o == null;
        }
    }
}
