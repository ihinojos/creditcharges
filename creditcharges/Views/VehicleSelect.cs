using creditcharges.Models;
using DevExpress.Data.Extensions;
using DevExpress.Xpo;
using DevExpress.XtraBars.Docking2010;
using DevExpress.XtraRichEdit.Fields.Expression;
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
    public partial class VehicleSelect : Form
    {
        #region Attributes
        private readonly SqlConnection sql;
        #endregion

        #region Constructor
        public VehicleSelect()
        {
            InitializeComponent();
            sql = new SqlConnection(Data.cn);
            LoadData();
        }
        #endregion

        #region Events

        private void plateBox_Leave(object sender, EventArgs e)
        {
            try
            {
                var plate = plateBox.Text;
                var query = "SELECT VName FROM Vehicles WHERE Plate = @plate";
                var cmd = new SqlCommand(query, sql);
                cmd.Parameters.AddWithValue("@plate", SqlDbType.VarChar).Value = plate;
                if (cmd.Connection.State != ConnectionState.Open) cmd.Connection.Open();
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {

                        vNameBox.Text = reader[0] as string;
                    }
                }
                cmd.Connection.Close();
            }
            catch { }
        }

        private void vNameBox_Leave(object sender, EventArgs e)
        {
            try
            {
                var name = vNameBox.Text;
                var idx = Data.vNames.BinarySearch(name);
                plateBox.Text = Data.plates.ElementAt(idx);
            }
            catch { }
        }



        private void windowsUIButtonPanel1_ButtonClick(object sender, DevExpress.XtraBars.Docking2010.ButtonEventArgs e)
        {
            var tag = ((WindowsUIButton)e.Button).Tag.ToString();
            switch (tag)
            {
                case "ok":
                    OK();
                    break;
                case "cancel":
                    break;
            }
        }
        #endregion

        #region Methods
        public void LoadData()
        {
            AutoCompleteStringCollection plates = new AutoCompleteStringCollection();
            plates.AddRange(Data.plates.ToArray());
            plateBox.AutoCompleteCustomSource = plates;

            AutoCompleteStringCollection names = new AutoCompleteStringCollection();
            names.AddRange(Data.vNames.ToArray());
            vNameBox.AutoCompleteCustomSource = names;
        }


        private void OK()
        {
            var plate = plateBox.Text.Trim();
            var name = vNameBox.Text.Trim();
            if (Data.plates.Contains(plate) && Data.vNames.Contains(name))
            {
                var query = "SELECT * FROM Vehicles WHERE Plate = @plate AND VName = @name";
                var cmd = new SqlCommand(query, sql);
                cmd.Parameters.AddWithValue("@plate", SqlDbType.VarChar).Value = plate;
                cmd.Parameters.AddWithValue("@name", SqlDbType.VarChar).Value = name;
                if (cmd.Connection.State != ConnectionState.Open) cmd.Connection.Open();
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        Vehicle v = new Vehicle
                        {
                            Id = reader[0] as string,
                            Plate = reader[1] as string,
                            VName = reader[2] as string,
                            Model = reader[3] as string,
                            VType = reader[4] as string,
                            AvgMPG = reader[5] as decimal? ?? default
                        };
                        Controller.controller.editTransaction.LoadVehicleDetails(v);
                        
                        reader.Close();
                    }
                    reader.Close();
                }
                cmd.Connection.Close();
            }
            else return;
            Dispose();
        }
        #endregion
    }
}
