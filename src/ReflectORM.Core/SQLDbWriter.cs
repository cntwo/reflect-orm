using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using System.Data.Common;
using ReflectORM.Extensions;

namespace ReflectORM.Core
{
    /// <summary>
    /// A concrete implementation of the BaseDbWriter that uses SQL to write data to a database
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class SQLDbWriter<T> : BaseDbWriter<T>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SQLDbWriter&lt;T&gt;"/> class.
        /// </summary>
        /// <param name="connectionString">The connection string.</param>
        /// <param name="isUpdatable">The is updatable.</param>
        public SQLDbWriter(string connectionString, IsUpdateableDelegate isUpdatable) : base(connectionString, isUpdatable) { }

        /// <summary>
        /// Deletes the specified id.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <returns></returns>
        public override bool Delete(int id)
        {
            DbCommand command = Connection.CreateCommand();

            Criteria criterion = new Criteria(IdColumn, ColumnType.Int, id);

            SQLGenerator<T> generator = new SQLGenerator<T>(command);
            generator.Criteria.Add(criterion);

            command.CommandText = generator.GenerateDelete(DatabaseTableName);

            DbDataReader reader = Operation(command);

            try
            {
                return (!reader.IsNull() && !reader.HasRows);
            }
            finally { CleanUp(reader); }
        }

        /// <summary>
        /// Saves the specified data.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <returns>The Id of the saved data</returns>
        public override object Save(T data)
        {
            try
            {
                DbCommand command = Connection.CreateCommand();

                SQLGenerator<T> generator = new SQLGenerator<T>(command);

                if (IsUpdatable(data))
                    command.CommandText = generator.GenerateUpdate(DatabaseTableName, data, ToDb<T>, IdColumn);
                else
                    command.CommandText = generator.GenerateInsert(DatabaseTableName, data, ToDb<T>, IdColumn);

                DbDataReader reader = Operation(command);

                object retVal = null;
                if (reader.Read())
                    retVal = reader[0];

                CleanUp(reader);
                return retVal;
            }
            catch { return null; }  //if error processing is implemented into the library, this implementation may be a problem.
        }
    }
}
