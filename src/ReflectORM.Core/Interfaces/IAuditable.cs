using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ReflectORM.Core
{
    /// <summary>
    /// An interface to provide audit information to an object
    /// </summary>
    public interface IAuditable : IControllable
    {
        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="IAuditable"/> is deleted.
        /// </summary>
        /// <value><c>true</c> if deleted; otherwise, <c>false</c>.</value>
        bool Deleted { get; set; }

        /// <summary>
        /// Gets or sets the changed by.
        /// </summary>
        /// <value>The changed by.</value>
        string EditedBy { get; set; }

        /// <summary>
        /// Gets or sets the changed on.
        /// </summary>
        /// <value>The changed on.</value>
        DateTime EditedOn { get; set; }
    }
}
