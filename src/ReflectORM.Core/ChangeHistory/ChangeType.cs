using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ReflectORM.Core.ChangeHistory
{
    /// <summary>
    /// The type of change being made to the record
    /// </summary>
    public enum ChangeType
    {
        /// <summary>
        /// The record is being instered
        /// </summary>
        Insert,
        /// <summary>
        /// The record is being updated
        /// </summary>
        Update,
        /// <summary>
        /// The record is being deleted
        /// </summary>
        Delete,
        /// <summary>
        /// The change type has not been set
        /// </summary>
        NotSet
    }
}
