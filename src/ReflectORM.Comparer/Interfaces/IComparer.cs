using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ReflectORM.Comparer.Interfaces
{
    interface IComparer<T>
    {
        Comparison Compare(T left, T right);
    }
}
