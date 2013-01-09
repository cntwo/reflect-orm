using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ReflectORM.Core.ChangeHistory
{
    /// <summary>
    /// Information about a column when a record is inserted/updated/deleted
    /// </summary>
    public class HistoryColumn
    {
        /// <summary>
        /// Gets or sets the id.
        /// </summary>
        /// <value>The id.</value>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the column.
        /// </summary>
        /// <value>The column.</value>
        public string Column { get; set; }

        /// <summary>
        /// Gets or sets the old value.
        /// </summary>
        /// <value>The old value.</value>
        public object OldValue { get; set; }

        /// <summary>
        /// Gets or sets the new value.
        /// </summary>
        /// <value>The new value.</value>
        public object NewValue { get; set; }

        /// <summary>
        /// Gets or sets the history id.
        /// </summary>
        /// <value>The history id.</value>
        public int HistoryId { get; set; }
    }
}
