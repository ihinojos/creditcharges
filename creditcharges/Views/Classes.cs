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
    public partial class Classes : Form
    {
        private readonly SqlConnection sql;
        public Classes()
        {
            sql = new SqlConnection(Data.cn);
            InitializeComponent();
            FillComboBox();
        }

        private void FillComboBox()
        {
            Data.getData();
            classBox.Items.Clear();
            entityBox.Items.Clear();
            classBox.Items.AddRange(Data.classes.ToArray());
            entityBox.Items.AddRange(Data.entities.ToArray());
        }

        private void classBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            var name = classBox.SelectedItem;
            var query = "SELECT Entity FROM Classes WHERE Class = @class";
            var cmd = new SqlCommand(query, sql);
            cmd.Parameters.AddWithValue("@class", SqlDbType.VarChar).Value = name;
            cmd.Connection.Open();
            using (var reader = cmd.ExecuteReader()) if (reader.Read()) entityBox.Text = reader[0] as string;
            cmd.Connection.Close();
        }

        private void windowsUIButtonPanel1_ButtonClick(object sender, DevExpress.XtraBars.Docking2010.ButtonEventArgs e)
        {
            var tag = ((WindowsUIButton)e.Button).Tag.ToString();
            switch (tag)
            {
                case "save":
                    SaveClass();
                    break;
                case "remove":
                    DeleteClass();
                    break;
                case "cancel": Dispose(); break;
            }
        }

        private void SaveClass()
        {
            try
            {
                if (Data.classes.Contains(classBox.Text))
                {
                    var query = "UPDATE Classes SET Entity = @entity WHERE Class = @class";
                    var cmd = new SqlCommand(query, sql);
                    var clas = classBox.Text;
                    var entity = entityBox.SelectedItem.ToString();
                    cmd.Parameters.AddWithValue("@class", SqlDbType.VarChar).Value = clas;
                    cmd.Parameters.AddWithValue("@entity", SqlDbType.VarChar).Value = entity;
                    cmd.Connection.Open();
                    var res = cmd.ExecuteNonQuery();
                    if (res == 1) MessageBox.Show("Saved.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    cmd.Connection.Close();
                    FillComboBox();
                }
                else
                {
                    var query = "INSERT INTO Classes VALUES (@class, @entity)";
                    var cmd = new SqlCommand(query, sql);
                    var clas = classBox.Text;
                    var entity = entityBox.SelectedItem.ToString();
                    cmd.Parameters.AddWithValue("@class", SqlDbType.VarChar).Value = clas;
                    cmd.Parameters.AddWithValue("@entity", SqlDbType.VarChar).Value = entity;
                    cmd.Connection.Open();
                    var res = cmd.ExecuteNonQuery();
                    if (res == 1) MessageBox.Show("Saved.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    cmd.Connection.Close();
                    FillComboBox();
                }
            }
            catch (NullReferenceException)
            {
                MessageBox.Show("Information missing, please verify.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DeleteClass()
        {

            if (Data.names.Contains(classBox.Text))
            {
                var query = "DELETE FROM Classes WHERE Class = @class";
                var cmd = new SqlCommand(query, sql);
                var name = classBox.Text;
                cmd.Parameters.AddWithValue("@class", SqlDbType.VarChar).Value = name;
                cmd.Connection.Open();
                var res = cmd.ExecuteNonQuery();
                cmd.Connection.Close();
                if (res == 1) MessageBox.Show("Class deleted.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                FillComboBox();
            }
            else MessageBox.Show("Class doesn't exists.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
