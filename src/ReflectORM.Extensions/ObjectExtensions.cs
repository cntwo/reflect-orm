using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ReflectORM.Extensions
{
    public static class ObjectExtensions
    {
        public static bool IsNull(this object source)
        {
            return source == null;
        }

        public static string ToSafeString(this object source)
        {
            return (source ?? string.Empty).ToString();
        }
    }
}
