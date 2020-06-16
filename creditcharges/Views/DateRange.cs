using creditcharges.Models;
using System;
using System.Windows.Forms;

namespace creditcharges.Views
{
    public partial class DateRange : Form
    {
        public DateRange()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                var date1 = (DateTime)dateEdit1.EditValue;
                var date2 = (DateTime)dateEdit2.EditValue;
                Controller.controller.mainForm.GetDataSource(date1, date2);
                Dispose();
            }
            catch { }
        }
    }
}