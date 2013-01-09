using System;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Data.Common;
using ReflectORM.Extensions;

namespace ReflectORM.Core
{
    /// <summary>
    /// Represents the foundation of all database access components. It should be used to connect to SQL Server.  Any common code that gets performed
    /// on SQLServer should be added here.  For example this class could contain common functionallity to execute queries.
    /// </summary>
    public abstract class ApplicationDatabase : IDisposable, ICloneable
    {
        /// <summary>
        /// The current SQL connection in use
        /// </summary>
        protected SqlConnection _connection;						// This is the main connection that will be used to connect to the client database
        private DatabaseErrorEventHandler _onDatabaseError;
        
        /// <summary>
        /// The current transaction
        /// </summary>
        protected DbTransaction _transaction;

        /// <summary>
        /// Whether or not the connection property will return a new connection or the current connection
        /// </summary>
        protected bool _multipleConnections = false;

        /// <summary>
        /// The connection string.
        /// </summary>
        protected string _connectionString = string.Empty;

        /// <summary>
        /// The constructor. Initialises the connection to the Client Database System.
        /// </summary>
        /// <param name="connectionString">The connection string.</param>
        public ApplicationDatabase(string connectionString)
        {
            _connectionString = connectionString;
        }

        /// <summary>
        /// Event that gets fired each time there is an error caused by any of the database operations.
        /// </summary>
        public event DatabaseErrorEventHandler OnDatabaseError
        {
            add { _onDatabaseError += value; }
            remove { _onDatabaseError -= value; }
        }

        /// <summary>
        /// Exposes the connection property.
        /// </summary>
        public virtual SqlConnection Connection
        {
            get
            {
                if (_multipleConnections)
                    return new SqlConnection(_connectionString);

                if (_connection == null)
                    _connection = new SqlConnection(_connectionString);

                return _connection;
            }

            protected set
            {
                _connection = value;
            }
        }

        /// <summary>
        /// Gets the profiled connection.
        /// </summary>
        /// <value>The profiled connection.</value>
        public virtual DbConnection ProfiledConnection
        {
            get
            {
                return Connection;
            }
        }

        /// <summary>
        /// Exposes the current transaction
        /// </summary>
        public SqlTransaction CurrentTransaction
        {
            get { if (_transaction is SqlTransaction) return (SqlTransaction)_transaction; return null; }
            set { _transaction = value; }
        }

        /// <summary>
        /// Opens the current connection for use
        /// </summary>
        public void OpenConnection(DbConnection connection)
        {
            if (connection.State != ConnectionState.Open)
            {
                connection.Open();
            }
        }

        /// <summary>
        /// Opens the connection.
        /// </summary>
        public void OpenConnection()
        {
            OpenConnection(_connection);
        }

        /// <summary>
        /// Begins a transaction
        /// </summary>
        public bool BeginTransaction()
        {
            return BeginTransaction(_connection);
        }

        /// <summary>
        /// Begins a transaction
        /// </summary>
        public bool BeginTransaction(SqlConnection connection)
        {
            bool retVal = false;

            try
            {
                OpenConnection(connection);
                _transaction = connection.BeginTransaction();
                retVal = true;
            }
            catch (Exception ex)
            {
                RaiseErrorHandler(ex);
            }
            return retVal;
        }

        /// <summary>
        /// Commits the current transaction
        /// </summary>
        /// <returns>True if commited successfully, else false</returns>
        public bool CommitTransaction()
        {
            bool retVal = false;
            try
            {
                if (_transaction != null)
                {
                    _transaction.Commit();
                    _transaction.Dispose();
                    _transaction = null;
                    retVal = true;
                }
            }
            catch (Exception ex)
            {
                RaiseErrorHandler(ex);
            }
            return retVal;
        }

        /// <summary>
        /// Rolls back the current transaction
        /// </summary>
        /// <returns>True if rolled back successfully, else false</returns>
        public bool RollbackTransaction()
        {
            bool retVal = false;
            try
            {
                if (_transaction != null)
                {
                    _transaction.Rollback();
                    _transaction.Dispose();
                    _transaction = null;
                }
                retVal = true;
            }
            catch (Exception ex)
            {
                RaiseErrorHandler(ex);
            }
            return retVal;
        }

