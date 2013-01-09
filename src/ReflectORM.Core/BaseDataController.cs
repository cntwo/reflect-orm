using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;
using ReflectORM.Core.ChangeHistory;
using ReflectORM.Extensions;
using ReflectORM.Comparer;

namespace ReflectORM.Core
{
    /// <summary>
    /// Abstract data handler class.
    /// </summary>
    /// <typeparam name="T">The type of Data to handle.</typeparam>
    public abstract class BaseDataController<T> : ApplicationDatabase where T:class
    {
        /// <summary>
        /// The Database Table Name
        /// </summary>
        protected string _databaseTableName = string.Empty;

        /// <summary>
        /// The View to select from.
        /// </summary>
        protected string _selectViewName = string.Empty;

        /// <summary>
        /// Get the name of the History table
        /// </summary>
        public virtual string HistoryTableName { get { return string.Format("{0}_History", DatabaseTableName); } }

        /// <summary>
        /// Get the name of the History column table
        /// </summary>
        public virtual string HistoryColumnTableName { get { return string.Format("{0}_Column", HistoryTableName); } }

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
        /// Gets the data object suffix.
        /// </summary>
        public virtual string DataObjectSuffix { get { return "data"; } }

        /// <summary>
        /// Gets a value indicating whether the handler allows the value of the ids to be provided
        /// </summary>
        /// <value>
        ///   <c>true</c> if [insert id]; otherwise, <c>false</c>.
        /// </value>
        public virtual bool InsertId { get { return false; } }

        #region Contructors

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseDataController&lt;T&gt;"/> class.
        /// </summary>
        /// <param name="connectionString">The connection string.</param>
        public BaseDataController(string connectionString) : base(connectionString) { _multipleConnections = true; }

        #endregion Contructors

        #region Commands

        //Default Command names, following the naming scheme: [TableName][Command] eg: BrewInsert;

        /// <summary>
        /// Get the name of the stored procedure for selecting a T
        /// </summary>
        protected virtual string SelectCommand
        {
            get { return string.Format("{0}{1}Select", StoredProcedurePrefix, DatabaseTableName); }
        }

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

        #endregion Commands

        /// <summary>
        /// Sets the transaction.
        /// </summary>
        /// <value>The transaction.</value>
        public DbTransaction Transaction
        {
            set
            {
                _transaction = value;
                if (!_transaction.IsNull())
                    _transaction.Connection.StateChange += new StateChangeEventHandler(Connection_StateChange);
            }
        }

        /// <summary>
        /// Handles the StateChange event of the ProfiledConnection control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Data.StateChangeEventArgs"/> instance containing the event data.</param>
        /// <remarks>Tidies up in all controllers, where a transaction may have been used.</remarks>
        private void Connection_StateChange(object sender, StateChangeEventArgs e)
        {
            if (e.CurrentState == ConnectionState.Closed)
                _transaction = null;
        }

        /// <summary>
        /// Get the name of the database table this object maps to.
        /// </summary>
        public virtual string DatabaseTableName
        {
            get
            {
                if (_databaseTableName != string.Empty) return _databaseTableName;
                else
                {
                    string dbName = typeof(T).Name;
                    if (dbName.ToLower().EndsWith(DataObjectSuffix.ToLower()) && dbName.Length > DataObjectSuffix.Length)
                        return dbName.Substring(0, dbName.Length - DataObjectSuffix.Length);
                    return dbName;
                }
            }
            set
            {
                _databaseTableName = value;
            }
        }

        /// <summary>
        /// Gets or sets the name of the select view.
        /// </summary>
        /// <remarks>This is to be used when you need to select from a view/table that doesn't match DatabaseTableName. If not set, this will return DatabaseTableName.</remarks>
        /// <value>The name of the select view.</value>
        public virtual string SelectViewName
        {
            get
            {
                if (_selectViewName != string.Empty) return _selectViewName;
                else return DatabaseTableName;
            }
            set { _selectViewName = value; }
        }

        /// <summary>
        /// Get the name of the column to sort by.
        /// </summary>
        protected virtual string SortColumn
        {
            get
            {
                if (typeof(IAuditable).IsAssignableFrom(typeof(T))) //check to see if T is an IAuditable
                    return "EditedOn";
                return IdColumn;
            }
        }

        /// <summary>
        /// Get the name of the ID column in the database.
        /// </summary>
        public virtual string IdColumn { get { return "Id"; } }

        /// <summary>
        /// A delegate for setting up the parameters for a stored procedure.
        /// </summary>
        /// <param name="data">The data for the parameters</param>
        /// <param name="command">The DbCommand that has the parameters to set.</param>
        protected delegate void SetParameterDelegate(T data, ref DbCommand command);

        /// <summary>
        /// A delegate for getting the data from a datareader.
        /// </summary>
        /// <param name="reader">The datareader that has the data.</param>
        /// <returns>A completed T.</returns>
        protected delegate T GetDataDelegate(DbDataReader reader);

        private void EmptyParams(T data, ref DbCommand command)
        {
            return; //nothing
        }

        #region Operation

        /// <summary>
        /// Provides a database operation
        /// </summary>
        /// <param name="commandText">The stored procedure to use.</param>
        /// <param name="data">The data to be used in the operation</param>
        /// <param name="setParam">The method for setting the parameters for the stored procedure.</param>
        /// <returns>
        /// A data reader, or null if there were errors.
        /// </returns>
        protected virtual DbDataReader Operation(string commandText, T data, SetParameterDelegate setParam)
        {
            bool forceNewConnection = false;
            return Operation(commandText, data, setParam, forceNewConnection);
        }

        /// <summary>
        /// Provides a database operation
        /// </summary>
        /// <param name="commandText">The stored procedure to use.</param>
        /// <param name="data">The data to be used in the operation</param>
        /// <param name="setParam">The method for setting the parameters for the stored procedure.</param>
        /// <param name="forceNewConnection">if set to <c>true</c> [force new connection].</param>
        /// <returns>
        /// A data reader, or null if there were errors.
        /// </returns>
        protected virtual DbDataReader Operation(string commandText, T data, SetParameterDelegate setParam, bool forceNewConnection)
        {
            return Operation(commandText, data, setParam, CommandType.StoredProcedure, forceNewConnection);
        }

        /// <summary>
        /// Provides a database operation
        /// </summary>
        /// <param name="commandText">The command text.</param>
        /// <param name="data">The data to be used in the operation</param>
        /// <param name="setParam">The method for setting the parameters for the stored procedure.</param>
        /// <param name="commandType">Type of command.</param>
        /// <returns>
        /// A data reader, or null if there were errors.
        /// </returns>
        protected virtual DbDataReader Operation(string commandText, T data, SetParameterDelegate setParam, CommandType commandType)
        {
            return Operation(commandText, data, setParam, commandType, false);
        }

        /// <summary>
        /// Provides a database operation
        /// </summary>
        /// <param name="commandText">The command text.</param>
        /// <param name="data">The data to be used in the operation</param>
        /// <param name="setParam">The method for setting the parameters for the stored procedure.</param>
        /// <param name="commandType">Type of command.</param>
        /// <param name="forceNewConnection">if set to <c>true</c> [force new connection].</param>
        /// <returns>
        /// A data reader, or null if there were errors.
        /// </returns>
        protected virtual DbDataReader Operation(string commandText, T data, SetParameterDelegate setParam, CommandType commandType, bool forceNewConnection)
        {
            DbCommand command = ProfiledConnection.CreateCommand();
            command.CommandText = commandText;
            DbConnection connection = ProfiledConnection;
            if (!_transaction.IsNull() && !forceNewConnection)
            {
                connection = _transaction.Connection;
                command.Transaction = _transaction;
                command.Connection = connection;
            }

            //Main method for executing commands.
            command.CommandType = commandType;

            if (setParam != null)
                setParam(data, ref command);

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
            return Operation(commandText, default(T), EmptyParams, forceNewConnection);
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
            return Operation(commandText, EmptyParams, commandType, forceNewConnection);
        }

