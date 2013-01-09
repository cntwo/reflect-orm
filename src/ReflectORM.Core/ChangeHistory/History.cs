using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ReflectORM.Core.ChangeHistory
{
    /// <summary>
    /// Information about a version of an object
    /// </summary>
    public class History
    {
        /// <summary>
        /// Gets or sets the id.
        /// </summary>
        /// <value>The id.</value>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the record id.
        /// </summary>
        /// <value>The record id.</value>
        public int RecordId { get; set; }

        /// <summary>
        /// Gets or sets the created by.
        /// </summary>
        /// <value>The created by.</value>
        public string CreatedBy { get; set; }

        /// <summary>
        /// Gets or sets the created on.
        /// </summary>
        /// <value>The created on.</value>
        public DateTime CreatedOn { get; set; }

        /// <summary>
        /// Gets or sets the comment.
        /// </summary>
        /// <value>The comment.</value>
        public string Comment { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="History"/> is committed.
        /// </summary>
        /// <value><c>true</c> if committed; otherwise, <c>false</c>.</value>
        public bool Committed { get; set; }

        /// <summary>
        /// Gets or sets the type.
        /// </summary>
        /// <value>The type.</value>
        public ChangeType Type { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="History"/> is published.
        /// </summary>
        /// <value><c>true</c> if published; otherwise, <c>false</c>.</value>
        public bool Published { get; set; }

        /// <summary>
        /// Gets or sets the columns.
        /// </summary>
        /// <value>The columns.</value>
        [ReflectORM.Attributes.ControllablePropertyAttribute(Read = false, Write = false)]
        public List<HistoryColumn> Columns { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="History"/> class.
        /// </summary>
        public History()
        {
            Type = ChangeType.NotSet;
        }
    }
}
