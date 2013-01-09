using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using System.Data.Common;

namespace ReflectORM.Core
{
    /// <summary>
    /// A delegate for setting up parameters.
    /// </summary>
    /// <param name="data">The data for the parameters</param>
    /// <param name="command">The DbCommand that has the parameters to set.</param>
    public delegate void SetParameterDelegate<T>(T data, ref DbCommand command);

    /// <summary>
    /// A delegate for getting the data from a datareader.
    /// </summary>
    /// <param name="reader">The datareader that has the data.</param>
    /// <returns>A completed T.</returns>
    public delegate T GetDataDelegate<T>(DbDataReader reader);
}
