using System;
using System.Text;
using ReflectORM.Attributes;
using ReflectORM.Comparer.Interfaces;
using System.Reflection;

namespace ReflectORM.Comparer
{
    public class ObjectComparer<T> : IComparer<T> where T:class
    {
        /// <summary>
        /// Compares the two given objects and returns information about the comparison.
        /// </summary>
        /// <param name="left">The first object to compare</param>
        /// <param name="right">The second object to compare</param>
        /// <returns>A Comparison object details information from the result of comparing the two given objects</returns>
        public Comparison Compare(T left, T right)
        {
            //this really doesn't play nice with null objects
            if (left == null) throw new ArgumentNullException("left"); 
            if (right == null) throw new ArgumentNullException("right");

            var type = typeof(T);
            var properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public);

            var comparison = new Comparison();
            
            foreach (var property in properties)
            {
                var leftVal = property.GetValue(left, null);
                var rightVal = property.GetValue(right, null);

                //if both values are null, then they are obviously equal, move on
                if (leftVal == null && rightVal == null) continue;

                if (leftVal == null // both leftVal and rightVal can't be null, so this means they are different
                    || rightVal == null
                    || !leftVal.Equals(rightVal))
                {
                    comparison.ChangedProperties.Add(property); //the two values are different, add it to the list and carry on
                }
            }

            return comparison;
        }
    }
}
