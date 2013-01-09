using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace ReflectORM.Extensions
{
    public static class IDataRecordExtensions
    {
        /// <summary>
        /// Checks whether the given field name exists in the collection of fields.
        /// </summary>
        /// <param name="record">The IDataRecord that is being extended.</param>
        /// <param name="fieldName">The name of the field that is being 
        /// searched for.</param>
        /// <returns><c>true</c> when the field exist in the IDataRecord fields collection; 
        /// <c>false</c> otherwise.</returns>
        public static bool FieldExists(this IDataRecord record, string fieldName)
        {
            for (int i = 0; i < record.FieldCount; i++)
            {
                string tempName = record.GetName(i);
                if (fieldName.ToUpperInvariant() == tempName.ToUpperInvariant())
                    return true;
            }
            return false;
        }

    }
}
