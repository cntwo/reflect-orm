using System;
using System.Collections.Generic;
using System.Text;

namespace ReflectORM.Core
{
    /// <summary>
    /// An enumaration of Database Error Types.
    /// </summary>
    public enum DatabaseErrorType
    {
        /// <summary>
        /// Identifies that the Database Error is critical.
        /// </summary>
        Critical,
        /// <summary>
        /// Identifies that the Database Error is managable.
        /// </summary>
        Manageble,
        /// <summary>
        /// Identifies that the Database Error has been generated during a series of transactions.
        /// </summary>
        Transactional
    }
}
