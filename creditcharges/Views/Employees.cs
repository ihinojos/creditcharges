using creditcharges.Models;
using DevExpress.XtraBars.Docking2010;
using DevExpress.XtraRichEdit.Layout;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace creditcharges.Views
{
    public partial class Employees : Form
    {

        private readonly SqlConnection sql;

        public Employees()
        {
            InitializeComponent();
            sql = new SqlConnection(Data.cn);
            LoadData();
        }

        private void LoadData()
        {
            var names = Data.names.ToArray();
            comboBox1.Items.Clear();
            comboBox1.Items.AddRange(names);
            AutoCompleteStringCollection collection = new AutoCompleteStringCollection();
            collection.AddRange(names);
            comboBox1.AutoCompleteCustomSource = collection;
        }

        private void windowsUIButtonPanel1_ButtonClick(object sender, ButtonEventArgs e)
        {
            var tag = ((WindowsUIButton)e.Button).Tag.ToString();
            switch (tag)
            {
                case "add":
                    AddEmployee();
                    break;
                case "remove":
                    RemoveEmployee();
                    break;
                case "cancel":
                    Dispose();
                    break;
            }
        }

        private void AddEmployee()
        {
            if (!Data.names.Contains(comboBox1.Text))
            {
                var query = "INSERT INTO Employees (Id, Name) VALUES (@id, @name)";
                var cmd = new SqlCommand(query, sql);
                var id = Guid.NewGuid().ToString("N");
                var name = comboBox1.Text;
                cmd.Parameters.AddWithValue("@id", SqlDbType.VarChar).Value = id;
                cmd.Parameters.AddWithValue("@name", SqlDbType.VarChar).Value = name;
                cmd.Connection.Open();
                var res = cmd.ExecuteNonQuery();
                cmd.Connection.Close();
                if (res == 1) MessageBox.Show("Employee added.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                Data.getData();
                LoadData();
            }
            else MessageBox.Show("Employee already exists.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void RemoveEmployee()
        {
            if (Data.names.Contains(comboBox1.Text))
            {
                var query = "DELETE FROM Employees WHERE Name = @name";
                var cmd = new SqlCommand(query, sql);
                var name = comboBox1.Text;
                cmd.Parameters.AddWithValue("@name", SqlDbType.VarChar).Value = name;
                cmd.Connection.Open();
                var res = cmd.ExecuteNonQuery();
                cmd.Connection.Close();
                if (res == 1) MessageBox.Show("Employee deleted.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                Data.getData();
                LoadData();
            }
            else MessageBox.Show("Employee doesn't exists.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

        }
    }
}
