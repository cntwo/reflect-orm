using System;
using System.Collections.Generic;
using System.Text;

namespace ReflectORM.Core
{
    /// <summary>
    /// An interface to provider a BaseDataReader
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IReaderProvider<T>
    {
        /// <summary>
        /// Gets the reader.
        /// </summary>
        /// <value>The reader.</value>
        BaseDbReader<T> Reader { get; }
    }
}
