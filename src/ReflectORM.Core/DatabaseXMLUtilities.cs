using System;
using System.Data;
using System.Data.SqlTypes;
using System.Collections;
using System.Text;
using System.IO;
using System.Xml;

namespace ReflectORM.Core
{
    /// <summary>
    /// Create SQL XML data type
    /// </summary>
    public static class CreateSQLXMLDataType
    {
        /// <summary>
        /// Return XML for an array of items
        /// </summary>
        /// <param name="List">List of items</param>
        /// <returns></returns>
        public static SqlXml GetXml(IEnumerable List)
        {
            return GetXml(List, "List", "Item");
        }

        /// <summary>
        /// Return XML for an array of items
        /// </summary>
        /// <param name="List">List of items</param>
        /// <param name="ListName">Name of the list</param>
        /// <param name="ItemName">Name of the item in the list.</param>
        /// <returns></returns>
        public static SqlXml GetXml(IEnumerable List, string ListName, string ItemName)
        {
            //We don't use 'using' or dispose or close the stream, 
            //since it leaves in the return variable
            MemoryStream stream = new MemoryStream();
            SqlXml Result = null;
            try
            {
                using (XmlWriter writer = XmlWriter.Create(stream))
                {
                    writer.WriteStartElement(ListName);
                    foreach (object obj in List)
                    {
                        writer.WriteElementString(ItemName, obj.ToString());
                    }
                    writer.WriteEndElement();
                    Result = new SqlXml(stream);
                }
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
            }
            return Result;
        }
    }
}
