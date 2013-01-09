using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ReflectORM.Core
{
    /// <summary>
    /// Representing an SQL join
    /// </summary>
    public class Join
    {
        /// <summary>
        /// Gets or sets the first table.
        /// </summary>
        /// <value>
        /// The first table.
        /// </value>
        public string FirstTable { get; set; }
        /// <summary>
        /// Gets or sets the second table.
        /// </summary>
        /// <value>
        /// The second table.
        /// </value>
        public string SecondTable { get; set; }
        /// <summary>
        /// Gets or sets the join columns.
        /// </summary>
        /// <value>
        /// The join columns.
        /// </value>
        public IEnumerable<JoinColumn> JoinColumns { get; set; }

        /// <summary>
        /// Gets the SQL.
        /// </summary>
        /// <returns></returns>
        public string GetSQL()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets the SQL.
        /// </summary>
        /// <param name="firstTable">The first table.</param>
        /// <param name="secondTable">The second table.</param>
        /// <returns></returns>
        public string GetSQL(string firstTable, string secondTable)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets the SQL.
        /// </summary>
        /// <param name="join">The join.</param>
        /// <returns></returns>
        public string GetSQL(Join join)
        {
            throw new NotImplementedException();
        }
    }
}
