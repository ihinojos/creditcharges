﻿using creditcharges.Models;
using DevExpress.Xpo.DB.Helpers;
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
    public partial class Vehicles : Form
    {
        private readonly string[] types = { "Motorcycle", "Sedan", "SUV", "Pickup", "Truck", "Trailer" };
        private readonly SqlConnection sql;
        public Vehicles()
        {
            InitializeComponent();
            sql = new SqlConnection(Data.cn);
            FillTextBoxes();
        }

        private void FillTextBoxes()
        {
            Data.getData();

            typeBox.Items.Clear();
            typeBox.Items.AddRange(types);

            AutoCompleteStringCollection plates = new AutoCompleteStringCollection();
            plates.AddRange(Data.plates.ToArray());
            plateBox.AutoCompleteCustomSource = plates;

            AutoCompleteStringCollection vNames = new AutoCompleteStringCollection();
            vNames.AddRange(Data.vNames.ToArray());
            vNameBox.AutoCompleteCustomSource = vNames;
        }

        private void SaveVehicle()
        {
            var plate = plateBox.Text;
            var vName = vNameBox.Text;
            var model = modelBox.Text;
            var vType = typeBox.SelectedItem.ToString();
            var id = Guid.NewGuid().ToString("N");
            string query = "";

            if (string.IsNullOrEmpty(plate) || string.IsNullOrEmpty(vName)) return;

            if (!Data.plates.Contains(plate))
            {
                query = "INSERT INTO Vehicles VALUES (@id, @plate, @name, @model, @type, NULL)";
            }
            else
            {
                query = "UPDATE Vehicles SET VName = @name, Model = @model, VType = @type WHERE Plate = @plate";
            }

            var cmd = new SqlCommand(query, sql);
            cmd.Parameters.AddWithValue("@id", SqlDbType.VarChar).Value = id;
            cmd.Parameters.AddWithValue("@plate", SqlDbType.VarChar).Value = plate;
            cmd.Parameters.AddWithValue("@name", SqlDbType.VarChar).Value = vName;
            cmd.Parameters.AddWithValue("@model", SqlDbType.VarChar).Value = model;
            cmd.Parameters.AddWithValue("@type", SqlDbType.VarChar).Value = vType;
            cmd.Connection.Open();
            var res = cmd.ExecuteNonQuery();
            cmd.Connection.Close();
            if (res == 1) MessageBox.Show("Vehicle saved. ", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void DeleteVehicle()
        {
            var plate = plateBox.Text;
            if (string.IsNullOrEmpty(plate)) return;
            if (Data.plates.Contains(plate))
            {
                var query = "DELETE FROM Vehicles WHERE Plate = @plate";
                var cmd = new SqlCommand(query, sql);
                cmd.Parameters.AddWithValue("@plate", SqlDbType.VarChar).Value = plate;
                cmd.Connection.Open();
                var res = cmd.ExecuteNonQuery();
                cmd.Connection.Close();
                if (res == 1) MessageBox.Show("Vehicle deleted.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else return;
        }

        private void windowsUIButtonPanel1_ButtonClick(object sender, DevExpress.XtraBars.Docking2010.ButtonEventArgs e)
        {
            var tag = ((WindowsUIButton)e.Button).Tag.ToString();
            switch (tag)
            {
                case "save":
                    SaveVehicle();
                    break;
                case "delete":
                    DeleteVehicle();
                    break;
                case "cancel":
                    Dispose();
                    break;
            }
            FillTextBoxes();
        }

        private void plateBox_Leave(object sender, EventArgs e)
        {
            var plate = plateBox.Text;
            if (Data.plates.Contains(plate))
            {
                var query = "SELECT * FROM Vehicles WHERE Plate = @plate";
                var cmd = new SqlCommand(query, sql);
                cmd.Parameters.AddWithValue("@plate", SqlDbType.VarChar).Value = plate;
                cmd.Connection.Open();
                using(var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        plateBox.Text = reader[1] as string;
                        vNameBox.Text = reader[2] as string;
                        modelBox.Text = reader[3] as string;
                        typeBox.Text = reader[4] as string;
                    }
                }
                cmd.Connection.Close();
            }
            else
            {
                return;
            }
        }

        private void vNameBox_Leave(object sender, EventArgs e)
        {
            var name = vNameBox.Text;
            if (Data.vNames.Contains(name))
            {
                var query = "SELECT * FROM Vehicles WHERE VName = @name";
                var cmd = new SqlCommand(query, sql);
                cmd.Parameters.AddWithValue("@name", SqlDbType.VarChar).Value = name;
                cmd.Connection.Open();
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        plateBox.Text = reader[1] as string;
                        vNameBox.Text = reader[2] as string;
                        modelBox.Text = reader[3] as string;
                        typeBox.Text = reader[4] as string;
                    }
                }
                cmd.Connection.Close();
            }
            else
            {
                return;
            }
        }
    }
}