        /// <summary>
        /// Provides a database operation.
        /// </summary>
        /// <param name="commandText">The stored procedure to use.</param>
        /// <param name="toDb">The method for setting the parameters for the stored procedure.</param>
        /// <returns>
        /// The data reader from the stored procedure.
        /// </returns>
        protected virtual DbDataReader Operation(string commandText, SetParameterDelegate toDb)
        {
            bool forceNewConnection = false;
            return Operation(commandText, toDb, forceNewConnection);
        }

        /// <summary>
        /// Provides a database operation.
        /// </summary>
        /// <param name="commandText">The stored procedure to use.</param>
        /// <param name="toDb">The method for setting the parameters for the stored procedure.</param>
        /// <param name="forceNewConnection">if set to <c>true</c> [force new connection].</param>
        /// <returns>
        /// The data reader from the stored procedure.
        /// </returns>
        protected virtual DbDataReader Operation(string commandText, SetParameterDelegate toDb, bool forceNewConnection)
        {
            return Operation(commandText, toDb, CommandType.StoredProcedure, forceNewConnection);
        }

        /// <summary>
        /// Provides a database operation.
        /// </summary>
        /// <param name="commandText">The stored procedure to use.</param>
        /// <param name="toDb">The method for setting the parameters for the stored procedure.</param>
        /// <param name="commandType">Type of the command.</param>
        /// <returns>
        /// The data reader from the stored procedure.
        /// </returns>
        protected virtual DbDataReader Operation(string commandText, SetParameterDelegate toDb, CommandType commandType)
        {
            bool forceNewConnection = false;
            return Operation(commandText, toDb, commandType, forceNewConnection);
        }

