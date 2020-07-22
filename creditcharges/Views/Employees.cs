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
        #region Attributes 
        private string lastNameSelected = "";
        private readonly SqlConnection sql;
        #endregion

        #region Constructor
        public Employees()
        {
            InitializeComponent();
            sql = new SqlConnection(Data.cn);
            LoadData();
        }
        #endregion

        #region Events 
        private void windowsUIButtonPanel1_ButtonClick(object sender, ButtonEventArgs e)
        {
            var tag = ((WindowsUIButton)e.Button).Tag.ToString();
            switch (tag)
            {
                case "save":
                    SaveEmployee();
                    break;
                case "remove":
                    RemoveEmployee();
                    break;
                case "cancel":
                    Dispose();
                    break;
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            lastNameSelected = comboBox1.SelectedItem.ToString();
            var query = "SELECT Entity from Employees WHERE Name = @name";
            var cmd = new SqlCommand(query, sql);
            cmd.Parameters.AddWithValue("@name", SqlDbType.VarChar).Value = lastNameSelected;
            if (cmd.Connection.State != ConnectionState.Open) cmd.Connection.Open();
            using (var reader = cmd.ExecuteReader())
            {
                if (reader.Read())
                {
                    comboBox2.Text = reader[0] as string;
                }
            }
            cmd.Connection.Close();
        }

        #endregion

        #region Methods
        private void LoadData()
        {
            Data.getData();

            var names = Data.names.ToArray();
            comboBox1.Items.Clear();
            comboBox1.Items.AddRange(names);
            AutoCompleteStringCollection collection = new AutoCompleteStringCollection();
            collection.AddRange(names);
            comboBox1.AutoCompleteCustomSource = collection;

            comboBox2.Items.Clear();
            comboBox2.Items.AddRange(Data.entities.ToArray());

            if (Controller.controller.editTransaction != null) Controller.controller.editTransaction.AddAutoCompleteOptions();
        }


        private void SaveEmployee()
        {
            if (!Data.names.Contains(comboBox1.Text))
            {
                var query = "INSERT INTO Employees (Id, Name, Entity) VALUES (@id, @name, @entity)";
                var cmd = new SqlCommand(query, sql);
                var id = Guid.NewGuid().ToString("N");
                var name = comboBox1.Text.Trim();
                var entity = comboBox2.SelectedItem.ToString();
                cmd.Parameters.AddWithValue("@id", SqlDbType.VarChar).Value = id;
                cmd.Parameters.AddWithValue("@name", SqlDbType.VarChar).Value = name;
                cmd.Parameters.AddWithValue("@entity", SqlDbType.VarChar).Value = entity;
                if (cmd.Connection.State != ConnectionState.Open) cmd.Connection.Open();
                var res = cmd.ExecuteNonQuery();
                cmd.Connection.Close();
                if (res == 1) MessageBox.Show("Employee added.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadData();
            }
            else
            {
                var query = "UPDATE Employees SET Name = @name, Entity = @entity WHERE Name = @last";
                var cmd = new SqlCommand(query, sql);
                cmd.Parameters.AddWithValue("@name", SqlDbType.VarChar).Value = comboBox1.Text.Trim();
                cmd.Parameters.AddWithValue("@entity", SqlDbType.VarChar).Value = comboBox2.SelectedItem;
                cmd.Parameters.AddWithValue("@last", SqlDbType.VarChar).Value = lastNameSelected;
                if (cmd.Connection.State != ConnectionState.Open) cmd.Connection.Open();
                var res = cmd.ExecuteNonQuery();
                cmd.Connection.Close();

                if(res == 1)
                {
                    MessageBox.Show("Employee info updated. ", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                } else
                {
                    MessageBox.Show("There's been an error, please try again. ", "Error", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                LoadData();
            }
        }

        private void RemoveEmployee()
        {
            if (Data.names.Contains(comboBox1.Text))
            {
                var query = "DELETE FROM Employees WHERE Name = @name";
                var cmd = new SqlCommand(query, sql);
                var name = comboBox1.Text;
                cmd.Parameters.AddWithValue("@name", SqlDbType.VarChar).Value = name;
                if (cmd.Connection.State != ConnectionState.Open) cmd.Connection.Open();
                var res = cmd.ExecuteNonQuery();
                cmd.Connection.Close();
                if (res == 1) MessageBox.Show("Employee deleted.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadData();
            }
            else MessageBox.Show("Employee doesn't exists.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

        }
        #endregion
    }
}
