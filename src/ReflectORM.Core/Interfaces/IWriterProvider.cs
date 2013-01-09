using System;
using System.Collections.Generic;
using System.Text;

namespace ReflectORM.Core
{
    /// <summary>
    /// An interface to provide a BaseDbWriter
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IWriterProvider<T>
    {
        /// <summary>
        /// Gets the writer.
        /// </summary>
        /// <value>The writer.</value>
        BaseDbWriter<T> Writer { get; }
    }
}
