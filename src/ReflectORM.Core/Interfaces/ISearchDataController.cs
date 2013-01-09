using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ReflectORM.Core.Interfaces
{
    /// <summary>
    /// An interface for SearchDataControllers
    /// </summary>
    public interface ISearchDataController
    {
        /// <summary>
        /// Gets the type.
        /// </summary>
        /// <value>The type.</value>
        Type Type { get; }
    }
}
