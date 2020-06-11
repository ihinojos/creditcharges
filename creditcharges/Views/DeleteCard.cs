using creditcharges.Models;
using DevExpress.XtraBars.Docking2010;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace creditcharges.Views
{
    public partial class DeleteCard : Form
    {
        private readonly SqlConnection sql;
        public DeleteCard()
        {
            InitializeComponent();
            sql = new SqlConnection(Data.cn);
            AutoCompleteStringCollection cards = new AutoCompleteStringCollection();
            cards.AddRange(Data.childCards.ToArray());
            childCard.AutoCompleteCustomSource = cards;
        }

        private void windowsUIButtonPanel1_ButtonClick(object sender, ButtonEventArgs e)
        {
            var tag = ((WindowsUIButton)e.Button).Tag.ToString();
            switch (tag)
            {
                case "delete":
                    deleteCard();
                    break;
                case "cancel":
                    Dispose();
                    break;
            }
        }

        private void deleteCard()
        {
            var child = childCard.Text;
            if (!string.IsNullOrEmpty(child))
            {
                string message = "Do you want to delete '"+child+"'?";
                MessageBoxButtons buttons = MessageBoxButtons.YesNo;
                DialogResult result = MessageBox.Show(message, "", buttons);
                if (result == DialogResult.Yes)
                {
                    var query = "DELETE FROM ChildCards WHERE Card = @card ";
                    SqlCommand cmd = new SqlCommand(query, sql);
                    cmd.Parameters.AddWithValue("@card", SqlDbType.VarChar).Value = child;
                    cmd.Connection.Open();
                    var res = cmd.ExecuteNonQuery();
                    if (res == 1) MessageBox.Show("The card has been deleted", "Success");
                    else MessageBox.Show("Plase check your internet connection", "Error");
                    cmd.Connection.Close();
                }
            }
        }

    }
}
