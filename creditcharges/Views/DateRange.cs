using creditcharges.Models;
using System;
using System.Windows.Forms;

namespace creditcharges.Views
{
    public partial class DateRange : Form
    {
        #region Attributes
        private string sender;
        #endregion

        #region Constructor

        /// <summary>
        /// Displays a form from which the user can select a range of dates and sends back the values.
        /// </summary>
        /// <param name="sender"></param>
        public DateRange(string sender)
        {
            this.sender = sender;
            InitializeComponent();
        }
        #endregion

        #region Events
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

        #endregion
    }
}