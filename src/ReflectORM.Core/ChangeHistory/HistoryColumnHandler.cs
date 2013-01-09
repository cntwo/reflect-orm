using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;

namespace ReflectORM.Core.ChangeHistory
{
    /// <summary>
    /// Data Handler for historycolumns
    /// </summary>
    public class HistoryColumnHandler : BaseDataController<HistoryColumn>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HistoryColumnHandler"/> class.
        /// </summary>
        /// <param name="connectionString">The connection string.</param>
        public HistoryColumnHandler(string connectionString) : base(connectionString) { }

        /// <summary>
        /// Gets the sort column.
        /// </summary>
        protected override string SortColumn
        {
            get
            {
                return IdColumn;
            }
        }

        /// <summary>
        /// Gets the latest record.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <param name="historyTableName">Name of the history table.</param>
        /// <param name="historyColumnTableName">Name of the history column table.</param>
        /// <param name="columnName">Name of the column.</param>
        /// <returns></returns>
        public virtual HistoryColumn GetLatestRecord(int id, string historyTableName, string historyColumnTableName, string columnName)
        {
            var retval = string.Empty;
            DbCommand command = ProfiledConnection.CreateCommand();
            Criteria columnCriterion = new Criteria("Column", ColumnType.String, columnName);
            Criteria objectCriterion = new Criteria("RecordId", ColumnType.String, "", id, historyTableName, "HistoryId", "Id");
            var generator = new SQLGenerator<HistoryColumn>(command);
            generator.Criteria.Add(columnCriterion);
            generator.Criteria.Add(objectCriterion);

            generator.OrderBy.Add(historyColumnTableName + "." + SortColumn);

            command.CommandText = generator.GenerateSelect(historyColumnTableName, 1);

            return SOperation(command, FromDb);
        }
    }
}