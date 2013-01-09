using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;

namespace ReflectORM.Core.ChangeHistory
{
    /// <summary>
    /// Data handler for the history class
    /// </summary>
    public class HistoryHandler : BaseDataController<History>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HistoryHandler"/> class.
        /// </summary>
        /// <param name="connectionString">The connection string.</param>
        public HistoryHandler(string connectionString)
            : base(connectionString)
        { }

        protected override string SortColumn
        {
            get
            {
                return IdColumn;
            }
        }

        public virtual History GetLatestRecord(int id, string historyTableName, string historyColumnTableName, string columnName)
        {
            DbCommand command = ProfiledConnection.CreateCommand();

            Criteria columnCriterion = new Criteria("Column", ColumnType.String, "", "Column", columnName, historyColumnTableName);
            columnCriterion.AddJoin(historyTableName, historyColumnTableName, "Id", "HistoryId");
            columnCriterion.SearchType = SearchType.Equals;

            Criteria objectCriterion = new Criteria("RecordId", ColumnType.Int, id);
            objectCriterion.SearchType = SearchType.Equals;

            var generator = new SQLGenerator<History>(command);

            generator.Criteria.Add(columnCriterion);
            generator.Criteria.Add(objectCriterion);

            generator.OrderBy.Add(historyTableName + "." + "CreatedOn" + " DESC");

            command.CommandText = generator.GenerateSelect(historyTableName, 1);

            return SOperation(command, FromDb);


            //SearchCriteria catIdCrit = new SearchCriteria("DepartmentalCategoryId", ColumnType.Int, "Departmental Category Id", "DepartmentalCategoryId", id, "AssetDepartmentalCategory");
            //catIdCrit.AddJoin("Asset", "AssetDepartmentalCategory", "AssetId", "AssetId");
            //catIdCrit.SearchType = SearchType.Equals;

            //SearchCriteria plannerCriterion = new SearchCriteria("Planner", ColumnType.Bool, true);
            //plannerCriterion.SearchType = SearchType.Equals;

            //SQLGenerator<Data.Asset> generator = new SQLGenerator<Data.Asset>(command);
            //generator.Criteria.Add(catIdCrit);
            //generator.Criteria.Add(plannerCriterion);
            //generator.OrderBy.Add("AssetNumber");
        }
    }
}