        /// <summary>
        /// Closes the current connection and disposes of the reader passed in
        /// </summary>
        /// <param name="reader">The reader object to dispose of (if there is one, else pass 'null')</param>
        public void CleanUp(DbDataReader reader)
        {
            if (_connection != null && _connection.State == ConnectionState.Open && _transaction == null)
                _connection.Close();

            if (reader != null)
            {
                if (!reader.IsClosed)
                    reader.Close();

                reader.Dispose();
            }           
        }
        
        /// <summary>
        /// Executes a command object and returns a reader
        /// </summary>
        /// <param name="comm">The command object to return</param>
        /// <returns>DbDataReader with the results</returns>
        /// <history>
        ///     [coullm]    22/07/09    Added functionality to throw a data operation exception
        /// </history>
        public virtual DbDataReader ExecuteReader(DbCommand comm)
        {
            DbDataReader reader = null;
            CommandBehavior behavior = CommandBehavior.Default;

            if (_multipleConnections)
                behavior = CommandBehavior.CloseConnection;

            OpenConnection(comm.Connection);
            reader = comm.ExecuteReader(behavior);
            return reader;
        }

        /// <summary>
        /// Called when a Database Error has occured.
        /// </summary>
        /// <param name="args">The details associated with the error.</param>
        protected void RaiseErrorHandler(DatabaseErrorEventArgs args)
        {
            if (_onDatabaseError != null)
            {
                _onDatabaseError(args);
            }
        }

        /// <summary>
        /// Called when a Database Error has occured.
        /// </summary>
        /// <param name="ex">The exception generated by the error.</param>
        protected void RaiseErrorHandler(Exception ex)
        {
            this.RaiseErrorHandler(new DatabaseErrorEventArgs(ex));
        }

        /// <summary>
        /// Sets the value of a specified parameter to DBDull.Value if the  value passed in is equivilent to DateTime.MinValue
        /// </summary>
        /// <param name="parameter">The parameter which will have its value set.</param>
        /// <param name="value">The value which determines whether the parameter value should be set to DBNull.Value.</param>
        protected void SetNullableParameter(SqlParameter parameter, DateTime value)
        {
            if (value <= SqlDateTime.MinValue.Value)
            {
                parameter.Value = DBNull.Value;
            }
            else
            {
                parameter.Value = value;
            }
        }

        /// <summary>
        /// Formats a date for SQL by checking if its between and not equal to DateMin and DateMax, if it is it returns DBNull, otherwise the date that was passed in
        /// </summary>
        /// <param name="date">DateTime object to evaluate</param>
        /// <returns>The Date passed in if the date is between and not equal to DateMin and DateMax, otherwise it returns DBNull</returns>
        public object FormatDateForSQL(DateTime date)
        {
            if (date.Year > SqlDateTime.MinValue.Value.Year && date.Year < SqlDateTime.MaxValue.Value.Year)
            {
                return date;
            }
            else
            {
                return DBNull.Value;
            }
        }

        /// <summary>
        /// Populates a DataTable by executing the specified SQL.
        /// </summary>
        /// <param name="table">The table which will be populated with result data.</param>
        /// <param name="SQL">The SQL query used to populate the table.</param>
        /// <returns>Returns true if the DataTable was sucessfully populated.</returns>
        /// <history>
        ///     [coullm]    22/07/09    Added functionality to throw a data operation exception
        /// </history>
        public virtual bool PopulateDataTable(DataTable table, string SQL)
        {
            bool result = false;
            SqlDataAdapter adapter = new SqlDataAdapter(SQL, (SqlConnection)this.Connection);
            try
            {
                this.Connection.Open();
                adapter.Fill(table);
                result = true;
            }
            finally
            {
                this.Connection.Close();
                if (adapter != null)
                {
                    adapter.Dispose();
                }
            }
            return result;
        }

        #region IDisposable Members

        /// <summary>
        /// Dispose the DbConnection used by this instance and frees any resources.
        /// </summary>
        public void Dispose()
        {
            if (_connection != null)
            {
                _connection.Dispose();
            }
        }

        #endregion

        #region ICloneable Members

        /// <summary>
        /// An instance that inherits from this class must provide a cloning mechanism.
        /// </summary>
        /// <returns>A copy of the cloned object.</returns>
        public abstract object Clone();

        #endregion
    }
}

