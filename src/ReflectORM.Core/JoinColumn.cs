using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ReflectORM.Core
{
    /// <summary>
    /// Representing the columns used in a Join
    /// </summary>
    public class JoinColumn
    {
        /// <summary>
        /// Gets or sets the first column.
        /// </summary>
        /// <value>
        /// The first column.
        /// </value>
        public string FirstColumn { get; set; }
        /// <summary>
        /// Gets or sets the second column.
        /// </summary>
        /// <value>
        /// The second column.
        /// </value>
        public string SecondColumn { get; set; }
    }
}
