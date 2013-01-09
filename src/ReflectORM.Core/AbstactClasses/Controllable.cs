using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SqlTypes;
using System.Runtime.Serialization;
using ReflectORM.Attributes;

namespace ReflectORM.Core
{
    /// <summary>
    /// Controllable class.
    /// </summary>
    [DataContractAttribute()]
    public abstract class Controllable : Identifiable, IControllable
    {
        #region Fields
        
        /// <summary>
        /// Gets or sets the updated by.
        /// </summary>
        /// <value>The updated by.</value>
        [DataMember]
        [ControllableProperty(Auditable = false)]
        public virtual string CreatedBy { get; set; }

        /// <summary>
        /// Gets or sets the time the object was Created.
        /// </summary>
        /// <value></value>
        [DataMember]
        [ControllableProperty(Auditable = false)]
        public virtual DateTime CreatedOn { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="Controllable"/> class.
        /// </summary>
        public Controllable()
            : base()
        {
            CreatedBy = string.Empty;
            CreatedOn = SqlDateTime.MinValue.Value;
        }

        #endregion
    }
}