        /// <summary>
        /// Provides a database operation.
        /// </summary>
        /// <param name="commandText">The stored procedure to use.</param>
        /// <param name="toDb">The method for setting the parameters for the stored procedure.</param>
        /// <param name="commandType">Type of the command.</param>
        /// <param name="forceNewConnection">if set to <c>true</c> [force new connection].</param>
        /// <returns>
        /// The data reader from the stored procedure.
        /// </returns>
        protected virtual DbDataReader Operation(string commandText, SetParameterDelegate toDb, CommandType commandType, bool forceNewConnection)
        {
            return Operation(commandText, default(T), toDb, commandType, forceNewConnection);
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

        /// <summary>
        /// Provides a database operation.
        /// </summary>
        /// <param name="commandText">The stored procedure to use.</param>
        /// <param name="parameters">The parameters.</param>
        /// <param name="commandType">Type of the command.</param>
        /// <param name="forceNewConnection">if set to <c>true</c> [force new connection].</param>
        /// <returns>
        /// The data reader from the stored procedure.
        /// </returns>
        protected virtual DbDataReader Operation(string commandText, ListDictionary parameters, CommandType commandType, bool forceNewConnection)
        {
            SetParameterDelegate del = delegate(T t, ref DbCommand command)
            {
                foreach (DictionaryEntry de in parameters)
                    command.AddParameterWithValue((string)de.Key, de.Value);
            };

            return Operation(commandText, del, commandType, forceNewConnection);
        }

        /// <summary>
        /// Get a T from the data source.
        /// </summary>
        /// <param name="commandText">The stored procedure to use.</param>
        /// <param name="parameters">List of parameters, ([ParameterName], [Value]).</param>
        /// <param name="fromDb">The method for mapping the data to an object.</param>
        /// <returns>The requested T.</returns>
        protected virtual T Operation(string commandText, ListDictionary parameters, GetDataDelegate fromDb)
        {
            bool forceNewConnection = false;
            return Operation(commandText, parameters, fromDb, forceNewConnection);
        }

        /// <summary>
        /// Get a T from the data source.
        /// </summary>
        /// <param name="commandText">The stored procedure to use.</param>
        /// <param name="parameters">List of parameters, ([ParameterName], [Value]).</param>
        /// <param name="fromDb">The method for mapping the data to an object.</param>
        /// <param name="forceNewConnection">if set to <c>true</c> [force new connection].</param>
        /// <returns>The requested T.</returns>
        protected virtual T Operation(string commandText, ListDictionary parameters, GetDataDelegate fromDb, bool forceNewConnection)
        {
            SetParameterDelegate del = delegate(T t, ref DbCommand command)
            {
                foreach (DictionaryEntry de in parameters)
                    command.AddParameterWithValue((string)de.Key, de.Value);
            };

            return Operation(commandText, del, fromDb, forceNewConnection);
        }

        /// <summary>
        /// Get a T from the data source.
        /// </summary>
        /// <param name="commandText">The stored procedure to use.</param>
        /// <param name="data">The data to use for parameters.</param>
        /// <param name="toDb">The method for setting the parameters for the stored precedure.</param>
        /// <param name="fromDb">The method for mapping the data to an object.</param>
        /// <returns>The requested T.</returns>
        protected virtual T Operation(string commandText, T data, SetParameterDelegate toDb, GetDataDelegate fromDb)
        {
            bool forceNewConnection = false;
            return Operation(commandText, data, toDb, fromDb, forceNewConnection);
        }

        /// <summary>
        /// Get a T from the data source.
        /// </summary>
        /// <param name="commandText">The stored procedure to use.</param>
        /// <param name="data">The data to use for parameters.</param>
        /// <param name="toDb">The method for setting the parameters for the stored precedure.</param>
        /// <param name="fromDb">The method for mapping the data to an object.</param>
        /// <param name="forceNewConnection">if set to <c>true</c> [force new connection].</param>
        /// <returns>The requested T.</returns>
        protected virtual T Operation(string commandText, T data, SetParameterDelegate toDb, GetDataDelegate fromDb, bool forceNewConnection)
        {
            DbDataReader dr = Operation(commandText, data, toDb, forceNewConnection);
            if (dr != null)
            {
                if (dr.Read())
                {
                    T retval = fromDb(dr);
                    dr.Close();
                    return retval;
                }

                CleanUp(dr);
            }

            return default(T);
        }

        /// <summary>
        /// Get a T from the data source.
        /// </summary>
        /// <param name="commandText">The stored procedure to use.</param>
        /// <param name="fromDb">The method for mapping the data to an object.</param>
        /// <returns>The requested T.</returns>
        protected virtual T Operation(string commandText, GetDataDelegate fromDb)
        {
            bool forceNewConnection = false;
            return Operation(commandText, fromDb, forceNewConnection);
        }

        /// <summary>
        /// Get a T from the data source.
        /// </summary>
        /// <param name="commandText">The stored procedure to use.</param>
        /// <param name="fromDb">The method for mapping the data to an object.</param>
        /// <param name="forceNewConnection">if set to <c>true</c> [force new connection].</param>
        /// <returns>The requested T.</returns>
        protected virtual T Operation(string commandText, GetDataDelegate fromDb, bool forceNewConnection)
        {
            DbDataReader dr = Operation(commandText, forceNewConnection);
            if (dr != null)
            {
                if (dr.Read())
                {
                    T retval = fromDb(dr);
                    dr.Close();
                    return retval;
                }

                CleanUp(dr);
            }

            return default(T);
        }

        /// <summary>
        /// Get a T from the data source.
        /// </summary>
        /// <param name="commandText">The stored procedure to use.</param>
        /// <param name="toDb">The method for setting the parameters for the stored precedure.</param>
        /// <param name="fromDb">The method for mapping the data to an object.</param>
        /// <returns>The requested T.</returns>
        protected virtual T Operation(string commandText, SetParameterDelegate toDb, GetDataDelegate fromDb)
        {
            bool forceNewConnection = false;
            return Operation(commandText, toDb, fromDb, forceNewConnection);
        }

        #endregion Operation

        /// <summary>
        /// Get a T from the data source.
        /// </summary>
        /// <param name="commandText">The stored procedure to use.</param>
        /// <param name="toDb">The method for setting the parameters for the stored precedure.</param>
        /// <param name="fromDb">The method for mapping the data to an object.</param>
        /// <param name="forceNewConnection">if set to <c>true</c> [force new connection].</param>
        /// <returns>The requested T.</returns>
        protected virtual T Operation(string commandText, SetParameterDelegate toDb, GetDataDelegate fromDb, bool forceNewConnection)
        {
            return Operation(commandText, default(T), toDb, fromDb, forceNewConnection);
        }

        #region SOperation

        /// <summary>
        /// Ss the operation.
        /// </summary>
        /// <param name="command">The command.</param>
        /// <returns></returns>
        protected virtual DbDataReader SOperation(DbCommand command)
        {
            return SOperation(command, false);
        }

        /// <summary>
        /// Ss the operation.
        /// </summary>
        /// <param name="command">The command.</param>
        /// <param name="forceNewConnection">if set to <c>true</c> [force new connection].</param>
        /// <returns></returns>
        protected virtual DbDataReader SOperation(DbCommand command, bool forceNewConnection)
        {
            if (command.Connection == null)
            {
                var comm = ProfiledConnection.CreateCommand(command.CommandText, command.Transaction);
                command = comm;
            }
            var connection = command.Connection;
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
        /// Ss the operation.
        /// </summary>
        /// <param name="command">The command.</param>
        /// <param name="fromDb">From db.</param>
        /// <returns></returns>
        protected virtual T SOperation(DbCommand command, GetDataDelegate fromDb)
        {
            DbDataReader dr = SOperation(command);

            try
            {
                if (dr.Read())
                {
                    T retval = fromDb(dr);
                    return retval;
                }
            }
            finally
            {
                CleanUp(dr);
            }

            return default(T);
        }

        #endregion SOperation

        #region SOperationList

        /// <summary>
        /// Ss the operation list.
        /// </summary>
        /// <param name="command">The command.</param>
        /// <param name="fromDb">From db.</param>
        /// <returns></returns>
        protected virtual List<T> SOperationList(DbCommand command, GetDataDelegate fromDb)
        {
            List<T> retval = new List<T>();

            DbDataReader dr = SOperation(command);

            try
            {
                while (dr.Read())
                    retval.Add(fromDb(dr));
            }
            finally
            {
                CleanUp(dr);
            }

            return retval;
        }

        #endregion SOperationList

        /// <summary>
        /// Get a list of T from the data source.
        /// </summary>
        /// <param name="commandText">The stored procedure to use.</param>
        /// <param name="fromDb">The method for mapping the data to an object.</param>
        /// <returns>A list of Ts.</returns>
        protected virtual List<T> OperationList(string commandText, GetDataDelegate fromDb)
        {
            bool forceNewConnection = false;
            return OperationList(commandText, fromDb, forceNewConnection);
        }

        /// <summary>
        /// Get a list of T from the data source.
        /// </summary>
        /// <param name="commandText">The stored procedure to use.</param>
        /// <param name="fromDb">The method for mapping the data to an object.</param>
        /// <param name="forceNewConnection">if set to <c>true</c> [force new connection].</param>
        /// <returns>A list of Ts.</returns>
        protected virtual List<T> OperationList(string commandText, GetDataDelegate fromDb, bool forceNewConnection)
        {
            return OperationList(commandText, fromDb, CommandType.StoredProcedure, forceNewConnection);
        }

        /// <summary>
        /// Get a list of T from the data source.
        /// </summary>
        /// <param name="commandText">The stored procedure to use.</param>
        /// <param name="fromDb">The method for mapping the data to an object.</param>
        /// <param name="commandType">Type of the command.</param>
        /// <returns>A list of Ts.</returns>
        protected virtual List<T> OperationList(string commandText, GetDataDelegate fromDb, CommandType commandType)
        {
            bool forceNewConnection = false;
            return OperationList(commandText, fromDb, commandType, forceNewConnection);
        }

        /// <summary>
        /// Get a list of T from the data source.
        /// </summary>
        /// <param name="commandText">The stored procedure to use.</param>
        /// <param name="fromDb">The method for mapping the data to an object.</param>
        /// <param name="commandType">Type of the command.</param>
        /// <param name="forceNewConnection">if set to <c>true</c> [force new connection].</param>
        /// <returns>A list of Ts.</returns>
        protected virtual List<T> OperationList(string commandText, GetDataDelegate fromDb, CommandType commandType, bool forceNewConnection)
        {
            return OperationList(commandText, default(T), EmptyParams, fromDb, commandType, forceNewConnection);
        }

        /// <summary>
        /// Get a list of T from the data source.
        /// </summary>
        /// <param name="commandText">The stored procedure to use.</param>
        /// <param name="parameters">List of parameters, ("ParameterName","Value").</param>
        /// <param name="fromDb">The method for mapping the data to an object.</param>
        /// <returns>A list of Ts.</returns>
        protected virtual List<T> OperationList(string commandText, ListDictionary parameters, GetDataDelegate fromDb)
        {
            bool forceNewConnection = false;
            return OperationList(commandText, parameters, fromDb, forceNewConnection);
        }

        /// <summary>
        /// Get a list of T from the data source.
        /// </summary>
        /// <param name="commandText">The stored procedure to use.</param>
        /// <param name="parameters">List of parameters, ("ParameterName","Value").</param>
        /// <param name="fromDb">The method for mapping the data to an object.</param>
        /// <param name="forceNewConnection">if set to <c>true</c> [force new connection].</param>
        /// <returns>A list of Ts.</returns>
        protected virtual List<T> OperationList(string commandText, ListDictionary parameters, GetDataDelegate fromDb, bool forceNewConnection)
        {
            return OperationList(commandText, parameters, fromDb, CommandType.StoredProcedure, forceNewConnection);
        }

        /// <summary>
        /// Get a list of T from the data source.
        /// </summary>
        /// <param name="commandText">The stored procedure to use.</param>
        /// <param name="parameters">List of parameters, ("ParameterName","Value").</param>
        /// <param name="fromDb">The method for mapping the data to an object.</param>
        /// <param name="commandType">Type of the command.</param>
        /// <returns>A list of Ts.</returns>
        protected virtual List<T> OperationList(string commandText, ListDictionary parameters, GetDataDelegate fromDb, CommandType commandType)
        {
            bool forceNewConnection = false;
            return OperationList(commandText, parameters, fromDb, commandType, forceNewConnection);
        }

        /// <summary>
        /// Get a list of T from the data source.
        /// </summary>
        /// <param name="commandText">The stored procedure to use.</param>
        /// <param name="parameters">List of parameters, ("ParameterName","Value").</param>
        /// <param name="fromDb">The method for mapping the data to an object.</param>
        /// <param name="commandType">Type of the command.</param>
        /// <param name="forceNewConnection">if set to <c>true</c> [force new connection].</param>
        /// <returns>A list of Ts.</returns>
        protected virtual List<T> OperationList(string commandText, ListDictionary parameters, GetDataDelegate fromDb, CommandType commandType, bool forceNewConnection)
        {
            SetParameterDelegate del = delegate(T t, ref DbCommand command)
            {
                foreach (DictionaryEntry de in parameters)
                    command.AddParameterWithValue((string)de.Key, de.Value);
            };

            return OperationList(commandText, default(T), del, fromDb, commandType, forceNewConnection);
        }

        /// <summary>
        /// Get a list of T from the data source.
        /// </summary>
        /// <param name="commandText">The stored procedure to use.</param>
        /// <param name="data">The data to use for parameters.</param>
        /// <param name="toDb">The method for setting the parameters for the stored procedure.</param>
        /// <param name="fromDb">The method for mapping the data to an object.</param>
        /// <returns>A list of Ts.</returns>
        protected virtual List<T> OperationList(string commandText, T data, SetParameterDelegate toDb, GetDataDelegate fromDb)
        {
            bool forceNewConnection = false;
            return OperationList(commandText, data, toDb, fromDb, forceNewConnection);
        }

        /// <summary>
        /// Get a list of T from the data source.
        /// </summary>
        /// <param name="commandText">The stored procedure to use.</param>
        /// <param name="data">The data to use for parameters.</param>
        /// <param name="toDb">The method for setting the parameters for the stored procedure.</param>
        /// <param name="fromDb">The method for mapping the data to an object.</param>
        /// <param name="forceNewConnection">if set to <c>true</c> [force new connection].</param>
        /// <returns>A list of Ts.</returns>
        protected virtual List<T> OperationList(string commandText, T data, SetParameterDelegate toDb, GetDataDelegate fromDb, bool forceNewConnection)
        {
            return OperationList(commandText, data, toDb, fromDb, CommandType.StoredProcedure, forceNewConnection);
        }

        /// <summary>
        /// Get a list of T from the data source.
        /// </summary>
        /// <param name="commandText">The stored procedure to use.</param>
        /// <param name="data">The data to use for parameters.</param>
        /// <param name="toDb">The method for setting the parameters for the stored procedure.</param>
        /// <param name="fromDb">The method for mapping the data to an object.</param>
        /// <param name="commandType">Type of the command.</param>
        /// <returns>A list of Ts.</returns>
        protected virtual List<T> OperationList(string commandText, T data, SetParameterDelegate toDb, GetDataDelegate fromDb, CommandType commandType)
        {
            bool forceNewConnection = false;
            return OperationList(commandText, data, toDb, fromDb, commandType, forceNewConnection);
        }

        /// <summary>
        /// Get a list of T from the data source.
        /// </summary>
        /// <param name="commandText">The stored procedure to use.</param>
        /// <param name="data">The data to use for parameters.</param>
        /// <param name="toDb">The method for setting the parameters for the stored procedure.</param>
        /// <param name="fromDb">The method for mapping the data to an object.</param>
        /// <param name="commandType">Type of the command.</param>
        /// <param name="forceNewConnection">if set to <c>true</c> [force new connection].</param>
        /// <returns>A list of Ts.</returns>
        protected virtual List<T> OperationList(string commandText, T data, SetParameterDelegate toDb, GetDataDelegate fromDb, CommandType commandType, bool forceNewConnection)
        {
            List<T> list = new List<T>();

            DbDataReader dr = Operation(commandText, data, toDb, commandType, forceNewConnection);
            if (dr != null)
            {
                while (dr.Read())
                    list.Add(fromDb(dr));

                CleanUp(dr);

                return list;
            }

            return null;
        }

        /// <summary>
        /// Save data.
        /// </summary>
        /// <param name="data">Data to save.</param>
        /// <returns>Return the saved data, or null if save failed.</returns>
        public virtual T Save(T data) { return Save(data, null); }

        /// <summary>
        /// Saves the specified data.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="transaction">The transaction.</param>
        /// <returns></returns>
        public virtual T Save(T data, DbTransaction transaction)
        {
            string commandText = InsertCommand;
            _transaction = transaction;
            if (IsUpdatable(data)) commandText = UpdateCommand;

            T obj = Operation(commandText, data, ToDb, FromDb, false);

            return obj;
        }

        /// <summary>
        /// Saves the specified data, returning a boolean.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <returns></returns>
        public virtual bool BSave(T data) { return BSave(data, null); }

        /// <summary>
        /// Saves the specified data, returning a boolean.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="transaction">The transaction.</param>
        /// <returns></returns>
        public virtual bool BSave(T data, DbTransaction transaction)
        {
            bool retVal = false;
            try
            {
                Save(data, transaction);
                retVal = true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return retVal;
        }

        /// <summary>
        /// Ss the save.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <returns></returns>
        public virtual int SSave(T data)
        {
            History h = null;
            return SSave(data, ref h);
        }

        /// <summary>
        /// Ss the save.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="history">The history.</param>
        /// <returns></returns>
        public virtual int SSave(T data, ref History history)
        {
            return SSave(data, ref history, null);
        }

        /// <summary>
        /// Ss the save.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="transaction">The transaction.</param>
        /// <returns></returns>
        public virtual int SSave(T data, DbTransaction transaction)
        {
            History h = null;
            return SSave(data, ref h);
        }

        /// <summary>
        /// Ss the save.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="history">The history.</param>
        /// <param name="transaction">The transaction.</param>
        /// <returns></returns>
        public virtual int SSave(T data, ref History history, DbTransaction transaction)
        {
            int retVal = -1;
            _transaction = transaction;
            DbCommand command = ProfiledConnection.CreateCommand();

            SQLGenerator<T> generator = new SQLGenerator<T>(command, true);
            DbDataReader reader = null;

            try
            {
                IIdentifiable iiData = data as IIdentifiable;
                IControllable icData = data as IControllable;
                IAuditable iaData = data as IAuditable;

                if (iaData != null) //this record is auditable
                {
                    DateTime editedOnDefault = DateTime.Now;
                    string editedByDefault = Environment.UserName;

                    //If we have a history object...
                    if (history != null)
                    {
                        //Check that a valid value has been provided for the history's createdOn property
                        if (history.CreatedOn < SqlDateTime.MinValue.Value)
                            history.CreatedOn = editedOnDefault;

                        //Check that a valid value has been provided for the history's createdBy property
                        if (history.CreatedBy == null || history.CreatedBy == string.Empty)
                            history.CreatedBy = editedByDefault;

                        //If the change happened chronologically after the last time the data was edited...
                        if (history.CreatedOn > iaData.EditedOn)
                        {
                            //Set the admin fields to be the ones provided by the history object
                            iaData.EditedOn = history.CreatedOn;
                            iaData.EditedBy = history.CreatedBy;
                        }
                    }
                    //Otherwise set the admin values to be the defaults
                    else
                    {
                        iaData.EditedOn = editedOnDefault;
                        iaData.EditedBy = editedByDefault;
                    }
                }

                bool insertingId = false;
                if (InsertId)
                    insertingId = !SExists(iiData.Id);

                //updating
                if (IsUpdatable(data) && !insertingId)
                {
                    List<PropertyInfo> changedProperties = new List<PropertyInfo>();

                    if (history != null)
                    {
                        bool insert = (history.Type == ChangeType.Insert);
                        if (history.Type != ChangeType.Delete && history.Type != ChangeType.Insert)
                            history.Type = ChangeType.Update;
                        if (iiData != null)
                            history.RecordId = iiData.Id;

                        changedProperties = ManageSaveChanges(data, history, insert);

                        //If we have nothing to save then we don't want to make a database save
                        if (changedProperties.Count <= 0)
                            return iiData.Id;
                    }

                    command.CommandText = generator.GenerateUpdate(DatabaseTableName, data, ToDb, IdColumn, changedProperties);
                }
                else //going to insert
                {
                    if (iaData != null)
                    {
                        //If there is no history object or the history's created on date hasn't been set...
                        if (history == null || history.CreatedOn < SqlDateTime.MinValue.Value)
                            iaData.CreatedOn = DateTime.Now;
                        else
                            iaData.CreatedOn = history.CreatedOn;

                        if (iaData.CreatedBy == null)
                            iaData.CreatedBy = iaData.EditedBy;
                        else if (iaData.CreatedBy == string.Empty)
                            iaData.CreatedBy = iaData.EditedBy;
                    }
                    generator.InsertId = InsertId;

                    command.CommandText = generator.GenerateInsert(DatabaseTableName, data, ToDb, IdColumn);
                }
                //REMOVE THESE COMMENTED LINES
                //Exception ex2 = new DataOperationException(command, new Exception());

                //CBRI.ErrorProcessing.ExceptionHandlers.ExceptionHandler(ref ex2, "CBRI.Database", "0.1", "exchange.campden.co.uk", "", "a.allen@campden.co.uk", true, "SSave", "BaseDataController");

                reader = SOperation(command);

                if (reader.Read())
                    retVal = Convert.ToInt32(reader[0]);

                //inserting
                if (!IsUpdatable(data) || insertingId)
                {
                    if (iaData != null)
                        iaData.Id = retVal;

                    if (history != null)
                    {
                        history.Type = ChangeType.Insert;
                        history.RecordId = iaData.Id;

                        ManageSaveChanges(data, history, true);
                    }
                }
                else
                {
                    retVal = iiData.Id;
                }
            }
            finally
            {
                CleanUp(reader);
            }

            return retVal;
        }

        /// <summary>
        /// Manages the save changes.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="history">The history.</param>
        /// <param name="insert">if set to <c>true</c> [insert].</param>
        /// <returns></returns>
        protected virtual List<PropertyInfo> ManageSaveChanges(T data, History history, bool insert)
        {
            IAuditable iData = data as IAuditable;

            if (iData == null)  //we can only manage auditable types
                return new List<PropertyInfo>();

            T currentData = insert ? default(T) : SGet(iData.Id, true);

            IAuditable iOldData = currentData as IAuditable;

            List<PropertyInfo> changedProperties = new List<PropertyInfo>();
            if (insert) //this is new data
            {
                changedProperties = new List<PropertyInfo>(typeof(T).GetProperties(BindingFlags.Instance | BindingFlags.Public));
            }
            else //updating
            {
                //these properties are the same forever
                iData.CreatedBy = iOldData.CreatedBy;
                iData.CreatedOn = iOldData.CreatedOn;

                var comparer = new ObjectComparer<T>();
                var comparison = comparer.Compare(currentData, data);
                changedProperties = comparison.ChangedProperties;
            }

            List<HistoryColumn> columnhistories = CreateColumnHistories(currentData, data, changedProperties, history);
            if (columnhistories.Count > 0)
            {
                HistoryHandler handler = new HistoryHandler(this.Connection.ConnectionString);
                handler.DatabaseTableName = HistoryTableName;

                HistoryColumnHandler columnHandler = new HistoryColumnHandler(this.Connection.ConnectionString);
                columnHandler.DatabaseTableName = HistoryColumnTableName;

                if (history.CreatedOn == null || history.CreatedOn < SqlDateTime.MinValue.Value)
                    history.CreatedOn = DateTime.Now;
                history.Id = handler.SSave(history);
                history.Columns = columnhistories;

                foreach (HistoryColumn hc in columnhistories)
                {
                    hc.HistoryId = history.Id;
                    columnHandler.SSave(hc);
                }
            }
            else
            {
                //If there are no non-audit fields then any changedProperties elements will just be changes to audit
                //fields. If the audit fields are the the only fields that have changed then we shouldn't alter the
                //database record, therefore we should empty the changedProperties list.
                changedProperties.Clear();
            }

            return changedProperties;
        }

        /// <summary>
        /// Creates the column histories.
        /// </summary>
        /// <param name="oldData">The old data.</param>
        /// <param name="newData">The new data.</param>
        /// <param name="properties">The properties.</param>
        /// <param name="history">The history.</param>
        /// <returns></returns>
        protected virtual List<HistoryColumn> CreateColumnHistories(T oldData, T newData, List<PropertyInfo> properties, History history)
        {
            List<HistoryColumn> columnHistories = new List<HistoryColumn>();

            foreach (PropertyInfo property in properties)
            {
                bool auditable = true;
                string colName = property.Name;
                bool write = true;

                object[] attributes = property.GetCustomAttributes(typeof(ReflectORM.Attributes.ControllablePropertyAttribute), true);
                if (attributes.Length > 0) //it does have the attribute.
                {
                    ReflectORM.Attributes.ControllablePropertyAttribute fda = (ReflectORM.Attributes.ControllablePropertyAttribute)attributes[0];
                    auditable = fda.Auditable;
                    colName = fda.ColumnName ?? property.Name;
                    write = fda.Write;
                }
                if (auditable)
                {
                    if (property.CanRead)
                    {
                        if (write)
                        {
                            HistoryColumn hc = new HistoryColumn();

                            hc.HistoryId = history.Id;
                            hc.Column = colName;
                            if (oldData.IsNull())
                                hc.OldValue = null;
                            else
                            {
                                object oldValue = property.GetValue(oldData, null);
                                hc.OldValue = oldValue;
                            }

                            object value = property.GetValue(newData, null);
                            hc.NewValue = value;

                            columnHistories.Add(hc);
                        }
                    }
                }
            }

            return columnHistories;
        }

        /// <summary>
        /// Ss the save get.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <returns></returns>
        public virtual T SSaveGet(T data)
        {
            History h = null;
            return SSaveGet(data, ref h);
        }

        /// <summary>
        /// Ss the save get.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="history">The history.</param>
        /// <returns></returns>
        public virtual T SSaveGet(T data, ref History history)
        {
            return SSaveGet(data, ref history, null);
        }

        /// <summary>
        /// Ss the save get.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="history">The history.</param>
        /// <param name="transaction">The transaction.</param>
        /// <returns></returns>
        public virtual T SSaveGet(T data, ref History history, DbTransaction transaction)
        {
            return SGet(SSave(data, ref history, transaction));
        }

        /// <summary>
        /// Get data from an Id
        /// Begins the transaction.
        /// </summary>
        protected new DbTransaction BeginTransaction()
        {
            DbConnection connection = ProfiledConnection;
            if (connection.State == ConnectionState.Closed)
                connection.Open();

            return _transaction = connection.BeginTransaction();
        }

        /// <summary>
        /// Commits the transaction if one exists.
        /// </summary>
        protected new void CommitTransaction()
        {
            if (!_transaction.IsNull())
                _transaction.Commit();
        }

        /// <summary>
        /// Get data
        /// </summary>
        /// <param name="id">The id of the record you want to retrieve.</param>
        /// <returns>
        /// The requested object, or null if it doesn't exist.
        /// </returns>
        public virtual T Get(int id)
        {
            ListDictionary list = new ListDictionary();
            list.Add(string.Format("@{0}", IdColumn), id);

            return Operation(SelectCommand, list, FromDb, true);
        }

        /// <summary>
        /// Gets the reflected joins.
        /// </summary>
        /// <returns></returns>
        protected virtual List<Join> GetJoins()
        {
            return new List<Join>();
        }

        /// <summary>
        /// Ss the delete.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <returns></returns>
        public virtual bool SDelete(int id)
        {
            History h = null;
            return SDelete(id, ref h);
        }

        /// <summary>
        /// Ss the delete.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <param name="history">The history.</param>
        /// <returns></returns>
        public virtual bool SDelete(int id, ref History history)
        {
            Criteria criterion = new Criteria(IdColumn, ColumnType.Int, id);
            return SDeleteByCriteria(criterion, ref history);
        }

        /// <summary>
        /// Ss the delete by column.
        /// </summary>
        /// <param name="colValue">The col value.</param>
        /// <param name="column">The column.</param>
        /// <returns></returns>
        public virtual bool SDeleteByColumn(object colValue, string column)
        {
            History h = null;
            return SDeleteByColumn(colValue, column, ref h);
        }

        /// <summary>
        /// Ss the delete by column.
        /// </summary>
        /// <param name="colValue">The col value.</param>
        /// <param name="column">The column.</param>
        /// <param name="history">The history.</param>
        /// <returns></returns>
        public virtual bool SDeleteByColumn(object colValue, string column, ref History history)
        {
            var value = colValue.ToSafeString();

            if (colValue is DateTime)
            {
                var dateValue = (DateTime)colValue;
                value = dateValue.ToString("s");
            }

            var colCriteria = new Criteria(column, value);
            colCriteria.SearchType = SearchType.Equals;

            return SDeleteByCriteria(colCriteria, ref history);
        }

        /// <summary>
        /// Ss the delete by criteria.
        /// </summary>
        /// <param name="criterion">The criterion.</param>
        /// <param name="history">The history.</param>
        /// <returns></returns>
        public virtual bool SDeleteByCriteria(Criteria criterion, ref History history)
        {
            if (history == null || (!(typeof(T).IsSubclassOf(typeof(Auditable)))))
            {
                DbCommand command = ProfiledConnection.CreateCommand();

                SQLGenerator<T> generator = new SQLGenerator<T>(command);
                generator.Criteria.Add(criterion);

                command.CommandText = generator.GenerateDelete(DatabaseTableName);

                return (SOperation(command, FromDb) == null);
            }
            else
            {
                History lastHistory = null;
                history.Type = ChangeType.Delete;
                List<T> deleteList = SGetByCriteria(criterion);
                foreach (IAuditable data in deleteList)
                {
                    History dataHistory = new History()
                    {
                        Comment = history.Comment,
                        Committed = history.Committed,
                        CreatedBy = history.CreatedBy,
                        CreatedOn = history.CreatedOn,
                        Published = history.Published,
                        Type = history.Type
                    };
                    data.Deleted = true;
                    SSave((T)data, ref dataHistory);
                    lastHistory = dataHistory;
                }
                history = lastHistory;
            }
            return true;
        }

        /// <summary>
        /// Checks whether the record exists, from the id, by generating SQL
        /// </summary>
        /// <param name="id">The id.</param>
        /// <returns></returns>
        public virtual bool SExists(int id)
        {
            return SExists(id, false);
        }

        /// <summary>
        /// Checks whether the record exists, from the id, by generating SQL
        /// </summary>
        /// <param name="id">The id.</param>
        /// <param name="ignoreDeletedStatus">if set to <c>true</c> [ignore deleted status].</param>
        /// <returns></returns>
        public virtual bool SExists(int id, bool ignoreDeletedStatus)
        {
            return SExistsForColumn(id, IdColumn, ignoreDeletedStatus);
        }

        /// <summary>
        /// Checks whether the record exists, for a column, by generating SQL
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="column">The column.</param>
        /// <returns></returns>
        public virtual bool SExistsForColumn(object value, string column)
        {
            return SExistsForColumn(value, column, false);
        }

        /// <summary>
        /// Checks whether the record exists, for a column, by generating SQL
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="column">The column.</param>
        /// <param name="ignoreDeletedStatus">if set to <c>true</c> [ignore deleted status].</param>
        /// <returns></returns>
        public virtual bool SExistsForColumn(object value, string column, bool ignoreDeletedStatus)
        {
            var criteria = new Criteria(column, ColumnType.Auto, value);
            criteria.SearchType = SearchType.Equals;

            return SExistsForCriteria(new List<Criteria>() { criteria }, ignoreDeletedStatus);
        }

        /// <summary>
        /// Checks whether the record exists, from a list of criteria, by generating SQL
        /// </summary>
        /// <param name="criteria">The criteria.</param>
        /// <param name="ignoreDeletedStatus">if set to <c>true</c> [ignore deleted status].</param>
        /// <returns></returns>
        public virtual bool SExistsForCriteria(IEnumerable<Criteria> criteria, bool ignoreDeletedStatus)
        {
            var comm = ProfiledConnection.CreateCommand();
            var generator = new SQLGenerator<T>(comm, ignoreDeletedStatus);

            generator.Criteria.AddRange(criteria);
            generator.Joins.AddRange(GetJoins());
            comm.CommandText = generator.GenerateSelect(SelectViewName, 1, IdColumn);

            var reader = SOperation(comm);
            try
            {
                if (reader == null)
                    return false;

                return reader.HasRows;
            }
            finally
            {
                CleanUp(reader);
            }
        }

        /// <summary>
        /// Ss the get.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <returns></returns>
        public virtual T SGet(int id)
        {
            return SGet(id, false);
        }

        /// <summary>
        /// Ss the get.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <param name="ignoreDeletedStatus">if set to <c>true</c> [ignore deleted status].</param>
        /// <returns></returns>
        public virtual T SGet(int id, bool ignoreDeletedStatus)
        {
            DbCommand command = ProfiledConnection.CreateCommand();

            Criteria criterion = new Criteria(IdColumn, ColumnType.Int, id);
            criterion.SearchType = SearchType.Equals;

            SQLGenerator<T> generator = new SQLGenerator<T>(command, ignoreDeletedStatus);
            generator.Criteria.Add(criterion);
            generator.Joins.AddRange(GetJoins());

            command.CommandText = generator.GenerateSelect(SelectViewName);

            return SOperation(command, FromDb);
        }

        /// <summary>
        /// Gets the deleted record.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <returns>Returns the record if it is deleted, otherwise returns null.</returns>
        public virtual T SGetDeleted(int id)
        {
            DbCommand command = ProfiledConnection.CreateCommand();

            Criteria idCrit = new Criteria(IdColumn, ColumnType.Int, id);
            idCrit.SearchType = SearchType.Equals;

            Criteria delCrit = new Criteria("Deleted", ColumnType.Bool, true);
            delCrit.SearchType = SearchType.Equals;

            SQLGenerator<T> generator = new SQLGenerator<T>(command, true);
            generator.Criteria.Add(idCrit);
            generator.Criteria.Add(delCrit);
            generator.Joins.AddRange(GetJoins());

            command.CommandText = generator.GenerateSelect(SelectViewName);

            return SOperation(command, FromDb);
        }

        /// <summary>
        /// Get's the latest history column record
        /// </summary>
        /// <param name="id">The id.</param>
        /// <param name="columnName">Name of the column.</param>
        /// <returns></returns>
        public virtual History SGetLatestHistoryColumn(int id, string columnName)
        {
            HistoryHandler hc = new HistoryHandler(this.Connection.ConnectionString);

            return hc.GetLatestRecord(id, HistoryTableName, HistoryColumnTableName, columnName);
        }

        /// <summary>
        /// Get all using SQL.
        /// </summary>
        /// <returns></returns>
        public virtual IList<T> SGetAll()
        {
            return SGetAll(false);
        }

        /// <summary>
        /// Get all using SQL.
        /// </summary>
        /// <returns></returns>
        public virtual IList<T> SGetAll(bool sorted)
        {
            return SGetAll(sorted, null);
        }

        /// <summary>
        /// Get all using SQL.
        /// </summary>
        /// <returns></returns>
        public virtual IList<T> SGetAll(bool sorted, IEnumerable<Criteria> criteria)
        {
            DbCommand command = ProfiledConnection.CreateCommand();

            SQLGenerator<T> generator = new SQLGenerator<T>(command);
            if (sorted)
                generator.OrderBy.Add(SortColumn);
            if (criteria != null && !criteria.IsEmpty())
                generator.Criteria.AddRange(criteria);

            DbUtilities.ValidateSearchCriteria(generator.Criteria);

            generator.Joins.AddRange(GetJoins());

            command.CommandText = generator.GenerateSelect(SelectViewName);

            return SOperationList(command, FromDb);
        }

        /// <summary>
        /// Check to see if a record exists.
        /// </summary>
        /// <param name="id">The id of the record you want to check.</param>
        /// <returns>True if the record exists, else false.</returns>
        public virtual bool Exists(int id)
        {
            ListDictionary list = new ListDictionary();
            list.Add(string.Format("@{0}", IdColumn), id);

            DbDataReader reader = Operation(SelectCommand, list, _transaction == null);
            bool exists = reader.HasRows;

            CleanUp(reader);

            return exists;
        }

        /// <summary>
        /// Delete data.
        /// </summary>
        /// <param name="id">The id of the record you want to delete.</param>
        /// <returns>Returns whether or not the record has been deleted.</returns>
        /// <remarks>Checks if the reader has any rows, so your stored proc should do a basic select of the id that is being deleted.</remarks>
        public virtual bool Delete(int id) { return Delete(id, null); }

        /// <summary>
        /// Delete data.
        /// </summary>
        /// <param name="id">The id of the record you want to delete.</param>
        /// <param name="transaction">The transaction.</param>
        /// <returns>
        /// Returns whether or not the record has been deleted.
        /// </returns>
        /// <remarks>Checks if the reader has any rows, so your stored proc should do a basic select of the id that is being deleted.</remarks>
        public virtual bool Delete(int id, DbTransaction transaction)
        {
            _transaction = transaction;
            ListDictionary list = new ListDictionary();
            list.Add(string.Format("@{0}", IdColumn), id);

            CleanUp(Operation(DeleteCommand, list, false));

            return !Exists(id);
        }

        /// <summary>
        /// Get all data.
        /// </summary>
        /// <returns>All of this type.</returns>
        public virtual List<T> GetAll()
        {
            ListDictionary list = new ListDictionary();
            list.Add(string.Format("@{0}", IdColumn), DBNull.Value);

            return OperationList(SelectCommand, list, FromDb, true);
        }

        /// <summary>
        /// Get common records
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="column">The column.</param>
        /// <returns></returns>
        public virtual List<T> SGetCommon(T data, string column)
        {
            Type type = typeof(T);
            PropertyInfo[] properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public);

            foreach (PropertyInfo property in properties)
            {
                if (property.Name != column)
                    continue;

                if (property.CanRead)
                {
                    object[] attributes = property.GetCustomAttributes(typeof(ReflectORM.Attributes.ControllablePropertyAttribute), true);

                    string colName = property.Name;
                    if (attributes.Length > 0) //it does have the attribute.
                    {
                        ReflectORM.Attributes.ControllablePropertyAttribute fda = (ReflectORM.Attributes.ControllablePropertyAttribute)attributes[0];
                        colName = fda.ColumnName ?? property.Name;
                    }

                    object actualValue = property.GetValue(data, null);

                    return SGetForColumn(actualValue, colName);
                }
            }

            throw new InvalidOperationException(string.Format("Invalid column {0}", column));
        }

        /// <summary>
        /// Get all records with a column with a specified value
        /// </summary>
        /// <param name="colValue">The col value.</param>
        /// <param name="column">The column.</param>
        /// <returns></returns>
        public List<T> SGetForColumn(object colValue, string column)
        {
            return SGetForColumn(colValue, column, false);
        }

        /// <summary>
        /// Get all records with a column with a specified value
        /// </summary>
        /// <param name="colValue">The col value.</param>
        /// <param name="column">The column.</param>
        /// <param name="sorted">if set to <c>true</c> [sorted].</param>
        /// <returns></returns>
        public List<T> SGetForColumn(object colValue, string column, bool sorted)
        {
            return SGetForColumn(colValue, column, false, false);
        }

        /// <summary>
        /// Get all records with a column with a specified value
        /// </summary>
        /// <param name="colValue">The col value.</param>
        /// <param name="column">The column.</param>
        /// <param name="sorted">if set to <c>true</c> [sorted].</param>
        /// <param name="ignoreDeletedStatus">if set to <c>true</c> [ignore deleted status].</param>
        /// <returns></returns>
        public List<T> SGetForColumn(object colValue, string column, bool sorted, bool ignoreDeletedStatus)
        {
            var value = colValue;

            if (colValue is string)
                value = colValue.ToSafeString();

            if (colValue is DateTime)
            {
                var dateValue = (DateTime)colValue;
                value = dateValue.ToString("s");
            }

            var colCriteria = new Criteria(column, value);
            colCriteria.SearchType = SearchType.Equals;

            if (colValue is int)
                colCriteria.Type = ColumnType.Int;

            if (colValue is bool)
                colCriteria.Type = ColumnType.Bool;

            return SGetByCriteria(colCriteria, sorted, ignoreDeletedStatus);
        }

        /// <summary>
        /// Get by Criteria using Generated SQL
        /// </summary>
        /// <param name="criterion">The criterion.</param>
        /// <returns></returns>
        public List<T> SGetByCriteria(Criteria criterion)
        {
            return SGetByCriteria(criterion, false);
        }

        /// <summary>
        /// Get by Criteria using Generated SQL
        /// </summary>
        /// <param name="criterion">The criterion.</param>
        /// <param name="sorted">if set to <c>true</c> [sorted].</param>
        /// <returns></returns>
        public List<T> SGetByCriteria(Criteria criterion, bool sorted)
        {
            return SGetByCriteria(criterion, false, false);
        }

        /// <summary>
        /// Get by Criteria using Generated SQL
        /// </summary>
        /// <param name="criterion">The criterion.</param>
        /// <param name="sorted">if set to <c>true</c> [sorted].</param>
        /// <param name="ignoreDeletedStatus">if set to <c>true</c> [ignore deleted status].</param>
        /// <returns></returns>
        public List<T> SGetByCriteria(Criteria criterion, bool sorted, bool ignoreDeletedStatus)
        {
            DbCommand command = ProfiledConnection.CreateCommand();

            SQLGenerator<T> generator = new SQLGenerator<T>(command, ignoreDeletedStatus);

            generator.Joins.AddRange(GetJoins());
            generator.Criteria.Add(criterion);

            if (sorted)
                generator.OrderBy.Add(SortColumn);

            command.CommandText = generator.GenerateSelect(SelectViewName);

            return SOperationList(command, FromDb);
        }

        /// <summary>
        /// Gets the record count.
        /// </summary>
        /// <returns></returns>
        public int GetRecordCount()
        {
            var command = Connection.CreateCommand();
            var generator = new SQLGenerator<T>(command);

            command.CommandText = generator.GenerateCount(SelectViewName, IdColumn);

            var reader = SOperation(command, true);

            int count = 0;
            try
            {
                if (reader.Read())
                    count = (int)reader[0];

                CleanUp(reader);
            }
            catch (Exception ex)
            {
                RaiseErrorHandler(ex);
            }

            return count;
        }

        /// <summary>
        /// Adds parameters to the command from the T.
        /// </summary>
        /// <param name="data">The data to get the parameters.</param>
        /// <param name="command">The command object to add parameters to.</param>
        protected virtual void ToDb(T data, ref DbCommand command)
        {
            var type = typeof(T);
            var properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public);

            var sb = new StringBuilder();

            foreach (PropertyInfo property in properties)
            {
                if (property.CanRead)
                {
                    var attributes = property.GetCustomAttributes(typeof(Attributes.ControllablePropertyAttribute), true);

                    var colName = property.Name;
                    var paramName = property.Name;
                    var delimiter = char.MinValue;
                    var write = true;
                    if (attributes.Length > 0) //it does have the attribute.
                    {
                        var fda = (Attributes.ControllablePropertyAttribute)attributes[0];
                        colName = fda.ColumnName ?? property.Name;
                        paramName = fda.ParameterName ?? colName;
                        delimiter = fda.StringSplitDelimiter;
                        write = fda.Write;
                    }

                    //don't add the Id column if we're inserting
                    if (!InsertId && !IsUpdatable(data) && colName.ToLower() == IdColumn.ToLower())
                        continue;

                    if (write)
                    {
                        var value = property.GetValue(data, null);

                        if (property.PropertyType.IsEnum)
                            value = value.ToString();

                        if (delimiter != char.MinValue)
                            if (value is IList)
                            {
                                var newVal = string.Empty;
                                var values = (IList<string>)value;
                                newVal = values.Aggregate(newVal, (current, s) => string.Format("{0}{1}", current, s));

                                value = newVal;
                            }

                        if (!paramName.StartsWith("@"))
                            paramName = string.Format("@{0}", paramName);   //prefix the name with the required @
                        if (value == null)
                            value = DBNull.Value;
                        command.AddParameterWithValue(paramName, value);
                    }
                }
            }
        }

        /// <summary>
        /// Gets whether or not the record is updatable or insertable.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <returns></returns>
        protected virtual bool IsUpdatable(T data)
        {
            var idable = data as IIdentifiable;

            if (idable != null)
                return idable.Id != DbConstants.DEFAULT_ID;

            return false;
        }

        /// <summary>
        /// Map data from the reader to T.
        /// </summary>
        /// <param name="reader">The reader to map from.</param>
        /// <returns>A T from the reader.</returns>
        public virtual T FromDb(IDataRecord reader)
        {
            object id = null;

            Type type = typeof(T);
            T variable = System.Activator.CreateInstance<T>();
            PropertyInfo[] properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public);

            StringBuilder sb = new StringBuilder();

            foreach (PropertyInfo property in properties)
            {
                if (property.CanWrite)
                {
                    object[] attributes = property.GetCustomAttributes(typeof(ReflectORM.Attributes.ControllablePropertyAttribute), true);

                    string colName = property.Name;
                    char delimiter = char.MinValue;
                    Type enumType = null;
                    bool read = true;
                    if (attributes.Length > 0) //it does have the attribute.
                    {
                        ReflectORM.Attributes.ControllablePropertyAttribute fda = (ReflectORM.Attributes.ControllablePropertyAttribute)attributes[0];
                        colName = fda.ColumnName ?? property.Name;
                        delimiter = fda.StringSplitDelimiter != char.MinValue ? fda.StringSplitDelimiter : char.MinValue;
                        enumType = fda.EnumType;
                        read = fda.Read;
                    }

                    if (reader.FieldExists(colName) && read)
                    {
                        object value = reader[colName];

                        if (value == DBNull.Value)
                        {
                            if (property.PropertyType == typeof(DateTime))
                                value = SqlDateTime.MinValue.Value;
                            else
                                value = null;
                        }

                        if (delimiter != char.MinValue)
                            value = new List<string>(((string)value).Split(delimiter));

                        if (property.PropertyType.IsEnum)
                            if (Enum.IsDefined(property.PropertyType, (string)value))
                                value = Enum.Parse(property.PropertyType, (string)value, true);

                        try
                        {
                            property.SetValue(variable, value, null);

                            if (colName == IdColumn) //this is the idProperty
                                id = value;
                        }
                        catch (TargetException) { }  //if we can't set the property, just leave it empty
                        catch (ArgumentException) { }  //if we can't set the property, just leave it empty
                    }
                }
            }

            return variable;
        }

