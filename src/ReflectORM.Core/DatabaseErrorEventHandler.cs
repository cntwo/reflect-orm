using System;
using System.Collections.Generic;
using System.Text;

namespace ReflectORM.Core
{
    /// <summary>
    /// Event delegate that contains event arguments associated with the database error.
    /// </summary>
    public delegate void DatabaseErrorEventHandler(DatabaseErrorEventArgs args);
}
