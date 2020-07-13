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
    public partial class Entities : Form
    {

        #region Attributes
        private int selectedIndex;
        private readonly SqlConnection sql;
        #endregion

        #region Constructor
        public Entities()
        {
            sql = new SqlConnection(Data.cn);
            InitializeComponent();
            FillComboBox();
        }
        #endregion

        #region Events

        private void windowsUIButtonPanel1_ButtonClick(object sender, DevExpress
            .XtraBars.Docking2010.ButtonEventArgs e)
        {
            try
            {
                var tag = ((WindowsUIButton)e.Button).Tag.ToString();
                switch (tag)
                {
                    case "add":
                        AddEntity();
                        break;
                    case "remove":
                        DeleteEntity();
                        break;
                    case "cancel":
                        Dispose();
                        break;
                }
            }
            catch
            {
            }
        }

        private void entityBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            selectedIndex = entityBox.SelectedIndex;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var instance = Controller.controller.classes;
            if (instance != null) instance.Dispose();
            instance = Controller.controller.classes = new Classes();
            instance.Show();
        }
        #endregion

        #region Methods
        private void FillComboBox()
        {
            entityBox.Items.Clear();
            entityBox.Items.AddRange(Data.entities.ToArray());
        }


        private void AddEntity()
        {
            var entity = entityBox.Text.Trim();
            if (entityBox.Items.Contains(entity))
            {
                MessageBox.Show("The entity already exists. ", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                var query = "INSERT INTO Entities VALUES (NEWID(), @entity)";
                var cmd = new SqlCommand(query, sql);
                cmd.Parameters.AddWithValue("@entity", SqlDbType.VarChar).Value = entity;
                if (cmd.Connection.State != ConnectionState.Open)
                    cmd.Connection.Open();
                var res = cmd.ExecuteNonQuery();

                if (res == 1)
                {
                    MessageBox.Show("The entity has been added. ", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("There's been an error, plase try again.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                cmd.Connection.Close();
                FillComboBox();
            }
        }

        private void DeleteEntity()
        {
            var entity = entityBox.Text.Trim();
            if (entityBox.Items.Contains(entity))
            {
                var query = "DELETE FROM Entities WHERE Name = @entity";
                var cmd = new SqlCommand(query, sql);
                cmd.Parameters.AddWithValue("@entity", SqlDbType.VarChar).Value = entity;
                cmd.Connection.Open();
                var res = cmd.ExecuteNonQuery();
                if (res == 1)
                {
                    MessageBox.Show("The entity has been deleted. ", "Error", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("There's been an error, plase try again.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                cmd.Connection.Close();
                FillComboBox();
            }
            else
            {
                MessageBox.Show("The entity does not exist. ", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        #endregion
    }
}
