using creditcharges.Models;
using System;
using System.Windows.Forms;

namespace creditcharges.Views
{
    public partial class DateRange : Form
    {
        private string sender;

        public DateRange(string sender)
        {
            this.sender = sender;
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                var date1 = (DateTime)dateEdit1.EditValue;
                var date2 = (DateTime)dateEdit2.EditValue;
                if(this.sender == "report")
                    Controller.controller.mainForm.GetDataSource(date1, date2);
                else if (this.sender == "diesel")
                    Controller.controller.mainForm.GetDieselSource(date1, date2);

            }
            catch { }
            finally
            {
                Dispose();
            }
        }
    }
}