using System;
using System.Collections.Generic;
using System.Text;

namespace ReflectORM.Core
{
    /// <summary>
    /// Interface for objects that can be persisted.
    /// </summary>
    public interface IControllable : IIdentifiable
    {        
        /// <summary>
        /// Gets or sets the time the object was Created.
        /// </summary>
        DateTime CreatedOn { get; set;}

        /// <summary>
        /// Gets or sets the created by.
        /// </summary>
        /// <value>The created by.</value>
        string CreatedBy { get; set; }
    }
}
