using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ReflectORM.Extensions
{
    public static class IEnumerableExtensions
    {
        public static bool IsEmpty<T>(this IEnumerable<T> source)
        {
            return !source.Any();
        }
    }
}
