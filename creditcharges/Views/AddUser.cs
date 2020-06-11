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
    public partial class AddUser : Form
    {

        private readonly SqlConnection sql;
        public AddUser()
        {
            InitializeComponent();
            sql = new SqlConnection(Data.cn);
        }

        private void windowsUIButtonPanel1_ButtonClick(object sender, DevExpress.XtraBars.Docking2010.ButtonEventArgs e)
        {
            var tag = ((WindowsUIButton)e.Button).Tag.ToString();
            switch (tag)
            {
                case "save":
                    CreateUser();
                    break;
                case "cancel":
                    Dispose();
                    break;
            }
                
        }

        private void CreateUser()
        {
            var username = usernameBox.Text;
            var name = nameBox.Text;
            var p1 = p1Box.Text;
            var p2 = p2Box.Text;
            var adm = admBox.Checked;

            if (!(string.IsNullOrEmpty(username) || string.IsNullOrEmpty(name) || string.IsNullOrEmpty(p1) || string.IsNullOrEmpty(p2)))
            {
                if (p1 == p2)
                {
                    if (Password.IsSecure(p1))
                    {
                        var hash = Password.Hash(p1);
                        var query = "INSERT INTO  Users (UserId, Name, Password, Admin) VALUES (@username, @name, @password, @admin)";
                        SqlCommand cmd = new SqlCommand(query, sql);
                        cmd.Parameters.AddWithValue("@username", SqlDbType.VarChar).Value = username;
                        cmd.Parameters.AddWithValue("@name", SqlDbType.VarChar).Value = name;
                        cmd.Parameters.AddWithValue("@password", SqlDbType.VarChar).Value = hash;
                        cmd.Parameters.AddWithValue("@admin", SqlDbType.Bit).Value = adm;
                        cmd.Connection.Open();
                        var res = cmd.ExecuteNonQuery();
                        if (res == 1) MessageBox.Show("User created successfully.", "Success");
                        else MessageBox.Show("Please check your internet connection.", "Error");
                    }
                } else MessageBox.Show("Passwords do not match.", "Error");
            }
            else MessageBox.Show("There are empty fields.", "Error");

            usernameBox.Text = string.Empty;
            nameBox.Text = string.Empty;
            p1Box.Text = string.Empty;
            p2Box.Text = string.Empty;
            admBox.Checked = false;

        }
    }
}
