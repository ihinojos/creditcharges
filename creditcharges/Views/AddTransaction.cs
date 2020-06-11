using creditcharges.Models;
using DevExpress.XtraBars.Docking2010;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace creditcharges.Views
{
    public partial class AddTransaction : Form
    {
        private double value;
        private readonly SqlConnection sql;
        public AddTransaction()
        {
            InitializeComponent();
            sql = new SqlConnection(Data.cn);
            dateBox.Value = DateTime.Now;
            dateBox.Enabled = false;

            AutoCompleteStringCollection names = new AutoCompleteStringCollection();
            names.AddRange(Data.names.ToArray());
            employeeBox.AutoCompleteCustomSource = names;

            AutoCompleteStringCollection cards = new AutoCompleteStringCollection();
            cards.AddRange(Data.childCards.ToArray());
            cardBoxNum.AutoCompleteCustomSource = cards;
        }

        private void windowsUIButtonPanel1_ButtonClick(object sender, ButtonEventArgs e)
        {
            var tag = ((WindowsUIButton)e.Button).Tag.ToString();
            switch (tag)
            {
                case "save":
                    AddRecord();
                    break;
                case "cancel":
                    Dispose();
                    break;
            }
        }

        private void AddRecord()
        {
            var id = Guid.NewGuid().ToString("N");
            var employee = employeeBox.Text;
            var number = cardBoxNum.Text;
            var val = value;
            var concept = conceptBox.Text;
            var date = dateBox.Value;
            var notes = notesBox.Text;
            var query = "INSERT INTO Records (Id, CardHolder, Card, Concept, Amount, TDate, Notes) " +
                            "VALUES (@id, @name, @card, @concept, @amount, @date, @notes)";

            SqlCommand cmd = new SqlCommand(query, sql);
            cmd.Parameters.AddWithValue("@id", SqlDbType.VarChar).Value = id;
            cmd.Parameters.AddWithValue("@name", SqlDbType.VarChar).Value = employee;
            cmd.Parameters.AddWithValue("@card", SqlDbType.Int).Value = number;
            cmd.Parameters.AddWithValue("@concept", SqlDbType.VarChar).Value = concept;
            cmd.Parameters.AddWithValue("@amount", SqlDbType.Decimal).Value = val;
            cmd.Parameters.AddWithValue("@date", SqlDbType.DateTime).Value = date;
            cmd.Parameters.AddWithValue("@notes", SqlDbType.Text).Value = notes;
            if (!(string.IsNullOrEmpty(employee) || string.IsNullOrEmpty(number) || string.IsNullOrEmpty(concept) || string.IsNullOrEmpty(number)))
            {
                cmd.Connection.Open();
                var rel = cmd.ExecuteNonQuery();
                if (rel > 0)
                {
                    query = "SELECT Main FROM ChildCards WHERE Card = @card";
                    cmd.CommandText = query;
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            var main = reader[0] as string;
                            query = "INSERT INTO QuickBooks (Id, Card) VALUES (@id, @main)";
                            cmd.CommandText = query;
                            cmd.Parameters.AddWithValue("@main", SqlDbType.VarChar).Value = main;
                            reader.Close();
                            var res = cmd.ExecuteNonQuery();
                            if (res == 1) MessageBox.Show("Record saved successfully.", "Success");
                            else MessageBox.Show("Please check your internet connection.", "Error");
                        }
                    }
                }
                cmd.Connection.Close();
                if (!Data.names.Contains(employee))
                {
                    using (StreamWriter w = File.AppendText("Resources\\names.txt")) w.WriteLine(employee);
                    Data.getData();
                }
                Controller.controller.mainForm.LoadTable();
                Dispose();
            }
            else MessageBox.Show("There are empty fields.");
        }

        private void amountBox_Leave(object sender, EventArgs e)
        {
            if (double.TryParse(amountBox.Text, out value))
                amountBox.Text = string.Format(System.Globalization.CultureInfo.CurrentCulture, "{0:C2}", value);
            else
                amountBox.Text = string.Empty;
        }
    }
}
