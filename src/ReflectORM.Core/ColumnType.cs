using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ReflectORM.Core
{
    /// <summary>
    /// The type of column used in the database
    /// </summary>
    public enum ColumnType
    {
        /// <summary>
        /// String
        /// </summary>
        String,
        /// <summary>
        /// Bool
        /// </summary>
        Bool,
        /// <summary>
        /// Int
        /// </summary>
        Int,
        /// <summary>
        /// Auto (this doesn't map to a database type, but let code handle the detection)
        /// </summary>
        Auto
    }
}
