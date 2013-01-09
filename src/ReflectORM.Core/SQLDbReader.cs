using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using System.Collections.Specialized;
using System.Data;
using System.Data.Common;

namespace ReflectORM.Core
{
    /// <summary>
    /// A concrete implementation of the BaseDbWriter that uses SQL to read data from a database
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class SQLDbReader<T> : BaseDbReader<T>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SQLDbReader&lt;T&gt;"/> class.
        /// </summary>
        /// <param name="connectionString">The connection string.</param>
        public SQLDbReader(string connectionString) : base(connectionString) { }

        /// <summary>
        /// Get data
        /// </summary>
        /// <param name="id">The id of the record you want to retrieve.</param>
        /// <returns>
        /// The requested object, or null if it doesn't exist.
        /// </returns>
        public override T Get(int id)
        {
            if (_cache.ContainsKey(id))
                return _cache[id];

            DbCommand command = Connection.CreateCommand();

            Criteria criterion = new Criteria(IdColumn, ColumnType.Int, id);

            SQLGenerator<T> generator = new SQLGenerator<T>(command);
            generator.Criteria.Add(criterion);

            command.CommandText = generator.GenerateSelect(DatabaseTableName);

            return Operation(command, FromDb);
        }

        /// <summary>
        /// Check to see if a record exists.
        /// </summary>
        /// <param name="id">The id of the record you want to check.</param>
        /// <returns>True if the record exists, else false.</returns>
        public override bool Exists(int id)
        {
            DbCommand command = Connection.CreateCommand();

            Criteria criterion = new Criteria(IdColumn, ColumnType.Int, id);

            SQLGenerator<T> generator = new SQLGenerator<T>(command);
            generator.Criteria.Add(criterion);

            command.CommandText = generator.GenerateSelect(DatabaseTableName);

            return Operation(command).HasRows;
        }

        /// <summary>
        /// Get all data.
        /// </summary>
        /// <returns>All of this type.</returns>
        public override List<T> GetAll()
        {
            DbCommand command = Connection.CreateCommand();

            SQLGenerator<T> generator = new SQLGenerator<T>(command);
            command.CommandText = generator.GenerateSelect(DatabaseTableName);

            return OperationList(command, FromDb);            
        }
    }
}