        /// <summary>
        /// Map data from the row to T.
        /// </summary>
        /// <param name="row">The row to map from.</param>
        /// <returns>A T from the row.</returns>
        public virtual T FromDb(DataRow row)
        {
            return FromDb(new DataRowAdapter(row));
        }

        /// <summary>
        /// An instance that inherits from this class must provide a cloning mechanism.
        /// </summary>
        /// <returns>A copy of the cloned object.</returns>
        public override object Clone()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Creates a XML structured object.
        /// </summary>
        /// <param name="collectionItems"></param>
        /// <param name="collection"></param>
        /// <param name="collectionName"></param>
        /// <returns>SqlXml</returns>
        public virtual SqlXml ConvertCollectionToXml<Y>(Dictionary<string, string> collectionItems, IList<Y> collection, string collectionName)
        {
            MemoryStream stream = new MemoryStream();
            StringBuilder builder = new StringBuilder();
            using (XmlWriter writer = XmlWriter.Create(stream))
            {
                writer.WriteStartElement(collectionName);
                foreach (Y t in collection)
                {
                    Dictionary<string, string> d = PropertiesToXML(t);
                    writer.WriteStartElement("row");
                    foreach (string propertyName in collectionItems.Keys)
                    {
                        string value = propertyName;
                        string par = collectionItems[propertyName];
                        writer.WriteAttributeString(par, value);
                    }
                    foreach (string propertyName in d.Keys)
                    {
                        string value = propertyName;
                        string par = d[propertyName];
                        writer.WriteAttributeString(par, value);
                    }
                    writer.WriteEndElement();
                }
                writer.WriteEndElement();
            }
            return new SqlXml(stream);
        }

        //Needs to be abstract
        /// <summary>
        /// Propertieses to XML.
        /// </summary>
        /// <param name="t">The t.</param>
        /// <returns></returns>
        public virtual Dictionary<string, string> PropertiesToXML(object t)
        {
            return new Dictionary<string, string>();
        }
    }
}