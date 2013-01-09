using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;

namespace ReflectORM.Core
{
    /// <summary>
    /// An object used to represent a criterion in an SQL where clause
    /// </summary>
    public class Criteria
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Criteria"/> class.
        /// </summary>
        /// <param name="column">The column.</param>
        /// <param name="value">The value.</param>
        public Criteria(string column, string value)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Criteria"/> class.
        /// </summary>
        /// <param name="column">The column.</param>
        /// <param name="value">The value.</param>
        /// <param name="columnName">Name of the column.</param>
        public Criteria(string column, ColumnType value, string columnName)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Criteria"/> class.
        /// </summary>
        /// <param name="column">The column.</param>
        /// <param name="value">The value.</param>
        /// <param name="columnName">Name of the column.</param>
        /// <param name="s">The s.</param>
        /// <param name="columnName1">The column name1.</param>
        /// <param name="historyColumnTableName">Name of the history column table.</param>
        public Criteria(string column, ColumnType value, string columnName, string s, string columnName1, string historyColumnTableName)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Criteria"/> class.
        /// </summary>
        /// <param name="recordid">The recordid.</param>
        /// <param name="value">The value.</param>
        /// <param name="columnName">Name of the column.</param>
        /// <param name="id">The id.</param>
        /// <param name="historyTableName">Name of the history table.</param>
        /// <param name="historyColumnTableName">Name of the history column table.</param>
        /// <param name="s">The s.</param>
        public Criteria(string recordid, ColumnType value, string columnName, int id, string historyTableName, string historyColumnTableName, string s)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Criteria"/> class.
        /// </summary>
        /// <param name="recordid">The recordid.</param>
        /// <param name="value">The value.</param>
        public Criteria(string recordid, object value)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Criteria"/> class.
        /// </summary>
        /// <param name="recordid">The recordid.</param>
        /// <param name="value">The value.</param>
        /// <param name="columnName">if set to <c>true</c> [column name].</param>
        public Criteria(string recordid, ColumnType value, bool columnName)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Criteria"/> class.
        /// </summary>
        /// <param name="idColumn">The id column.</param>
        /// <param name="value">The value.</param>
        /// <param name="columnName">Name of the column.</param>
        public Criteria(string idColumn, ColumnType value, int columnName)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Criteria"/> class.
        /// </summary>
        /// <param name="idColumn">The id column.</param>
        /// <param name="value">The value.</param>
        /// <param name="columnName">Name of the column.</param>
        public Criteria(string idColumn, ColumnType value, object columnName)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets or sets the column.
        /// </summary>
        /// <value>
        /// The column.
        /// </value>
        public string Column { get; set; }
        /// <summary>
        /// Gets or sets the name of the parameter.
        /// </summary>
        /// <value>
        /// The name of the parameter.
        /// </value>
        public string ParameterName { get; set; }

        /// <summary>
        /// Gets or sets the criteria children.
        /// </summary>
        /// <value>
        /// The criteria children.
        /// </value>
        public IEnumerable<Criteria> CriteriaChildren { get; set; }

        /// <summary>
        /// Gets or sets the table.
        /// </summary>
        /// <value>
        /// The table.
        /// </value>
        public string Table { get; set; }

        /// <summary>
        /// Gets or sets the joins.
        /// </summary>
        /// <value>
        /// The joins.
        /// </value>
        public IEnumerable<Join> Joins
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        /// <summary>
        /// Gets or sets the type of the search.
        /// </summary>
        /// <value>
        /// The type of the search.
        /// </value>
        public SearchType SearchType
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        /// <summary>
        /// Gets or sets the type.
        /// </summary>
        /// <value>
        /// The type.
        /// </value>
        public ColumnType Type
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        /// <summary>
        /// Adds the join.
        /// </summary>
        /// <param name="historyTableName">Name of the history table.</param>
        /// <param name="historyColumnTableName">Name of the history column table.</param>
        /// <param name="id">The id.</param>
        /// <param name="historyid">The historyid.</param>
        public void AddJoin(string historyTableName, string historyColumnTableName, string id, string historyid)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets the SQL.
        /// </summary>
        /// <param name="b">if set to <c>true</c> [b].</param>
        /// <param name="usetable">The usetable.</param>
        /// <returns></returns>
        public string GetSQL(bool b, string usetable)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets the parameters.
        /// </summary>
        /// <param name="parameters">The parameters.</param>
        /// <returns></returns>
        public ListDictionary GetParameters(ListDictionary parameters)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets the SQL.
        /// </summary>
        /// <param name="b">if set to <c>true</c> [b].</param>
        /// <returns></returns>
        public string GetSQL(bool b)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets the parameter value.
        /// </summary>
        /// <returns></returns>
        public object GetParameterValue()
        {
            throw new NotImplementedException();
        }
    }
}
