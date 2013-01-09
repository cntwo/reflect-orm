using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ReflectORM.Attributes
{
    public class SearchScreenPropertyAttribute : Attribute
    {
        public SearchScreenPropertyAttribute()
        {
            Display = true;
            DisplayText = string.Empty;
            Prefix = string.Empty;
            Suffix = string.Empty;
            StringFormat = string.Empty;
        }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="SearchScreenPropertyAttribute"/> is display.
        /// </summary>
        /// <value><c>true</c> if display; otherwise, <c>false</c>.</value>
        public bool Display { get; set; }

        /// <summary>
        /// Gets or sets the display text.
        /// </summary>
        /// <value>The display text.</value>
        public string DisplayText { get; set; }

        /// <summary>
        /// Gets or sets the prefix.
        /// </summary>
        /// <value>The prefix.</value>
        public string Prefix { get; set; }

        /// <summary>
        /// Gets or sets the suffix.
        /// </summary>
        /// <value>The suffix.</value>
        public string Suffix { get; set; }

        /// <summary>
        /// Gets or sets the string format.
        /// </summary>
        /// <value>The string format.</value>
        public string StringFormat { get; set; }
    }
}
