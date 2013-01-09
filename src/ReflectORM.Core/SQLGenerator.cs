using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using System.Collections.Specialized;
using System.Reflection;
using ReflectORM.Extensions;
using System.Data.Common;
using System.Collections;

namespace ReflectORM.Core
{
    /// <summary>
    /// Generates SQL
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class SQLGenerator<T>
    {
        private DbCommand _command;

        /// <summary>
        /// Gets or sets the criteria.
        /// </summary>
        /// <value>The criteria.</value>
        public List<Criteria> Criteria { get; private set; }
        /// <summary>
        /// Gets or sets the joins.
        /// </summary>
        /// <value>The joins.</value>
        public List<Join> Joins { get; private set; }

        /// <summary>
        /// Gets or sets the order by.
        /// </summary>
        /// <value>The order by.</value>
        public List<string> OrderBy { get; private set; }

        /// <summary>
        /// Gets or sets the group by.
        /// </summary>
        /// <value>The group by.</value>
        public List<string> GroupBy { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether [insert id].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [insert id]; otherwise, <c>false</c>.
        /// </value>
        public bool InsertId { get; set; }

        /// <summary>
        /// Gets or sets the command.
        /// </summary>
        /// <value>The command.</value>
        public DbCommand Command { get { return _command; } private set { _command = value; } }

        /// <summary>
        /// Initializes a new instance of the <see cref="SQLGenerator&lt;T&gt;"/> class.
        /// </summary>
        /// <param name="command">The command.</param>
        public SQLGenerator(DbCommand command) : this(command, false) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="SQLGenerator&lt;T&gt;"/> class.
        /// </summary>
        /// <param name="command">The command.</param>
        /// <param name="ignoreDeletedStatus">if set to <c>true</c> [ignore deleted status].</param>
        public SQLGenerator(DbCommand command, bool ignoreDeletedStatus)
        {
            Criteria = new List<Criteria>();
            if (!ignoreDeletedStatus && (typeof(T).IsSubclassOf(typeof(Auditable))))
            {
                var sc = new Criteria("Deleted", ColumnType.Bool, false);
                sc.SearchType = SearchType.Equals;
                Criteria.Add(sc);
            }

            Joins = new List<Join>();
            OrderBy = new List<string>();
            GroupBy = new List<string>();
            InsertId = false;
            Command = command;
        }

        /// <summary>
        /// Generates the select.
        /// </summary>
        /// <param name="table">The table.</param>
        /// <returns></returns>
        public string GenerateSelect(string table)
        {
            return GenerateSelect(table, 0, GetReadableColumnNames(table).ToArray());
        }

        /// <summary>
        /// Generates the select.
        /// </summary>
        /// <param name="table">The table.</param>
        /// <param name="top">The top.</param>
        /// <returns></returns>
        public string GenerateSelect(string table, int top)
        {
            return GenerateSelect(table, top, GetReadableColumnNames(table).ToArray());
        }

        /// <summary>
        /// Generates the select.
        /// </summary>
        /// <param name="table">The table.</param>
        /// <param name="columns">The columns.</param>
        /// <returns></returns>
        public string GenerateSelect(string table, params string[] columns)
        {
            return GenerateSelect(table, -1, columns);
        }

        /// <summary>
        /// Generates the select.
        /// </summary>
        /// <param name="table">The table.</param>
        /// <param name="top">The top.</param>
        /// <param name="columns">The columns.</param>
        /// <returns></returns>
        public string GenerateSelect(string table, int top, params string[] columns)
        {
            StringBuilder sql = new StringBuilder();

            string topString = string.Empty;

            if (top > 0)
                topString = string.Format("TOP ({0}) ", top);

            sql.AppendFormat("SELECT {0}", topString);
            sql.Append(BuildDelimitedString(columns));
            sql.AppendFormat(" FROM {0} ", table.SurroundWithSquareBrackets());

            GetJoinsFromCriteria();

            sql.Append(GenerateJoins());
            sql.Append(" ");
            sql.Append(GenerateWhereClause(table));
            sql.Append(GenerateGroupBy());
            sql.Append(GenerateOrderBy());

            return sql.ToString();
        }

        /// <summary>
        /// Generates the delete.
        /// </summary>
        /// <param name="table">The table.</param>
        /// <returns></returns>
        public string GenerateDelete(string table)
        {
            return GenerateDelete(table, table);
        }

        /// <summary>
        /// Generates the delete.
        /// </summary>
        /// <param name="table">The table.</param>
        /// <param name="selectTable">The select table.</param>
        /// <returns></returns>
        public string GenerateDelete(string table, string selectTable)
        {
            if (Criteria.Count == 0)
                return string.Empty;

            StringBuilder sql = new StringBuilder();

            sql.AppendFormat("DELETE FROM {0} ", table);
            sql.Append(GenerateWhereClause(table));

            sql.Append(GenerateSelect(selectTable));

            return sql.ToString();
        }

        /// <summary>
        /// Generates the insert.
        /// </summary>
        /// <param name="table">The table.</param>
        /// <param name="data">The data.</param>
        /// <param name="toDb">To db.</param>
        /// <param name="idColumn">The id column.</param>
        /// <returns></returns>
        public string GenerateInsert(string table, T data, SetParameterDelegate<T> toDb, string idColumn)
        {
            toDb(data, ref _command);    //populate the command object with parameters

            return GenerateInsert(table, data, idColumn);
        }

        /// <summary>
        /// Generates the insert.
        /// </summary>
        /// <param name="table">The table.</param>
        /// <param name="data">The data.</param>
        /// <param name="idColumn">The id column.</param>
        /// <returns></returns>
        public string GenerateInsert(string table, T data, string idColumn)
        {
            StringBuilder sql = new StringBuilder();

            if (InsertId)
                sql.AppendFormat("SET IDENTITY_INSERT {0} ON; ", table.SurroundWithSquareBrackets());

            sql.AppendFormat("INSERT INTO {0} (", table.SurroundWithSquareBrackets());
            sql.AppendFormat("{0}) ", BuildDelimitedString(GetWritableColumnNames(data, idColumn, false, new List<PropertyInfo>())));
            sql.AppendFormat("SELECT {0} ", BuildDelimitedString(GetParameterNames()));

            if (InsertId)
                sql.AppendFormat(" SET IDENTITY_INSERT {0} OFF; ", table.SurroundWithSquareBrackets());

            sql.AppendFormat("SELECT SCOPE_IDENTITY()");

            return sql.ToString();
        }

        /// <summary>
        /// Generates the update.
        /// </summary>
        /// <param name="table">The table.</param>
        /// <param name="data">The data.</param>
        /// <param name="toDb">To db.</param>
        /// <param name="idColumn">The id column.</param>
        /// <returns></returns>
        public string GenerateUpdate(string table, T data, SetParameterDelegate<T> toDb, string idColumn)
        {
            return GenerateUpdate(table, data, toDb, idColumn, new List<PropertyInfo>());
        }

        /// <summary>
        /// Generates the update.
        /// </summary>
        /// <param name="table">The table.</param>
        /// <param name="data">The data.</param>
        /// <param name="toDb">To db.</param>
        /// <param name="idColumn">The id column.</param>
        /// <param name="properties">The properties.</param>
        /// <returns></returns>
        public string GenerateUpdate(string table, T data, SetParameterDelegate<T> toDb, string idColumn, List<PropertyInfo> properties)
        {
            toDb(data, ref _command);

            return GenerateUpdate(table, data, idColumn, properties);
        }

        /// <summary>
        /// Generates the update.
        /// </summary>
        /// <param name="table">The table.</param>
        /// <param name="data">The data.</param>
        /// <param name="idColumn">The id column.</param>
        /// <param name="properties">The properties.</param>
        /// <returns></returns>
        public string GenerateUpdate(string table, T data, string idColumn, List<PropertyInfo> properties)
        {
            StringBuilder sql = new StringBuilder();

            Criteria id = GetIdCriteria(data, idColumn);

            if (id != null)
                Criteria.Add(id);
            else
                throw new NullReferenceException("Getting Id criteria failed.");    //some bad stuff happened, an exception should be thrown

            sql.AppendFormat("UPDATE {0} SET ", table.SurroundWithSquareBrackets());
            IEnumerable<string> cols = GetWritableColumnNames(data, idColumn, true, properties);
            IEnumerable<string> parameters = GetParameterNames(data, idColumn, properties);

            if (cols.Count() != parameters.Count()) return string.Empty; //bad things have happened, an exception should be thrown here

            for (int i = 0; i < cols.Count(); i++)
            {
                sql.AppendFormat("{0} = {1}", cols.ElementAt(i), parameters.ElementAt(i));
                if (i < cols.Count() - 1)   //don't add a comma to the end of the list
                    sql.Append(',');
            }

            sql.Append(" ");
            sql.Append(GenerateWhereClause(table));

            return sql.ToString();
        }

        /// <summary>
        /// Generates SQL to get record count.
        /// </summary>
        /// <param name="table">The table.</param>
        /// <param name="countColumn">The count column.</param>
        /// <returns></returns>
        public string GenerateCount(string table, string countColumn)
        {
            StringBuilder sql = new StringBuilder();

            sql.AppendFormat("SELECT COUNT({0}) ", countColumn);
            sql.AppendFormat("FROM {0} ", table);

            GetJoinsFromCriteria();

            sql.Append(GenerateJoins());
            sql.Append(" ");
            sql.Append(GenerateWhereClause(table));
            sql.Append(GenerateGroupBy());
            sql.Append(GenerateOrderBy());

            return sql.ToString();
        }

        /// <summary>
        /// Gets the writable column names.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="idColumn">The id column.</param>
        /// <param name="updatable">if set to <c>true</c> [updatable].</param>
        /// <param name="properties">The properties.</param>
        /// <returns></returns>
        private IEnumerable<string> GetWritableColumnNames(T data, string idColumn, bool updatable, List<PropertyInfo> properties)
        {
            List<string> columns = new List<string>();

            Type type = typeof(T);
            if (properties.Count == 0)
                properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public).ToList();

            StringBuilder sb = new StringBuilder();

            foreach (PropertyInfo property in properties)
            {
                if (property.CanRead)
                {
                    object[] attributes = property.GetCustomAttributes(typeof(ReflectORM.Attributes.ControllablePropertyAttribute), true);

                    string colName = property.Name;
                    bool write = true;
                    if (attributes.Length > 0) //it does have the attribute.
                    {
                        ReflectORM.Attributes.ControllablePropertyAttribute fda = (ReflectORM.Attributes.ControllablePropertyAttribute)attributes[0];
                        colName = fda.ColumnName ?? property.Name;
                        write = fda.Write;
                    }

                    //don't add the Id
                    if (!InsertId && colName == idColumn)
                        continue;

                    if (write)
                        columns.Add(string.Format("[{0}]", colName));
                }
            }

            return columns;
        }

        /// <summary>
        /// Gets the readable column names.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<string> GetReadableColumnNames(string table)
        {
            List<string> columns = new List<string>();

            Type type = typeof(T);
            PropertyInfo[] properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public);

            StringBuilder sb = new StringBuilder();

            foreach (PropertyInfo property in properties)
            {
                if (property.CanRead)
                {
                    object[] attributes = property.GetCustomAttributes(typeof(ReflectORM.Attributes.ControllablePropertyAttribute), true);

                    string tableName = table;
                    string colName = property.Name;
                    bool read = true;
                    ReflectORM.Attributes.ControllablePropertyAttribute fda = new ReflectORM.Attributes.ControllablePropertyAttribute();
                    if (attributes.Length > 0) //it does have the attribute.
                    {
                        fda = (ReflectORM.Attributes.ControllablePropertyAttribute)attributes[0];
                        colName = fda.ColumnName ?? property.Name;
                        tableName = fda.TableName ?? table;
                        read = fda.Read;
                    }

                    if (read)
                    {
                        if (fda.UseCoalesce)
                            columns.Add(string.Format("COALESCE([{0}].[{1}], {2}) AS {1}", tableName, colName, fda.Coalesce));
                        else
                        {
                            if (fda.CreateBool.Trim() == string.Empty)
                                columns.Add(string.Format("[{0}].[{1}]", tableName, colName));
                            else
                            {
                                columns.Add(string.Format("CASE WHEN ([{0}].[{1}] {2}) THEN CAST(1 AS bit) ELSE CAST(0 AS bit) END AS [{3}]", tableName, fda.CreateBoolColumn, fda.CreateBool, colName));
                            }
                        }
                    }
                }
            }

            return columns;
        }

