using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using CBRI.Database;

namespace DatabaseCodeTester
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //Customer c = CustomerController.Instance.SGet(1);

            //c.CompanyName = "Campden";

            //CustomerController.Instance.SSave(c);

            Customer d = new Customer()
            {
                City = "London",
                Address = "Main Street",
                PostCode = "L18 9KD",
                CompanyName = "Company X",
                Country = "England"
            };

            SQLDbWriterDefault<Customer> writer = new SQLDbWriterDefault<Customer>("Server=SERVER18\\dev;Database=Test;Trusted_Connection=True;");

            writer.Save(d);

            //d = CustomerController.Instance.SSaveGet(d);
        }
    }
}
