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
        #region Attributes
        private readonly SqlConnection sql;
        #endregion

        #region Constructor
        public AddUser()
        {
            InitializeComponent();
            sql = new SqlConnection(Data.cn);
        }
        #endregion

        #region Events
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
        #endregion

        #region Methods
        private void CreateUser()
        {
            var username = usernameBox.Text.Trim();
            var name = nameBox.Text.Trim();
            var p1 = p1Box.Text.Trim();
            var p2 = p2Box.Text.Trim();
            var adm = admBox.Checked;

            if(name.Trim().Split(new char[] { ' '}).Length < 2)
            {
                MessageBox.Show("El nombre debe incluir mínimo 2 partes.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

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
                        if (cmd.Connection.State != ConnectionState.Open) cmd.Connection.Open();
                        var res = cmd.ExecuteNonQuery();
                        if (res == 1) MessageBox.Show("Usuario creado exitosamente.", "Hecho", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        else MessageBox.Show("Ocurrió un error, por favor intente de nuevo.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                } else MessageBox.Show("La contraseña no coincide.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else MessageBox.Show("Campos vacíos, por favor verifique.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

            usernameBox.Text = string.Empty;
            nameBox.Text = string.Empty;
            p1Box.Text = string.Empty;
            p2Box.Text = string.Empty;
            admBox.Checked = false;

        }
        #endregion
    }
}
