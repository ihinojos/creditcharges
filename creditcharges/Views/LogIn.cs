using creditcharges.Models;
using DevExpress.XtraBars;
using DevExpress.XtraBars.Ribbon.Accessible;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace creditcharges.Views
{
    public partial class LogIn : Form
    {
        #region Attributes
        private SqlConnection sql;
        #endregion

        #region Constructor
        public LogIn()
        {
            InitializeComponent();
            sql = new SqlConnection(Data.cn);
            KeyDown += new KeyEventHandler(passBox_KeyDown);
        }
        #endregion

        #region Events

        private void passBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter) LogInAccount();
        }
        #endregion

        #region Methods
        private void LogInAccount()
        {
            var user = userBox.Text;
            var pass = passBox.Text;
            if (!string.IsNullOrEmpty(user) && !string.IsNullOrEmpty(pass))
            {
                var query = "SELECT * FROM Users WHERE UserId = @userId";
                SqlCommand cmd = new SqlCommand(query, sql);
                cmd.Parameters.AddWithValue("@userId", SqlDbType.VarChar).Value = user;
                if (cmd.Connection.State != ConnectionState.Open) cmd.Connection.Open();
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        var hash = reader.GetString(2);
                        var admn = reader.GetBoolean(3);
                        switch (Password.Verify(pass, hash))
                        {
                            case true:
                                var instance = Controller.controller.mainForm = new MainForm(admn, reader[1] as string);
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

        private void Get_Account(object sender, EventArgs e)
        {
            LogInAccount();
        }
        #endregion
    }
}