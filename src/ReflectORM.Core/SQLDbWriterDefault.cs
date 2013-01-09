using System;
using System.Collections.Generic;
using System.Text;

namespace ReflectORM.Core
{
    /// <summary>
    /// A concrete implementation of the BaseDbWriter that uses SQL to write data to a database, and has an implementation of IsUpdatable
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class SQLDbWriterDefault<T> : SQLDbWriter<T> where T : IControllable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SQLDbWriterDefault&lt;T&gt;"/> class.
        /// </summary>
        /// <param name="connectionString">The connection string.</param>
        public SQLDbWriterDefault(string connectionString) : base(connectionString, IsUpdatable) { }

        /// <summary>
        /// Determines whether the specified data is updatable.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <returns>
        /// 	<c>true</c> if the specified data is updatable; otherwise, <c>false</c>.
        /// </returns>
        public static new bool IsUpdatable(T data)
        {
            return (data.Id != DbConstants.DEFAULT_ID);
        }
    }
}
