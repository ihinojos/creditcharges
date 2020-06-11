using creditcharges.Models;
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
    public partial class LogIn : Form
    {

        private SqlConnection sql;

        public LogIn()
        {
            InitializeComponent();
            sql = new SqlConnection(Data.cn);
        }

        private void Get_Account(object sender, EventArgs e)
        {
            var user = userBox.Text;
            var pass = passBox.Text;
            if (!string.IsNullOrEmpty(user) && !string.IsNullOrEmpty(pass))
            {
                var query = "SELECT * FROM Users WHERE UserId = @userId";
                SqlCommand cmd = new SqlCommand(query, sql);
                cmd.Parameters.AddWithValue("@userId", SqlDbType.VarChar).Value = user;
                cmd.Connection.Open();
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        var hash = reader.GetString(2);
                        var admn = reader.GetBoolean(3);
                        switch (Password.Verify(pass, hash))
                        {
                            case true:
                                var instance = Controller.controller.mainForm = new MainForm(admn);
                                instance.Show();
                                Hide();
                                break;
                            case false:
                                MessageBox.Show("The password is incorrect.");
                                break;
                        }
                    }
                    else
                    {
                        MessageBox.Show("The username does not exist.");
                    }
                    passBox.Text = "";
                    cmd.Connection.Close();
                }
            }
            else MessageBox.Show("There are empty fields!");
        }
    }
}
