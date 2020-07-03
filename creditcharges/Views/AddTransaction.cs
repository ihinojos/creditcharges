using creditcharges.Models;
using DevExpress.XtraBars.Docking2010;
using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Windows.Forms;

namespace creditcharges.Views
{
    [Obsolete]
    public partial class AddTransaction : Form
    {
        private double value;
        private readonly SqlConnection sql;

        public AddTransaction()
        {
            InitializeComponent();
            sql = new SqlConnection(Data.cn);
            dateBox.Value = DateTime.Now;

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
            var user = Controller.controller.mainForm.User;
            var query = "INSERT INTO Records (Id, CardHolder, Card, Concept, Amount, TDate, Notes, Author) " +
                            "VALUES (@id, @name, @card, @concept, @amount, @date, @notes, @user)";

            SqlCommand cmd = new SqlCommand(query, sql);
            cmd.Parameters.AddWithValue("@id", SqlDbType.VarChar).Value = id;
            cmd.Parameters.AddWithValue("@name", SqlDbType.VarChar).Value = employee;
            cmd.Parameters.AddWithValue("@card", SqlDbType.Int).Value = number;
            cmd.Parameters.AddWithValue("@concept", SqlDbType.VarChar).Value = concept;
            cmd.Parameters.AddWithValue("@amount", SqlDbType.Decimal).Value = val;
            cmd.Parameters.AddWithValue("@date", SqlDbType.DateTime).Value = date;
            cmd.Parameters.AddWithValue("@notes", SqlDbType.Text).Value = notes;
            cmd.Parameters.AddWithValue("@user", SqlDbType.VarChar).Value = user;
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

                            if (checkBox1.Checked)
                            {
                                query = "INSERT INTO Fuel (Id, Odometer, Plate, Model, Gallons) VALUES (@id, @odometer, @plate, @model, @gallons)";
                                var odometer = int.Parse(odometerBox.Text);
                                var plate = plateBox.Text;
                                var model = modelBox.Text;
                                var gallons = decimal.Parse(gallonsBox.Text);
                                cmd.CommandText = query;
                                cmd.Parameters.AddWithValue("@odometer", SqlDbType.Int).Value = odometer;
                                cmd.Parameters.AddWithValue("@plate", SqlDbType.VarChar).Value = plate;
                                cmd.Parameters.AddWithValue("@model", SqlDbType.VarChar).Value = model;
                                cmd.Parameters.AddWithValue("@gallons", SqlDbType.Decimal).Value = gallons;
                                res = cmd.ExecuteNonQuery();
                            }
                            if (res == 1) MessageBox.Show("Record saved successfully.", "Success");
                            else MessageBox.Show("Please check your internet connection.", "Error");
                        }
                    }
                }
                cmd.Connection.Close();
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

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            odometerBox.Enabled = checkBox1.Checked;
            plateBox.Enabled = checkBox1.Checked;
            modelBox.Enabled = checkBox1.Checked;
            gallonsBox.Enabled = checkBox1.Checked;
        }

        private void odometerBox_Leave(object sender, EventArgs e)
        {
            try
            {
                int.Parse(odometerBox.Text.Trim());
            }
            catch
            {
                odometerBox.Text = string.Empty;
            }
        }

        private void litersBox_Leave(object sender, EventArgs e)
        {
            try
            {
                decimal.Parse(gallonsBox.Text);
            }
            catch
            {
                gallonsBox.Text = string.Empty;
            }
        }
    }
}