using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Specialized;
using System.Data.SqlClient;
using ReflectORM.Extensions;
using System.Data.Common;

namespace ReflectORM.Core
{
    /// <summary>
    /// A concrete implementation of the BaseDbWriter that uses stored procedures to write data to a database
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class SProcDbWriter<T> : BaseDbWriter<T>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SProcDbWriter&lt;T&gt;"/> class.
        /// </summary>
        /// <param name="connectionString">The connection string.</param>
        /// <param name="isUpdatable">The is updatable.</param>
        public SProcDbWriter(string connectionString, IsUpdateableDelegate isUpdatable) : base(connectionString, isUpdatable) { }

        /// <summary>
        /// Get the name of the stored procedure for deleting a T
        /// </summary>
        protected virtual string DeleteCommand
        {
            get { return string.Format("{0}{1}Delete", StoredProcedurePrefix, DatabaseTableName); }
        }
        /// <summary>
        /// Get the name of the stored procedure for inserting a T
        /// </summary>
        protected virtual string InsertCommand
        {
            get { return string.Format("{0}{1}Insert", StoredProcedurePrefix, DatabaseTableName); }
        }
        /// <summary>
        /// Get the name of the stored procedure for updating a T
        /// </summary>
        protected virtual string UpdateCommand
        {
            get { return string.Format("{0}{1}Update", StoredProcedurePrefix, DatabaseTableName); }
        }

        /// <summary>
        /// Delete data.
        /// </summary>
        /// <param name="id">The id of the record you want to delete.</param>
        /// <returns>Returns whether or not the record has been deleted.</returns>
        /// <remarks>Checks if the reader has any rows, so your stored proc should do a basic select of the id that is being deleted.</remarks>
        public override bool Delete(int id)
        {
            ListDictionary list = new ListDictionary();
            list.Add(string.Format("@{0}", IdColumn), id);

            
            DbDataReader reader = Operation(DeleteCommand, list, false);

            try
            {
                return (!reader.IsNull() && !reader.HasRows);
            }
            finally { CleanUp(reader); }
        }

        /// <summary>
        /// Save data.
        /// </summary>
        /// <param name="data">Data to save.</param>
        /// <returns>Return the saved data, or null if save failed.</returns>
        public override object Save(T data) { return Save(data, null); }

        /// <summary>
        /// Saves the specified data.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="transaction">The transaction.</param>
        /// <returns>The Id of the data saved</returns>
        public virtual object Save(T data, SqlTransaction transaction)
        {
            string commandText = InsertCommand;
            _transaction = transaction;
            if (IsUpdatable(data)) commandText = UpdateCommand;

            DbDataReader reader = Operation(commandText, data, ToDb<T>, false);

            object retVal = reader[0];
            CleanUp(reader);
            return retVal;
        }
    }
}
