using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace ReflectORM.Core
{
    /// <summary>
    /// Base controller
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class BaseController<T> where T : IControllable
    {
        /// <summary>
        /// Gets whether or not the record is updatable or insertable.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <returns></returns>
        protected virtual bool IsUpdatable(T data) { return data.Id != DbConstants.DEFAULT_ID; }

        ///// <summary>
        ///// Compares an object or list against its database counterpart.  Non-recursive.
        ///// </summary>
        ///// <param name="a"></param>
        ///// <param name="b"></param>
        ///// <returns>Bool</returns>
        //public virtual bool HasChanged(T a)
        //{
        //    T b = Get(a.Id);
        //    return HasChanged(a, b);
        //}

        ///// <summary>
        ///// Compares 2 objects or lists.  Non-recursive.
        ///// </summary>
        ///// <param name="a"></param>
        ///// <param name="b"></param>
        ///// <returns>Bool</returns>
        //public virtual bool HasChanged(T a, T b)
        //{
        //    if (a is IList)
        //        return CompareLists((IList)a, (IList)b);
        //    else
        //        return CompareObjects(a, b);
        //}

        ////Compares two objects
        //private bool CompareObjects(T a, T b)
        //{
        //    return (!CBRI.DataValidation.ObjectComparer.Compare(a, b));
        //}

        ////Iterates through the lists comparing each item
        //private bool CompareLists(IList a, IList b)
        //{
        //    if (a.Count != b.Count)
        //        return true;

        //    foreach (object o in b)
        //    {
        //        int count = 0;
        //        if (!CBRI.DataValidation.ObjectComparer.Compare(a[count], o))
        //            return true;
        //        count++;
        //    }
        //    return false;
        //}
    }
}
