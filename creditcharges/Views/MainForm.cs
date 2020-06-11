﻿using creditcharges.Models;
using DevExpress.Data.Mask;
using DevExpress.Utils;
using DevExpress.XtraBars.Docking2010;
using DevExpress.XtraGrid.Columns;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraGrid.Views.Grid.ViewInfo;
using Microsoft.VisualBasic.FileIO;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;

namespace creditcharges.Views
{
    public partial class MainForm : Form
    {
        public readonly bool ADMIN;
        private bool report = false;
        private readonly SqlConnection sql;
        public MainForm(bool admin)
        {
            ADMIN = admin;
            InitializeComponent();
            sql = new SqlConnection(Data.cn);
            LoadTable();
            addCardToolStripMenuItem.Enabled = ADMIN;

        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {

            Controller.controller.logIn.Show();
        }

        private void windowsUIButtonPanel1_ButtonClick(object sender, DevExpress.XtraBars.Docking2010.ButtonEventArgs e)
        {
            try
            {
                var tag = ((WindowsUIButton)e.Button).Tag.ToString();
                switch (tag)
                {
                    case "print":
                        generalGrid.ShowRibbonPrintPreview();
                        break;
                    case "new":
                        AddTransaction();
                        break;
                    case "edit":
                        EditTransaction(generalGridView);
                        break;
                    case "delete":
                        DeleteRecord();
                        break;
                    case "refresh":
                        LoadTable();
                        break;
                }
            }
            catch { }
        }

        private void DeleteRecord()
        {
            try
            {
                var id = generalGridView.GetRowCellValue(generalGridView.FocusedRowHandle, "Id").ToString();

                string message = "Do you want to delete this record?";
                MessageBoxButtons buttons = MessageBoxButtons.YesNo;
                DialogResult result = MessageBox.Show(message, "", buttons);
                if (result == DialogResult.Yes)
                {
                    var query = "DELETE FROM QuickBooks WHERE Id = @id";
                    SqlCommand cmd = new SqlCommand(query, sql);
                    cmd.Parameters.AddWithValue("@id", SqlDbType.VarChar).Value = id;
                    cmd.Connection.Open();
                    var res = cmd.ExecuteNonQuery();
                    cmd.CommandText = "DELETE FROM Images WHERE Id = @id";
                    var ret = cmd.ExecuteNonQuery();
                    cmd.CommandText = "DELETE FROM Records WHERE Id = @id";
                    var rel = cmd.ExecuteNonQuery();
                    cmd.Connection.Close();
                    if (res > 0 && rel > 0 && ret > 0) MessageBox.Show("Record deleted.");
                    dateEdit1.EditValue = null;
                }
            }
            catch (NullReferenceException)
            {
                MessageBox.Show("Please select a record.");
            }

        }

        public void LoadTable()
        {
            if (generalGrid.DataSource != null) generalGrid.DataSource = null;
            generalGrid.DataSource = GetDataSource();
            GridColumn price = generalGridView.Columns["Amount"];
            price.DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
            price.DisplayFormat.FormatString = "c2";
            generalGridView.Columns[2].Visible = false;
            generalGridView.Columns[7].Visible = false;
            generalGridView.Columns[8].Visible = false;
            generalGridView.Columns[9].Visible = false;
            generalGridView.Columns[10].Visible = false;
            generalGridView.Columns[11].Visible = false;
            generalGridView.Columns[12].Visible = false;
            generalGridView.BestFitColumns();
        }

        private void AddTransaction()
        {
            var instance = Controller.controller.addTransaction;
            if (instance != null) instance.Dispose();
            instance = Controller.controller.addTransaction = new AddTransaction();
            instance.Show();
        }

        private void EditTransaction(GridView view)
        {
            var flag = false;
            var id = "";
            try { id = view.GetRowCellValue(view.FocusedRowHandle, "Id").ToString(); }
            catch (NullReferenceException)
            {

                var msg = "";
                switch (view.Tag)
                {
                    case "general":
                        msg = "Please select a transaction to edit.";
                        break;
                    case "detail":
                        msg = "There is no record associated with the report.";
                        break;
                }
                MessageBox.Show(msg, "Error");
                flag = true;
            }
            if (!flag && view == generalGridView)
            {
                var instance = Controller.controller.editTransaction;
                if (instance != null) instance.Dispose();
                instance = Controller.controller.editTransaction = new EditTransaction(id, "", "");
                instance.Show();
            }
            else if (!flag && view == detailsGridView)
            {
                var concept = view.GetRowCellValue(view.FocusedRowHandle, "Concept").ToString();
                var location = view.GetRowCellValue(view.FocusedRowHandle, "Location").ToString();
                var instance = Controller.controller.editTransaction;
                if (instance != null) instance.Dispose();
                instance = Controller.controller.editTransaction = new EditTransaction(id, location, concept);
                instance.Show();
            }
        }

        private BindingList<Transaction> GetDataSource()
        {
            BindingList<Transaction> result = new BindingList<Transaction>();
            var date = DateTime.Now.ToString("yyyy-MM-dd");
            var query = "SELECT * FROM Records WHERE TDate >= @today";
            SqlCommand cmd = new SqlCommand(query, sql);
            cmd.Parameters.AddWithValue("@today", SqlDbType.DateTime).Value = date;
            cmd.Connection.Open();
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    result.Add(new Transaction()
                    {
                        Id = reader[0] as string,
                        Employee = reader[1] as string,
                        Card = reader[2] as string,
                        Amount = reader.GetDecimal(3),
                        Concept = reader[4] as string,
                        Location = reader[5] as string,
                        Date = reader.GetDateTime(6),
                        Notes = reader[7] as string
                    });
                }
                cmd.Connection.Close();
            }
            return result;
        }


