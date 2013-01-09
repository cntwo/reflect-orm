using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CBRI.Database;

namespace DatabaseCodeTester
{
    public class CustomerController : BaseDataController<Customer>
    {

		#region Singleton
		private static CustomerController _instance = null;

		/// <summary>
        /// Gets the single CustomerController instance.
        /// </summary>
		public static CustomerController Instance
		{
			get
			{
				if(_instance == null) _instance = new CustomerController();
				return _instance;
			}
		}

		private CustomerController():base("Server=SERVER18\\dev;Database=Test;Trusted_Connection=True;")	{  }
		#endregion


        protected override bool IsUpdatable(Customer data)
        {
            return data.Id != DbConstants.DEFAULT_ID;
        }
    }
}
