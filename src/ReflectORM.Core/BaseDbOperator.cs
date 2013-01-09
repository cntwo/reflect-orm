using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using System.Collections.Specialized;
using System.Collections;
using System.Data;
using System.Data.Common;
using ReflectORM.Extensions;

namespace ReflectORM.Core
{
    /// <summary>
    /// The base class for operating on a database, has the core operation methods.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class BaseDbOperator<T> : ApplicationDatabase
    {
        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="BaseDbOperator&lt;T&gt;"/> class.
        /// </summary>
        /// <param name="connectionString">The connection string.</param>
        public BaseDbOperator(string connectionString) : base(connectionString) { _multipleConnections = true; }
        #endregion

        #region Properties
        /// <summary>
        /// Gets the name of the pagination table.
        /// </summary>
        /// <value>The name of the pagination table.</value>
        public virtual string PaginationTableName { get { return "pagination"; } }

        /// <summary>
        /// Gets the stored procedure prefix.
        /// </summary>
        /// <value>The stored procedure prefix.</value>
        public virtual string StoredProcedurePrefix { get { return "usp_"; } }

        /// <summary>
        /// Sets the transaction.
        /// </summary>
        /// <value>The transaction.</value>
        public SqlTransaction Transaction
        {
            set
            {
                _transaction = value;
                if (!_transaction.IsNull())
                    _transaction.Connection.StateChange += new StateChangeEventHandler(Connection_StateChange);
            }

        }

        /// <summary>
        /// Get the name of the database table this object maps to.
        /// </summary>
        public virtual string DatabaseTableName { get { return typeof(T).Name; } }

        /// <summary>
        /// Get the name of the ID column in the database.
        /// </summary>
        public virtual string IdColumn { get { return "Id"; } }

        /// <summary>
        /// Get the name of the column to sort by.
        /// </summary>
        protected virtual string SortColumn { get { return "Updated"; } }

        #endregion

        /// <summary>
        /// Handles the StateChange event of the Connection control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Data.StateChangeEventArgs"/> instance containing the event data.</param>
        /// <remarks>Tidies up in all controllers, where a transaction may have been used.</remarks>
        void Connection_StateChange(object sender, StateChangeEventArgs e)
        {
            if (e.CurrentState == ConnectionState.Closed)
                _transaction = null;
        }

        #region Method: Operation

        /// <summary>
        /// Performs a database operation.
        /// </summary>
        /// <param name="command">The command.</param>
        /// <returns>A data reader, or null if there were errors.</returns>
        protected virtual DbDataReader Operation(DbCommand command)
        {
            return Operation(command, true);
        }

        /// <summary>
        /// Operations the specified command.
        /// </summary>
        /// <param name="command">The command.</param>
        /// <param name="forceNewConnection">if set to <c>true</c> [force new connection].</param>
        /// <returns></returns>
        protected virtual DbDataReader Operation(DbCommand command, bool forceNewConnection)
        {
            DbConnection connection = Connection;
            if (!_transaction.IsNull() && !forceNewConnection)
            {
                connection = _transaction.Connection;
                command.Transaction = _transaction;
            }

            command.Connection = connection;

            try
            {
                return ExecuteReader(command);
            }
            catch
            {
                if (!_transaction.IsNull() && !forceNewConnection)
                    _transaction.Rollback();

                throw;
            }
        }

        /// <summary>
        /// Provides a database operation
        /// </summary>
        /// <param name="commandText">The command text.</param>
        /// <param name="parameters">The parameters.</param>
        /// <param name="commandType">Type of command.</param>
        /// <param name="forceNewConnection">if set to <c>true</c> [force new connection].</param>
        /// <returns>
        /// A data reader, or null if there were errors.
        /// </returns>
        protected virtual DbDataReader Operation(string commandText, ListDictionary parameters, CommandType commandType, bool forceNewConnection)
        {
            DbCommand command = Connection.CreateCommand(commandText);            

            //Main method for executing commands.          
            command.CommandType = commandType;

            foreach (DictionaryEntry de in parameters)
                command.AddParameterWithValue((string)de.Key, de.Value);

            return Operation(command, forceNewConnection);
        }

        /// <summary>
        /// Performs a database Operation using the specified command.
        /// </summary>
        /// <param name="commandText">The command text.</param>
        /// <returns>
        /// The data reader from the stored procedure.
        /// </returns>
        protected virtual DbDataReader Operation(string commandText)
        {
            bool forceNewConnection = false;
            return Operation(commandText, forceNewConnection);
        }
        /// <summary>
        /// Performs a database Operation using the specified command.
        /// </summary>
        /// <param name="commandText">The command text.</param>
        /// <param name="forceNewConnection">if set to <c>true</c> [force new connection].</param>
        /// <returns>
        /// The data reader from the stored procedure.
        /// </returns>
        protected virtual DbDataReader Operation(string commandText, bool forceNewConnection)
        {
            return Operation(commandText, new ListDictionary(), forceNewConnection);
        }

        /// <summary>
        /// Performs a database Operation using the specified command.
        /// </summary>
        /// <param name="commandText">The command text.</param>
        /// <param name="commandType">Type of the command.</param>
        /// <returns>
        /// The data reader from the stored procedure.
        /// </returns>
        protected virtual DbDataReader Operation(string commandText, CommandType commandType)
        {
            bool forceNewConnection = false;
            return Operation(commandText, commandType, forceNewConnection);
        }
        /// <summary>
        /// Performs a database Operation using the specified command.
        /// </summary>
        /// <param name="commandText">The command text.</param>
        /// <param name="commandType">Type of the command.</param>
        /// <param name="forceNewConnection">if set to <c>true</c> [force new connection].</param>
        /// <returns>
        /// The data reader from the stored procedure.
        /// </returns>
        protected virtual DbDataReader Operation(string commandText, CommandType commandType, bool forceNewConnection)
        {
            return Operation(commandText, new ListDictionary(), commandType, forceNewConnection);
        }

        /// <summary>
        /// Provides a database operation.
        /// </summary>
        /// <param name="commandText">The stored procedure to use.</param>
        /// <param name="parameters">List of parameters, ("ParameterName","Value").</param>
        /// <returns>
        /// The data reader from the stored procedure.
        /// </returns>
        protected virtual DbDataReader Operation(string commandText, ListDictionary parameters)
        {
            bool forceNewConnection = false;
            return Operation(commandText, parameters, forceNewConnection);
        }
        /// <summary>
        /// Provides a database operation.
        /// </summary>
        /// <param name="commandText">The stored procedure to use.</param>
        /// <param name="parameters">List of parameters, ("ParameterName","Value").</param>
        /// <param name="forceNewConnection">if set to <c>true</c> [force new connection].</param>
        /// <returns>
        /// The data reader from the stored procedure.
        /// </returns>
        protected virtual DbDataReader Operation(string commandText, ListDictionary parameters, bool forceNewConnection)
        {
            return Operation(commandText, parameters, CommandType.StoredProcedure, forceNewConnection);
        }

        /// <summary>
        /// Provides a database operation.
        /// </summary>
        /// <param name="commandText">The stored procedure to use.</param>
        /// <param name="parameters">The parameters.</param>
        /// <param name="commandType">Type of the command.</param>
        /// <returns>
        /// The data reader from the stored procedure.
        /// </returns>
        protected virtual DbDataReader Operation(string commandText, ListDictionary parameters, CommandType commandType)
        {
            bool forceNewConnection = false;
            return Operation(commandText, parameters, commandType, forceNewConnection);
        }
        #endregion

        /// <summary>
        /// An instance that inherits from this class must provide a cloning mechanism.
        /// </summary>
        /// <returns>A copy of the cloned object.</returns>
        public override object Clone()
        {
            throw new NotImplementedException();
        }
    }
}
