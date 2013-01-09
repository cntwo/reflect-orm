using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;
using CBRI.Database;
//using CBRIFocalPoint.BudgetPackBusinessObject;

namespace DatabaseCodeTester
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {

            Console.WriteLine(DbUtilities.MakeSQLConnectionString("focalpoint_db", "CampdenTestsystem", Environment.MachineName, "focalpointuser", "campdenbri", false));
            DbConnection thisConnection = DbUtilities.GetOpenConnection("focalpoint_db", "CampdenTestsystem", Environment.MachineName, "focalpointuser", "campdenbri", false);

            //BudgetPack budgetPack = BudgetPackController.Instance.GetBudgetPack("CampdenTestSystem", 98);
            //int findindex = budgetPack.ExternalIncome.FindIndex(delegate(ExternalIncome externalIncome) { return externalIncome.Id == 278; });
            //if (findindex > 0) budgetPack.ExternalIncome[findindex].ToBeDeleted = true;
            //BudgetPackController.Instance.SaveBudgetPack("CampdenTestSystem", budgetPack, "0029");

            //Application.EnableVisualStyles();
            //Application.SetCompatibleTextRenderingDefault(false);
            //Application.Run(new Form1());
        }
    }
}
