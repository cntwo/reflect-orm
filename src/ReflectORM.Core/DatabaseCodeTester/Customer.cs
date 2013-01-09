using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CBRI.Database;

namespace DatabaseCodeTester
{
    public class Customer : IControllable
    {
        #region IControllable Members

        public int Id
        {
            get;
            set;
        }

        [ControllableProperty(Read = false, Write = false)]
        public DateTime CreatedOn
        {
            get;
            set;
        }

        [ControllableProperty(Read = false, Write = false)]
        public DateTime Updated
        {
            get;
            set;
        }

        public string CompanyName { get; set; }

        public string Address { get; set; }

        public string City { get; set; }

        [ControllableProperty (ColumnName="PostalCode")]
        public string PostCode { get; set; }

        public string Country { get; set; }


        public Customer()
        {
            Id = -1;
        }
        #endregion
    }
}
