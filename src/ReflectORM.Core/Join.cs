using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ReflectORM.Core
{
    public class Join
    {
        public string FirstTable { get; set; }
        public string SecondTable { get; set; }
        public IEnumerable<JoinColumn> JoinColumns { get; set; }

        public string GetSQL()
        {
            throw new NotImplementedException();
        }

        public string GetSQL(string firstTable, string secondTable)
        {
            throw new NotImplementedException();
        }

        public string GetSQL(Join join)
        {
            throw new NotImplementedException();
        }
    }
}
