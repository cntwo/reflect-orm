using System;
using System.Data;
using System.Data.SqlClient;
using System.Data.Common;
using System.Linq;
using ReflectORM.Extensions;
using System.Collections.Generic;

namespace ReflectORM.Core
{
    /// <summary>
    /// SQL Server Database Utilities.
    /// </summary>
    public static class DbUtilities
    {
        #region Global Private Variables

        private const string cstrDefaultStringPart0 = "Integrated Security=";
        private const string cstrDefaultStringPart1 = ";Persist Security Info=False;Initial Catalog=";
        private const string cstrDefaultStringPart2 = ";Data Source=";
        private const string cstrDefaultStringPart3Dev = @"server18.campden.co.uk\Dev";
        private const string cstrDefaultStringPart4 = ";User ID=";
        private const string cstrDefaultStringPart5 = ";Password=";
        private const string cstrDefaultStringPart6 = ";Workstation ID=";

        #endregion

        #region Public Methods
        /// <summary>
        /// Create a connection object for the specified connection string and opens it
        /// </summary>
        /// <returns></returns>
        public static DbConnection GetOpenConnection(string DbConnectionString)
        {
            DbConnection _mycon = new SqlConnection(DbConnectionString);
            if (_mycon != null)
                if (_mycon.State == ConnectionState.Closed) _mycon.Open();
            return _mycon;
        }
        
        /// <summary>
        /// Checks a string and replaces CR-LF with CR, and '(apostrophe) with two apostrophes.
        /// It also removes any double dashes "--"
        /// </summary>
        /// <param name="strSQLString">A string to fix.</param>
        /// <returns>String ready for SQL.</returns>
        /// <remarks>Used to validate strings for use in SQL queries.</remarks>
        /// <history>
        /// 	[abristow]	27/08/04	Created
        /// </history>
        public static string FixForSQL(string strSQLString)
        {
            string strRetVal = strSQLString;

            // Compact a CR-LF sequence as CR to save space
            strRetVal = strRetVal.Replace("\n", "\r");

            // Replace each apostrophe with two apostrophes
            strRetVal = strRetVal.Replace("'", "''");

            // remove double dashes
            strRetVal = strRetVal.Replace("--", "");

            return strRetVal;
        }

        /// <summary>
        /// Truncates the millisecond of a datetime so that SQL Server does not do this when it saves
        /// and so DateTimes passed in will be exactly the same when they are retrieved
        /// </summary>
        /// <param name="dt">DateTime to truncate</param>
        /// <returns>Truncated DateTime</returns>
        public static DateTime TruncateTo100thOfASecond(DateTime dt)
        {
            int m = (dt.Millisecond / 10) * 10;
            return new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second, m);
        } 

        /// <summary>
        /// Executes a sql string and returns the results in a datatable
        /// </summary>
        /// <param name="strSQl">The sql to execute</param>
        /// <param name="DbConnection">The database connection to use</param>
        /// <returns>A datatable containing the results</returns>
        public static DataTable ExecuteSQL(string strSQl, DbConnection DbConnection)
        {
            return ExecuteSQL(strSQl, DbConnection, string.Empty);
        }

        /// <summary>
        /// Executes a sql string and returns the results in a datatable
        /// </summary>
        /// <param name="strSQl">The sql to execute</param>
        /// <param name="DbConnection">The database connection to use</param>
        /// <param name="strTableName">The name to give the table</param>
        /// <returns>A datatable containing the results</returns>
        /// <history>
        ///     [coullm]    21/07/09    Added functionality to throw a data operation exception
        /// </history>
        public static DataTable ExecuteSQL(string strSQl, DbConnection DbConnection, string strTableName)
        {
            DbCommand comm = DbConnection.CreateCommand(strSQl);
            return ExecuteSQL(comm, strTableName);
        }

        /// <summary>
        /// Executes a sql command and returns the results in a datatable
        /// </summary>
        /// <param name="sqlComm">The SQL command to execute</param>
        /// <returns>Database containing the results of the search</returns>
        public static DataTable ExecuteSQL(DbCommand sqlComm)
        {
            return ExecuteSQL(sqlComm, string.Empty);
        }

        /// <summary>
        /// Executes a sql command and returns the results in a datatable
        /// </summary>
        /// <param name="sqlComm">The SQL command to execute</param>
        /// <param name="strTableName">Table name of the result set</param>
        /// <returns>Database containing the results of the search</returns>
        public static DataTable ExecuteSQL(DbCommand sqlComm, string strTableName)
        {
            SqlDataAdapter adapter = new SqlDataAdapter();
            DataTable dtResults = new DataTable(strTableName);

            adapter = new SqlDataAdapter((SqlCommand)sqlComm);
            adapter.Fill(dtResults);
            return dtResults;
        }

        /// <summary>
        /// Converts date into string in format mm/dd/yy.
        /// </summary>
        /// <param name="dtDate">Date to convert.</param>
        /// <returns>String in the format mm/dd/yy.</returns>
        /// <history>
        /// 	[abristow]	27/08/04	Created
        /// </history>
        public static string FixDateForSQL(DateTime dtDate)
        {
            return dtDate.Month.ToString() + "/" + dtDate.Day.ToString() + "/" + dtDate.Year.ToString();
        }

        #endregion

        /// <summary>
        /// Validates the search criteria. This is done to prevent duplicates parameter names from occuring in the list.
        /// </summary>
        /// <param name="searchCrit">The search crit.</param>
        /// <returns></returns>
        internal static IEnumerable<Criteria> ValidateSearchCriteria(IEnumerable<Criteria> searchCrit)
        {
            if (searchCrit.IsNull()) return searchCrit;

            Dictionary<string, int> columnIndexDict = new Dictionary<string, int>();
            return SetCriteriaParamNames(ref columnIndexDict, searchCrit);
        }

        /// <summary>
        /// Sets the criteria param names of a parameter list.
        /// </summary>
        /// <param name="dict">The dict.</param>
        /// <param name="searchCrit">The search crit.</param>
        /// <returns></returns>
        internal static IEnumerable<Criteria> SetCriteriaParamNames(ref Dictionary<string, int> dict, IEnumerable<Criteria> searchCrit)
        {
            if (searchCrit.IsNull()) return searchCrit;

            foreach (Criteria crit in searchCrit)
            {
                crit.ParameterName = SetCriteriaParamName(ref dict, crit);
                if (crit.CriteriaChildren != null && crit.CriteriaChildren.Count() > 0)
                    SetCriteriaParamNames(ref dict, crit.CriteriaChildren);
            }
            return searchCrit;
        }

        /// <summary>
        /// Sets the name of the criteria param.
        /// </summary>
        /// <param name="dict">The dict.</param>
        /// <param name="crit">The crit.</param>
        /// <returns></returns>
        internal static string SetCriteriaParamName(ref Dictionary<string, int> dict, Criteria crit)
        {
            //If the column isn't in the dictionary...
            if (!dict.ContainsKey(crit.Column))
                dict.Add(crit.Column, 0);
            else
                dict[crit.Column] = ++dict[crit.Column];
            string number = string.Empty;
            if (dict[crit.Column] != 0)
                number = dict[crit.Column].ToString();
            crit.ParameterName = string.Format("{0}{1}", crit.Column, number);
            return crit.ParameterName;
        }

        /// <summary>
        /// Determines whether [is db server available].
        /// </summary>
        /// <returns>
        /// 	<c>true</c> if [is db server available]; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsDbServerAvailable()
        {
            throw new NotImplementedException();
        }
    }
}
