using System;
using System.Collections.Generic;
using System.Text;

namespace ReflectORM.Core
{
    /// <summary>
    /// Interface for objects that are identified with an Id property.
    /// </summary>
    public interface IIdentifiable
    {
        /// <summary>
        /// Gets or sets the id.
        /// </summary>
        /// <value>The id.</value>
        int Id { get; set; }
    }
}
