using ReflectORM.Comparer.Interfaces;

namespace ReflectORM.Comparer
{
    public class ObjectComparer<T> : IComparer<T>
    {
        public Comparison Compare(T left, T right)
        {
            throw new System.NotImplementedException();
        }
    }
}
