using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Serialization;
using System.ComponentModel;
using System.Linq.Expressions;
using ReflectORM.Attributes;

namespace ReflectORM.Core
{
    /// <summary>
    /// Identifiable class.
    /// </summary>
    [DataContractAttribute()]
    public abstract class Identifiable : IIdentifiable
    {
        #region Fields

        /// <summary>
        /// Gets or sets the ID of the object.
        /// </summary>
        /// <value></value>
        [ControllableProperty(Auditable = false)]
        [DataMember]
        public virtual int Id { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="Identifiable"/> class.
        /// </summary>
        public Identifiable()
        {
            Id = DbConstants.DEFAULT_ID;
        }

        #endregion
    }
}
