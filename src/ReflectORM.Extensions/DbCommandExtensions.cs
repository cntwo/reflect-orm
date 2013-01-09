using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Common;

namespace ReflectORM.Extensions
{
    public static class DbCommandExtensions
    {
        /// <summary>
        /// Adds a parameter to the command.
        /// </summary>
        /// <param name="command">
        /// The command.
        /// </param>
        /// <param name="parameterName">
        /// Name of the parameter.
        /// </param>
        /// <param name="parameterValue">
        /// The parameter value.
        /// </param>
        /// <remarks>
        /// </remarks>
        public static void AddParameterWithValue(this DbCommand command, string parameterName, object parameterValue)
        {
            var parameter = command.CreateParameter();
            parameter.ParameterName = parameterName;
            parameter.Value = parameterValue;
            command.Parameters.Add(parameter);
        }

        public static DbCommand CreateCommand(this DbConnection connection, string commandText)
        {
            return CreateCommand(connection, commandText, null);
        }

        public static DbCommand CreateCommand(this DbConnection connection, string commandText, DbTransaction transaction)
        {
            var comm = connection.CreateCommand();
            comm.CommandText = commandText;
            
            if(transaction != null)
                comm.Transaction = transaction;

            return comm;
        }
    }
}
