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
    public abstract class Auditable : Controllable, IAuditable
    {
        #region Fields

        /// <summary>
        /// Gets or sets the updated by.
        /// </summary>
        /// <value>The updated by.</value>
        [DataMember]
        [ControllableProperty(Auditable = false)]
        public virtual string EditedBy { get; set; }

        /// <summary>
        /// Gets or sets the time the object was Created.
        /// </summary>
        /// <value></value>
        [DataMember]
        [ControllableProperty(Auditable = false)]
        public virtual DateTime EditedOn { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="Auditable"/> is deleted.
        /// </summary>
        /// <value><c>true</c> if deleted; otherwise, <c>false</c>.</value>
        [DataMember]
        public virtual bool Deleted { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="Controllable"/> class.
        /// </summary>
        public Auditable()
            : base()
        {
            EditedBy = string.Empty;
            EditedOn = SqlDateTime.MinValue.Value;
            Deleted = false;
        }

        #endregion
    }
}