        private void addCardToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var instance = Controller.controller.addCard;
            if (instance != null) instance.Dispose();

            instance = Controller.controller.addCard = new AddCard();

            instance.Show();
        }

        private void deleteCardToolStripMenuItem_Click(object sender, EventArgs e)
        {

            var instance = Controller.controller.delCard;
            if (instance != null) instance.Dispose();

            instance = Controller.controller.delCard = new DeleteCard();

            instance.Show();
        }

        private void windowsUIButtonPanel2_ButtonClick(object sender, ButtonEventArgs e)
        {
            try
            {
                var tag = ((WindowsUIButton)e.Button).Tag.ToString();
                switch (tag)
                {
                    case "load":
                        LoadReport();
                        break;
                    case "fill":
                        EditTransaction(detailsGridView);
                        break;
                    case "print":
                        detailsGrid.ShowRibbonPrintPreview();
                        break;
                    case "details":
                        DateRangeForm();
                        break;
                }
            }
            catch { }
        }

        private void DateRangeForm()
        {
            var instance = Controller.controller.dateRange;
            if (instance != null) instance.Dispose();
            instance = Controller.controller.dateRange = new DateRange();
            instance.Show();
        }

        public void LoadCustomDates(DateTime start, DateTime end)
        {
            report = false;
            BindingList<Transaction> result = new BindingList<Transaction>();
            var date = start.ToString("yyyy-MM-dd");
            end = end.AddDays(1);
            var tomorrow = end.ToString("yyyy-MM-dd");
            
            var query = "SELECT R.Id, R.TDate, R.Card, R.Location, R.Concept, R.Amount, R.CardHolder, " +
            "Q.Account, Q.Entity, Q.Class, Q.JobNumber, Q.JobName, Q.Card, R.Notes " +
            "FROM Records R JOIN QuickBooks Q ON R.Id = Q.Id "+
            "WHERE R.TDate >= @today AND R.TDate < @tomorrow ";

            SqlCommand cmd = new SqlCommand(query, sql);
            cmd.Parameters.AddWithValue("@today", SqlDbType.DateTime).Value = date;
            cmd.Parameters.AddWithValue("@tomorrow", SqlDbType.DateTime).Value = tomorrow;
            cmd.Connection.Open();
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    result.Add(new Transaction()
                    {
                        Id = reader[0] as string,
                        Date = reader.GetDateTime(1),
                        Card = reader[2] as string,
                        Location = reader[3] as string,
                        Concept = reader[4] as string,
                        Amount = reader.GetDecimal(5),
                        Employee = reader[6] as string,
                        Account = reader[7] as string,
                        Entity = reader[8] as string,
                        Class = reader[9] as string,
                        JobNumber = reader[10] as string,
                        JobName = reader[11] as string,
                        MainCard = reader[12] as string,
                        Notes = reader[13] as string
                    });
                }
                cmd.Connection.Close();
            }
            if (detailsGrid.DataSource != null) detailsGrid.DataSource = null;
            detailsGrid.DataSource = result;
            GridColumn price = detailsGridView.Columns["Amount"];
            price.DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
            price.DisplayFormat.FormatString = "c2";
            detailsGridView.BestFitColumns();
        }

        private void LoadReport()
        {
            report = true;
            BindingList<Report> result = new BindingList<Report>();
            OpenFileDialog open = new OpenFileDialog
            {
                Filter = "Comma Separated Values(*.csv)|*.csv"
            };
            if (open.ShowDialog() == DialogResult.OK)
            {
                var csv = open.FileName;
                using (TextFieldParser parser = new TextFieldParser(csv, System.Text.Encoding.UTF8))
                {
                    parser.TextFieldType = FieldType.Delimited;
                    parser.SetDelimiters(",");
                    var fields = parser.ReadFields();
                    while (!parser.EndOfData)
                    {
                        var values = parser.ReadFields();

                        result.Add(new Report()
                        {
                            T_date = DateTime.Parse(values[0]),
                            Card = values[1] == "0" ? "0000" : values[1],
                            Location = values[2],
                            Concept = values[3],
                            Amount = string.IsNullOrEmpty(values[5]) ? decimal.Parse(values[4]) : decimal.Parse(values[5])
                        });

                    }
                }
                if (detailsGrid.DataSource != null) detailsGrid.DataSource = null;
                detailsGrid.DataSource = result;
                GridColumn price = detailsGridView.Columns["Amount"];
                price.DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
                price.DisplayFormat.FormatString = "c2";
                detailsGridView.Columns[5].Visible = false;
                detailsGridView.BestFitColumns();
            }
        }

        private void detailsGridView_RowStyle(object sender, RowStyleEventArgs e)
        {
            if (report)
            {
                GridView view = sender as GridView;
                if (e.RowHandle >= 0)
                {
                    var date = DateTime.Parse(view.GetRowCellDisplayText(e.RowHandle, view.Columns[0]));
                    var card = view.GetRowCellDisplayText(e.RowHandle, view.Columns[1]);
                    var amnt = decimal.Parse(view.GetRowCellDisplayText(e.RowHandle, view.Columns[4]).Replace("$", ""));

                    var query = "SELECT * FROM Records WHERE Card = @card AND Amount = @amnt AND TDate >= @date";
                    SqlCommand cmd = new SqlCommand(query, sql);
                    cmd.Parameters.AddWithValue("@date", SqlDbType.DateTime).Value = date;
                    cmd.Parameters.AddWithValue("@card", SqlDbType.VarChar).Value = card;
                    cmd.Parameters.AddWithValue("@amnt", SqlDbType.Decimal).Value = amnt;

                    cmd.Connection.Open();
                    var reader = cmd.ExecuteReader();
                    if (reader.Read())
                    {
                        e.Appearance.BackColor = Color.LightBlue;
                        view.SetRowCellValue(e.RowHandle, "Id", reader[0] as string);
                    }
                    else
                    {
                        e.Appearance.BackColor = Color.Salmon;
                        e.Appearance.BackColor2 = Color.SeaShell;
                    }
                    reader.Close();
                    cmd.Connection.Close();

                }
            }
        }

        private void dateEdit1_EditValueChanged(object sender, EventArgs e)
        {
            try
            {
                var date = (DateTime)dateEdit1.EditValue;
                if (generalGrid.DataSource != null) generalGrid.DataSource = null;
                generalGrid.DataSource = GetCustomDate(date);
                GridColumn price = generalGridView.Columns["Amount"];
                price.DisplayFormat.FormatType = FormatType.Numeric;
                price.DisplayFormat.FormatString = "c2";
                generalGridView.Columns[2].Visible = false;
                generalGridView.Columns[7].Visible = false;
                generalGridView.Columns[8].Visible = false;
                generalGridView.Columns[9].Visible = false;
                generalGridView.Columns[10].Visible = false;
                generalGridView.Columns[11].Visible = false;
                generalGridView.Columns[12].Visible = false;
            } catch (NullReferenceException)
            {
                LoadTable();
            }
        }

        private BindingList<Transaction> GetCustomDate(DateTime _date)
        {
            BindingList<Transaction> result = new BindingList<Transaction>();
            var date = _date.ToString("yyyy-MM-dd");
            var tomorrou = _date.AddDays(1);
            var tomorrow = tomorrou.ToString("yyyy-MM-dd");
            var query = "SELECT * FROM Records WHERE TDate >= @today AND TDate < @tomorrow";
            SqlCommand cmd = new SqlCommand(query, sql);
            cmd.Parameters.AddWithValue("@today", SqlDbType.DateTime).Value = date;
            cmd.Parameters.AddWithValue("@tomorrow", SqlDbType.DateTime).Value = tomorrow;
            cmd.Connection.Open();
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    result.Add(new Transaction()
                    {
                        Id = reader[0] as string,
                        Employee = reader[1] as string,
                        Card = reader[2] as string,
                        Amount = reader.GetDecimal(3),
                        Concept = reader[4] as string,
                        Location = reader[5] as string,
                        Date = reader.GetDateTime(6),
                        Notes = reader[7] as string
                    });
                }
                cmd.Connection.Close();
            }
            return result;
        }

        private void addUserToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var instance = Controller.controller.addUser;
            if (instance != null) instance.Dispose();
            instance = Controller.controller.addUser = new AddUser();
            instance.Show();
        }

        private void fileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (System.Deployment.Application.ApplicationDeployment.IsNetworkDeployed)
            {
                MessageBox.Show("Intelogix México © 2020\n\nCurrent Version: " + System.Deployment.Application.ApplicationDeployment.CurrentDeployment.CurrentVersion);
            }
            else
            {
                MessageBox.Show("Not currently deployed.");
            }
        }

        private void employeesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var instance = Controller.controller.employees;
            if (instance != null) instance.Dispose();
            instance = Controller.controller.employees = new Employees();
            instance.Show();
        }
    }
}
