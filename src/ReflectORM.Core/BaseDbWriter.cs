using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Reflection;
using System.Data.SqlClient;
using System.Data.Common;
using ReflectORM.Extensions;

namespace ReflectORM.Core
{
    /// <summary>
    /// Base Db Writer, provides base methods for writing data to a database
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class BaseDbWriter<T> : BaseDbOperator<T>
    {
        /// <summary>
        /// 
        /// </summary>
        public delegate bool IsUpdateableDelegate(T data);

        /// <summary>
        /// Saves the specified data.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <returns>The Id of the saved data</returns>
        public abstract object Save(T data);

        /// <summary>
        /// Deletes the specified id.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <returns></returns>
        public abstract bool Delete(int id);

        /// <summary>
        /// Gets or sets the is updatable.
        /// </summary>
        /// <value>The is updatable.</value>
        public virtual IsUpdateableDelegate IsUpdatable { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseDbWriter&lt;T&gt;"/> class.
        /// </summary>
        /// <param name="connectionString">The connection string.</param>
        /// <param name="isUpdatable">The is updatable.</param>
        public BaseDbWriter(string connectionString, IsUpdateableDelegate isUpdatable)
            : base(connectionString)
        {
            IsUpdatable = isUpdatable;
        }

        /// <summary>
        /// Operations the specified command text.
        /// </summary>
        /// <param name="commandText">The command text.</param>
        /// <param name="data">The data.</param>
        /// <param name="setParam">The set param.</param>
        /// <param name="forceNewConnection">if set to <c>true</c> [force new connection].</param>
        /// <returns></returns>
        protected DbDataReader Operation(string commandText, T data, SetParameterDelegate<T> setParam, bool forceNewConnection)
        {
            DbCommand command = Connection.CreateCommand(commandText);
            if (setParam != null)
                setParam(data, ref command);

            return Operation(command, forceNewConnection);
        }

        /// <summary>
        /// Adds parameters to the command from the T.
        /// </summary>
        /// <param name="data">The data to get the parameters.</param>
        /// <param name="command">The command object to add parameters to.</param>
        protected virtual void ToDb<S>(T data, ref DbCommand command)
        {
            Type type = data.GetType();
            PropertyInfo[] properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public);

            StringBuilder sb = new StringBuilder();

            foreach (PropertyInfo property in properties)
            {
                if (property.CanRead)
                {
                    object[] attributes = property.GetCustomAttributes(typeof(ReflectORM.Attributes.ControllablePropertyAttribute), true);

                    string colName = property.Name;
                    string paramName = property.Name;
                    char delimiter = char.MinValue;
                    bool write = true;
                    if (attributes.Length > 0) //it does have the attribute.
                    {
                        ReflectORM.Attributes.ControllablePropertyAttribute fda = (ReflectORM.Attributes.ControllablePropertyAttribute)attributes[0];
                        colName = fda.ColumnName ?? property.Name;
                        paramName = fda.ParameterName ?? property.Name;
                        delimiter = fda.StringSplitDelimiter;
                        write = fda.Write;
                    }

                    //don't add the Id column if we're inserting
                    if (!IsUpdatable(data) && colName == IdColumn)
                        continue;

                    if (write)
                    {
                        object value = property.GetValue(data, null);

                        if (property.PropertyType.IsEnum)
                            value = value.ToString();

                        if (delimiter != char.MinValue)
                            if (value is IList)
                            {
                                string newVal = string.Empty;
                                IList<string> values = (IList<string>)value;
                                foreach (string s in values)
                                    newVal = string.Format("{0}{1}", newVal, s);

                                value = newVal;
                            }

                        if (!paramName.StartsWith("@"))
                            paramName = string.Format("@{0}", paramName);   //prefix the name with the required @ 

                        command.AddParameterWithValue(paramName, value);
                    }
                }
            }
        }
    }
}
