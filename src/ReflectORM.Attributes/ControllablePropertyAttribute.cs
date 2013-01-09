using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ReflectORM.Attributes
{
    /// <summary>
    /// A custom attribute that allows for control over a property when it is use dynamically by ToDb and FromDb in the database code.
    /// </summary>
    public class ControllablePropertyAttribute : System.Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ControllablePropertyAttribute"/> class.
        /// </summary>
        public ControllablePropertyAttribute()
        {
            Read = true;
            Write = true;
            CreateBool = string.Empty;
            CreateBoolColumn = string.Empty;
            Coalesce = string.Empty;
            IncludeInResultsTable = false;
            Auditable = true;
        }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="ControllablePropertyAttribute"/> is read.
        /// </summary>
        /// <value><c>true</c> if read; otherwise, <c>false</c>.</value>
        public bool Read { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="ControllablePropertyAttribute"/> is write.
        /// </summary>
        /// <value><c>true</c> if write; otherwise, <c>false</c>.</value>
        public bool Write { get; set; }

        /// <summary>
        /// Gets or sets the name of the column. Use this if the database column name is different from the property name.
        /// </summary>
        /// <value>The name of the column.</value>
        public string ColumnName { get; set; }

        /// <summary>
        /// Gets or sets the name of the table.
        /// </summary>
        /// <value>The name of the table.</value>
        public string TableName { get; set; }

        /// <summary>
        /// Gets or sets the name of the parameter. Use this if the database parameter names differs from the column name and the property name.
        /// </summary>
        /// <value>The name of the parameter.</value>
        public string ParameterName { get; set; }

        /// <summary>
        /// Gets or sets the string split delimiter.
        /// If your property is a string, and needs to be split. Set the delimiter here.
        /// </summary>
        /// <value>The string split delimiter.</value>
        public char StringSplitDelimiter { get; set; }

        /// <summary>
        /// Gets or sets the type of the enum.
        /// </summary>
        /// <value>The type of the enum.</value>
        public Type EnumType { get; set; }

        /// <summary>
        /// Gets or sets the coalesce value (to replace NULL) to be used.
        /// </summary>
        /// <value>The coalesce value.</value>
        public string Coalesce { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [use coalesce].
        /// </summary>
        /// <value><c>true</c> if [use coalesce]; otherwise, <c>false</c>.</value>
        public bool UseCoalesce { get; set; }

        /// <summary>
        /// Gets or sets the create bool string, for checking a column and creating a bool instead.
        /// </summary>
        /// <value>The create bool.</value>
        public string CreateBool { get; set; }

        /// <summary>
        /// Gets or sets the name of the create bool column.
        /// </summary>
        /// <value>The name of the create bool.</value>
        public string CreateBoolColumn { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [include in results table].
        /// </summary>
        /// <value>
        /// 	<c>true</c> if [include in results table]; otherwise, <c>false</c>.
        /// </value>
        public bool IncludeInResultsTable { get; set; }

        /// <summary>
        /// Gets or sets the type of the SQL. If not set, will be 'guessed'
        /// </summary>
        /// <value>The type of the SQL.</value>
        public string SQLType { get; set; }


        /// <summary>
        /// Gets or sets whether this <see cref="ControllablePropertyAttribute"/> is auditable.
        /// </summary>
        /// <value>
        ///   <c>true</c> if auditable; otherwise, <c>false</c>.
        /// </value>
        public bool Auditable { get; set; }
    }
}
