using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ReflectORM.Core
{
    /// <summary>
    /// A custom exception that is thrown when a parameter already exists in a command object
    /// </summary>
    public class ParameterExistsException : Exception
    {
        string _paramaterName = string.Empty;

        /// <summary>
        /// Gets a message that describes the current exception.
        /// </summary>
        /// <value></value>
        /// <returns>
        /// The error message that explains the reason for the exception, or an empty string("").
        /// </returns>
        public override string Message
        {
            get
            {
                return string.Format("A parameter with the name {0}, already exists in the command object. Please consider renaming one or both of the parameters.", _paramaterName);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ParameterExistsException"/> class.
        /// </summary>
        /// <param name="parameterName">Name of the parameter.</param>
        public ParameterExistsException(string parameterName) : base()
        {
            _paramaterName = parameterName;
        }
    }
}
