using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Reflection;
using System.Collections.Specialized;
using System.Data.SqlClient;
using System.Data;
using ReflectORM.Extensions;
using System.Data.Common;
using ReflectORM.Attributes;

namespace ReflectORM.Core
{
    /// <summary>
    /// Base Db Reader, provides base methods for accessing data from a database
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class BaseDbReader<T> : BaseDbOperator<T>
    {
        /// <summary>
        /// Object cache. Stores recently retrieved Ts
        /// </summary>
        protected Dictionary<object, T> _cache = new Dictionary<object, T>();

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseDbReader&lt;T&gt;"/> class.
        /// </summary>
        /// <param name="connectionString">The connection string.</param>
        public BaseDbReader(string connectionString) : base(connectionString) { }
       
        /// <summary>
        /// Map data from the reader to T.
        /// </summary>
        /// <param name="reader">The reader to map from.</param>
        /// <returns>A T from the reader.</returns>
        protected internal virtual T FromDb(IDataRecord reader)
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

                        if (delimiter != char.MinValue)
                            value = new List<string>(((string)value).Split(delimiter));

                        if (property.PropertyType.IsEnum)
                            value = Enum.Parse(property.PropertyType, (string)value, true);

                        try
                        {
                            property.SetValue(variable, value, null);

                            if (colName == IdColumn) //this is the idProperty
                                id = value;
                        }
                        catch (TargetException) { }  //if we can't set the property, just leave it empty
                    }
                }
            }

            if (id != null && variable != null)
                if (!_cache.ContainsKey(id))
                    _cache.Add(id, variable);

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
        /// Get data
        /// </summary>
        /// <param name="id">The id of the record you want to retrieve.</param>
        /// <returns>The requested object, or null if it doesn't exist.</returns>
        public abstract T Get(int id);

        /// <summary>
        /// Get all data.
        /// </summary>
        /// <returns>All of this type.</returns>
        public abstract List<T> GetAll();

        /// <summary>
        /// Check to see if a record exists.
        /// </summary>
        /// <param name="id">The id of the record you want to check.</param>
        /// <returns>True if the record exists, else false.</returns>
        public abstract bool Exists(int id);

        /// <summary>
        /// Gets the record count.
        /// </summary>
        /// <returns></returns>
        public virtual int GetRecordCount()
        {
            DbDataReader reader = Operation(string.Format("SELECT COUNT([{0}]) FROM [{1}]", IdColumn, DatabaseTableName), CommandType.Text, true);
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
        /// Get a T from the data source.
        /// </summary>
        /// <param name="commandText">The stored procedure to use.</param>
        /// <param name="parameters">List of parameters, ([ParameterName], [Value]).</param>
        /// <param name="fromDb">The method for mapping the data to an object.</param>
        /// <returns>The requested T.</returns>
        protected virtual T Operation(string commandText, ListDictionary parameters, GetDataDelegate<T> fromDb)
        {
            bool forceNewConnection = false;
            return Operation(commandText, parameters, fromDb, forceNewConnection);
        }

        /// <summary>
        /// Get a T from the data source.
        /// </summary>
        /// <param name="commandText">The stored procedure to use.</param>
        /// <param name="parameters">The parameters.</param>
        /// <param name="fromDb">The method for mapping the data to an object.</param>
        /// <param name="forceNewConnection">if set to <c>true</c> [force new connection].</param>
        /// <returns>The requested T.</returns>
        protected virtual T Operation(string commandText, ListDictionary parameters, GetDataDelegate<T> fromDb, bool forceNewConnection)
        {
            DbDataReader dr = Operation(commandText, parameters, forceNewConnection);
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
        protected virtual T Operation(string commandText, GetDataDelegate<T> fromDb)
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
        protected virtual T Operation(string commandText, GetDataDelegate<T> fromDb, bool forceNewConnection)
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
        /// Gets a T from the data source.
        /// </summary>
        /// <param name="command">The command to use.</param>
        /// <param name="fromDb">The method for mapping the data to an object.</param>
        /// <returns>The requested T.</returns>
        protected virtual T Operation(DbCommand command, GetDataDelegate<T> fromDb)
        {
            DbDataReader dr = Operation(command);

            if (dr.Read())
            {
                T retval = fromDb(dr);
                CleanUp(dr);
                return retval;
            }

            return default(T);
        }

        /// <summary>
        /// Get a list of T from the data source.
        /// </summary>
        /// <param name="command">The command to use.</param>
        /// <param name="fromDb">The method for mapping the data to an object.</param>
        /// <returns>A list of Ts</returns>
        protected virtual List<T> OperationList(DbCommand command, GetDataDelegate<T> fromDb)
        {
            DbDataReader dr = Operation(command);

            List<T> retVal = new List<T>();

            while (dr.Read())
            {
                T item = fromDb(dr);
                if (!item.IsNull())
                    retVal.Add(item);
            }
            CleanUp(dr);

            return retVal;
        }

        /// <summary>
        /// Get a list of T from the data source.
        /// </summary>
        /// <param name="commandText">The stored procedure to use.</param>
        /// <param name="fromDb">The method for mapping the data to an object.</param>
        /// <returns>A list of Ts.</returns>
        protected virtual List<T> OperationList(string commandText, GetDataDelegate<T> fromDb)
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
        protected virtual List<T> OperationList(string commandText, GetDataDelegate<T> fromDb, bool forceNewConnection)
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
        protected virtual List<T> OperationList(string commandText, GetDataDelegate<T> fromDb, CommandType commandType)
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
        protected virtual List<T> OperationList(string commandText, GetDataDelegate<T> fromDb, CommandType commandType, bool forceNewConnection)
        {
            return OperationList(commandText, new ListDictionary(), fromDb, commandType, forceNewConnection);
        }

        /// <summary>
        /// Get a list of T from the data source.
        /// </summary>
        /// <param name="commandText">The stored procedure to use.</param>
        /// <param name="parameters">List of parameters, ("ParameterName","Value").</param>
        /// <param name="fromDb">The method for mapping the data to an object.</param>
        /// <returns>A list of Ts.</returns>
        protected virtual List<T> OperationList(string commandText, ListDictionary parameters, GetDataDelegate<T> fromDb)
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
        protected virtual List<T> OperationList(string commandText, ListDictionary parameters, GetDataDelegate<T> fromDb, bool forceNewConnection)
        {
            return OperationList(commandText, parameters, fromDb, CommandType.StoredProcedure, forceNewConnection);
        }

        /// <summary>
        /// Get a list of T from the data source.
        /// </summary>
        /// <param name="commandText">The stored procedure to use.</param>
        /// <param name="parameters">The parameters.</param>
        /// <param name="fromDb">The method for mapping the data to an object.</param>
        /// <param name="commandType">Type of the command.</param>
        /// <returns>A list of Ts.</returns>
        protected virtual List<T> OperationList(string commandText, ListDictionary parameters, GetDataDelegate<T> fromDb, CommandType commandType)
        {
            bool forceNewConnection = false;
            return OperationList(commandText, parameters, fromDb, commandType, forceNewConnection);
        }
        /// <summary>
        /// Get a list of T from the data source.
        /// </summary>
        /// <param name="commandText">The stored procedure to use.</param>
        /// <param name="parameters">The parameters.</param>
        /// <param name="fromDb">The method for mapping the data to an object.</param>
        /// <param name="commandType">Type of the command.</param>
        /// <param name="forceNewConnection">if set to <c>true</c> [force new connection].</param>
        /// <returns>A list of Ts.</returns>
        protected virtual List<T> OperationList(string commandText, ListDictionary parameters, GetDataDelegate<T> fromDb, CommandType commandType, bool forceNewConnection)
        {
            List<T> list = new List<T>();

            DbDataReader dr = Operation(commandText, parameters, commandType, forceNewConnection);
            if (dr != null)
            {
                while (dr.Read())
                    list.Add(fromDb(dr));

                CleanUp(dr);

                return list;
            }

            return null;
        }

        
    }
}
