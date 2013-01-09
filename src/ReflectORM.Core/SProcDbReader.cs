using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Specialized;
using System.Data.SqlClient;
using System.Data.Common;

namespace ReflectORM.Core
{
    /// <summary>
    /// A concrete implementation of the DbReader that uses stored procedures to read data from a database
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class SProcDbReader<T> : BaseDbReader<T>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SProcDbReader&lt;T&gt;"/> class.
        /// </summary>
        /// <param name="connectionString">The connection string.</param>
        public SProcDbReader(string connectionString) : base(connectionString) { }

        /// <summary>
        /// Get the name of the stored procedure for selecting a T
        /// </summary>
        protected virtual string SelectCommand
        {
            get { return string.Format("{0}{1}Select", StoredProcedurePrefix, DatabaseTableName); }
        }

        /// <summary>
        /// Get data
        /// </summary>
        /// <param name="id">The id of the record you want to retrieve.</param>
        /// <returns>The requested object, or null if it doesn't exist.</returns>
        public override T Get(int id)
        {
            if (_cache.ContainsKey(id))
                return _cache[id];

            ListDictionary list = new ListDictionary();
            list.Add(string.Format("@{0}", IdColumn), id);

            return Operation(SelectCommand, list, FromDb, true);
        }
        
        /// <summary>
        /// Get all data.
        /// </summary>
        /// <returns>All of this type.</returns>
        public override List<T> GetAll()
        {
            ListDictionary list = new ListDictionary();
            list.Add(string.Format("@{0}", IdColumn), DBNull.Value);

            return OperationList(SelectCommand, list, FromDb, true);
        }

        /// <summary>
        /// Check to see if a record exists.
        /// </summary>
        /// <param name="id">The id of the record you want to check.</param>
        /// <returns>True if the record exists, else false.</returns>
        public override bool Exists(int id)
        {
            ListDictionary list = new ListDictionary();
            list.Add(string.Format("@{0}", IdColumn), id);

            DbDataReader reader = Operation(SelectCommand, list, true);
            bool exists = reader.HasRows;

            CleanUp(reader);

            return exists;
        }
    }
}
