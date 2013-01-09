using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;

namespace ReflectORM.Core
{
    public class Criteria
    {
        public Criteria(string column, string value)
        {
            throw new NotImplementedException();
        }

        public Criteria(string column, ColumnType value, string columnName)
        {
            throw new NotImplementedException();
        }

        public Criteria(string column, ColumnType value, string columnName, string s, string columnName1, string historyColumnTableName)
        {
            throw new NotImplementedException();
        }

        public Criteria(string recordid, ColumnType value, string columnName, int id, string historyTableName, string historyColumnTableName, string s)
        {
            throw new NotImplementedException();
        }

        public Criteria(string recordid, object value)
        {
            throw new NotImplementedException();
        }

        public Criteria(string recordid, ColumnType value, bool columnName)
        {
            throw new NotImplementedException();
        }

        public Criteria(string idColumn, ColumnType value, int columnName)
        {
            throw new NotImplementedException();
        }

        public Criteria(string idColumn, ColumnType value, object columnName)
        {
            throw new NotImplementedException();
        }

        public string Column { get; set; }
        public string ParameterName { get; set; }

        public IEnumerable<Criteria> CriteriaChildren { get; set; }

        public string Table { get; set; }

        public IEnumerable<Join> Joins
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        public SearchType SearchType
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        public ColumnType Type
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        public void AddJoin(string historyTableName, string historyColumnTableName, string id, string historyid)
        {
            throw new NotImplementedException();
        }

        public string GetSQL(bool b, string usetable)
        {
            throw new NotImplementedException();
        }

        public ListDictionary GetParameters(ListDictionary parameters)
        {
            throw new NotImplementedException();
        }

        public string GetSQL(bool b)
        {
            throw new NotImplementedException();
        }

        public object GetParameterValue()
        {
            throw new NotImplementedException();
        }
    }
}