        private Criteria GetIdCriteria(T data, string idColumn)
        {
            List<string> parameterNames = new List<string>();

            Type type = data.GetType();
            PropertyInfo[] properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public);

            StringBuilder sb = new StringBuilder();

            foreach (PropertyInfo property in properties)
            {
                if (property.CanRead)
                {
                    object[] attributes = property.GetCustomAttributes(typeof(ReflectORM.Attributes.ControllablePropertyAttribute), true);

                    string colName = property.Name;
                    bool write = true;
                    if (attributes.Length > 0) //it does have the attribute.
                    {
                        ReflectORM.Attributes.ControllablePropertyAttribute fda = (ReflectORM.Attributes.ControllablePropertyAttribute)attributes[0];
                        colName = fda.ColumnName ?? property.Name;
                        write = fda.Write;
                    }

                    if (colName.ToLower() == idColumn.ToLower())
                        return new Criteria(idColumn, ColumnType.Int, property.GetValue(data, null));
                }
            }

            return null;
        }

        /// <summary>
        /// Gets the parameter names.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="idColumn">The id column.</param>
        /// <param name="properties">The properties.</param>
        /// <returns></returns>
        private IEnumerable<string> GetParameterNames(T data, string idColumn, List<PropertyInfo> properties)
        {
            List<string> parameterNames = new List<string>();

            Type type = data.GetType();
            if (properties.Count == 0)
                properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public).ToList();

            StringBuilder sb = new StringBuilder();

            foreach (PropertyInfo property in properties)
            {
                if (property.CanRead)
                {
                    object[] attributes = property.GetCustomAttributes(typeof(ReflectORM.Attributes.ControllablePropertyAttribute), true);

                    string colName = property.Name;
                    string paramName = property.Name;
                    bool write = true;
                    if (attributes.Length > 0) //it does have the attribute.
                    {
                        ReflectORM.Attributes.ControllablePropertyAttribute fda = (ReflectORM.Attributes.ControllablePropertyAttribute)attributes[0];
                        colName = fda.ColumnName ?? property.Name;
                        paramName = fda.ParameterName ?? colName;
                        write = fda.Write;
                    }

                    //don't add the Id
                    if (colName == idColumn)
                        continue;

                    if (!paramName.StartsWith("@"))
                        paramName = string.Format("@{0}", paramName);   //prefix the name with the required @ 

                    if (write)
                        parameterNames.Add(paramName);
                }
            }

            return parameterNames;
        }

        /// <summary>
        /// Gets the parameter names.
        /// </summary>
        /// <returns></returns>
        private IEnumerable<string> GetParameterNames()
        {
            List<string> parameterNames = new List<string>();

            foreach (SqlParameter param in Command.Parameters)
                parameterNames.Add(param.ParameterName);

            return parameterNames;
        }

        /// <summary>
        /// Gets the joins from criteria.
        /// </summary>
        private void GetJoinsFromCriteria()
        {
            foreach (Criteria sc in Criteria)
            {
                foreach (Join j in sc.Joins)
                {
                    if (Joins.Contains(j)) continue;

                    Joins.Add(j);
                }
            }
        }

        /// <summary>
        /// Generates the joins.
        /// </summary>
        /// <returns></returns>
        private string GenerateJoins()
        {
            return GenerateJoins(Joins);
        }

        /// <summary>
        /// Generates the joins.
        /// </summary>
        /// <param name="joins">The joins.</param>
        /// <returns></returns>
        internal static string GenerateJoins(IEnumerable<Join> joins)
        {
            StringBuilder joinString = new StringBuilder();

            foreach (Join j in joins)
                joinString.Append(j.GetSQL());

            return joinString.ToString();
        }

        private string GenerateOrderBy()
        {
            StringBuilder sb = new StringBuilder();

            if (!OrderBy.IsNull() && OrderBy.Count > 0)
            {
                sb.Append(" ORDER BY ");
                sb.Append(BuildDelimitedString(OrderBy));
            }

            return sb.ToString();
        }

        private string GenerateGroupBy()
        {
            StringBuilder sb = new StringBuilder();

            if (!GroupBy.IsNull() && GroupBy.Count > 0)
            {
                sb.Append(" GROUP BY ");
                sb.Append(BuildDelimitedString(GroupBy));
            }

            return sb.ToString();
        }

        /// <summary>
        /// Generates the where clause.
        /// </summary>
        /// <param name="table">The table.</param>
        /// <returns></returns>
        private string GenerateWhereClause(string table)
        {
            return GenerateWhereClause(this.Criteria, _command, table);
        }

        /// <summary>
        /// Generates the where clause.
        /// </summary>
        /// <param name="criteria">The criteria.</param>
        /// <param name="command">The command.</param>
        /// <param name="table">The table.</param>
        /// <returns></returns>
        internal static string GenerateWhereClause(IEnumerable<Criteria> criteria, DbCommand command, string table)
        {
            StringBuilder whereSql = new StringBuilder();

            if (criteria.Count() > 0)
                whereSql.Append("WHERE ");

            criteria = DbUtilities.ValidateSearchCriteria((IList<Criteria>)criteria);

            int i = 0;
            foreach (Criteria c in criteria)
            {
                var usetable = table;
                if (c.Table != string.Empty) usetable = c.Table;

                string criteriaSql = c.GetSQL(i++ > 0, usetable);   //i > 0 will tell us if we need to include the operator. The first criteria won't need an operator.
                if (criteriaSql != string.Empty)
                {
                    string paramName = c.ParameterName;
                    if (!command.Parameters.Contains(paramName))
                    {
                        ListDictionary parameters = new ListDictionary();
                        parameters = c.GetParameters(parameters);
                        //throw new ParameterExistsException(paramName);

                        foreach (DictionaryEntry de in parameters)
                            command.AddParameterWithValue((string)de.Key, de.Value);
                    }
                    whereSql.Append(criteriaSql);
                }
            }

            return whereSql.ToString();
        }

        /// <summary>
        /// Builds the delimited string.
        /// </summary>
        /// <param name="list">The list.</param>
        /// <returns></returns>
        public string BuildDelimitedString(IEnumerable<string> list)
        {
            return BuildDelimitedString(list, ',');
        }

        /// <summary>
        /// Builds the delimited string.
        /// </summary>
        /// <param name="list">The list.</param>
        /// <param name="delimiter">The delimiter.</param>
        /// <returns></returns>
        private string BuildDelimitedString(IEnumerable<string> list, char delimiter)
        {
            string delimitedString = string.Empty;
            foreach (string s in list)
                delimitedString = string.Format("{0}{1}{2}", delimitedString, delimiter, s);

            delimitedString = delimitedString.Trim(delimiter); //remove any leading or trailing delimiters

            return delimitedString;
        }

        /// <summary>
        /// Gets the parameter list.
        /// </summary>
        /// <param name="criteria">The criteria.</param>
        /// <param name="table">The table.</param>
        /// <returns></returns>
        public static ListDictionary GetParameterList(IEnumerable<Criteria> criteria, string table)
        {
            ListDictionary parameters = new ListDictionary();

            int i = 0;  //added criteria counter
            foreach (Criteria c in criteria)
            {
                if (c.Table == string.Empty) c.Table = table;

                string criteriaSql = c.GetSQL(i++ > 0);   //i > 0 will tell us if we need to include the operator. The first criteria won't need an operator.
                if (criteriaSql != string.Empty)
                    parameters.Add(c.ParameterName, c.GetParameterValue());
            }

            return parameters;
        }


        /// <summary>
        /// Gets the paginated SQL string.
        /// </summary>
        /// <param name="SQL">The SQL, starting with the first column you want to select, eg: id FROM table. Don't put SELECT at the start.</param>
        /// <param name="databaseTableName">Name of the database table.</param>
        /// <param name="idColumn">The id column.</param>
        /// <param name="resultCountColumn">The result count column name.</param>
        /// <param name="resultsPerPage">The results per page.</param>
        /// <param name="firstRecord">The first record.</param>
        /// <param name="orderBy">The order by, the column to order by, usually KEY_TBL.[RANK].</param>
        /// <param name="order">The order. (Ascending or Descending)</param>
        /// <returns></returns>
        public static string GetPaginatedSQL(string SQL, string databaseTableName, string idColumn, out string resultCountColumn, int resultsPerPage, int firstRecord, string orderBy, SortOrder order)
        {
            resultCountColumn = "totalResults";
            string paginationTable = "pagination";

            //remove the word SELECT from the start of the SQL
            if (SQL.ToUpper().Trim().StartsWith("SELECT"))
                SQL = SQL.Remove(0, 6);

            StringBuilder sb = new StringBuilder();

            sb.AppendFormat("WITH {0} AS (", paginationTable);
            sb.Append("SELECT row_number() OVER (ORDER BY");

            string[] orderBys = orderBy.Split(',');

            for (int i = 0; i < orderBys.Length; i++)
            {
                sb.AppendFormat(" {0}", orderBys[i].Trim());
                sb.Append((order == SortOrder.Ascending) ? " ASC" : " DESC");
                if (i < orderBys.Length - 1) //don't add a comma at the end
                    sb.Append(",");
            }

            sb.Append(" ) as rowNo, ");
            sb.Append(SQL);
            sb.AppendFormat(") SELECT [{1}].*, (SELECT COUNT(rowNo) FROM [{0}]) as {2} FROM [{0}] ", paginationTable, databaseTableName, resultCountColumn);
            sb.AppendFormat("INNER JOIN [{0}] ON [{0}].[{1}] = [{2}].[{1}] ", databaseTableName, idColumn, paginationTable);

            if (resultsPerPage > 0)
                sb.AppendFormat(" WHERE rowNo BETWEEN {0} AND {1}", firstRecord, firstRecord + resultsPerPage);

            sb.Append(" ORDER BY rowNo");

            return sb.ToString();
        }

        /// <summary>
        /// Gets the join SQL.
        /// </summary>
        /// <param name="allJoins">All joins.</param>
        /// <param name="table">The table.</param>
        /// <returns></returns>
        public string GetJoinSQL(List<Join> allJoins, string table)
        {
            return GetJoinSQL(allJoins, table, string.Empty, string.Empty, string.Empty);
        }

        /// <summary>
        /// Gets the join SQL.
        /// </summary>
        /// <param name="allJoins">All joins.</param>
        /// <param name="table">The table.</param>
        /// <param name="replacement">The replacement.</param>
        /// <param name="idColumn">The id column.</param>
        /// <param name="replacementColumn">The replacement column.</param>
        /// <returns></returns>
        public string GetJoinSQL(List<Join> allJoins, string table, string replacement, string idColumn, string replacementColumn)
        {
            StringBuilder joins = new StringBuilder();
            List<Join> usedJoins = new List<Join>();

            foreach (Join j in allJoins)
            {
                string tempTable = string.Empty;
                bool used = false;
                if (j.FirstTable == string.Empty)
                    tempTable = table;
                else
                    tempTable = j.FirstTable;
                string tempSecondTable = string.Empty;

                foreach (Join usedJoin in usedJoins)
                {
                    if (j.Equals(usedJoin))
                    {
                        used = true;
                        break;
                    }
                }

                if (!used)
                {
                    if (j.FirstTable == table)
                    {
                        tempTable = replacement;
                        foreach (JoinColumn jc in j.JoinColumns)
                            if (jc.FirstColumn == idColumn)
                                jc.FirstColumn = replacementColumn;
                    }

                    if (j.SecondTable == table)
                    {
                        tempSecondTable = replacement;
                        foreach (JoinColumn jc in j.JoinColumns)
                            if (jc.SecondColumn == idColumn)
                                jc.SecondColumn = replacementColumn;
                    }

                    joins.Append(j.GetSQL(tempTable, tempSecondTable));

                    usedJoins.Add(j);
                }
            }

            return joins.ToString();
        }

        private static string AppendTableNameToSortColumn(string tableName, string orderBy)
        {
            string retVal = string.Empty;
            string[] orderBys = orderBy.Split(',');

            for (int i = 0; i < orderBys.Length; i++)
                retVal += string.Format("[{0}].[{1}], ", tableName, orderBys[i].Trim());
            return retVal.Remove(retVal.Length - 2, 2);
        }
    }
}
