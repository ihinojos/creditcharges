using creditcharges.Models;
using DevExpress.XtraBars.Docking2010;
using DevExpress.XtraLayout.Utils;
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
    public partial class ChildCards : Form
    {
        private readonly string Card;
        private readonly SqlConnection sql;
        public ChildCards(string card)
        {
            Card = card;
            sql = new SqlConnection(Data.cn);
            InitializeComponent();
            LoadCards();
        }

        private void LoadCards()
        {
            var query = "SELECT Card FROM ChildCards WHERE Main = @card";
            var cmd = new SqlCommand(query, sql);
            cmd.Parameters.AddWithValue("@card", SqlDbType.VarChar).Value = Card;
            cmd.Connection.Open();
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    childsBox.Items.Add(reader[0] as string);
                }
                label1.Text = "Child Cards[" + childsBox.Items.Count + "]:";
                cmd.Connection.Close();
            }
        }

        private void windowsUIButtonPanel1_ButtonClick(object sender, DevExpress.XtraBars.Docking2010.ButtonEventArgs e)
        {
            var tag = ((WindowsUIButton)e.Button).Tag.ToString();
            switch (tag)
            {
                case "add":
                    AddChildCard();
                    break;
                case "remove":
                    RemoveChildCard();
                    break;
            }
        }

        private void AddChildCard()
        {
            var card = childsBox.Text;
            if (!childsBox.Items.Contains(card))
            {
                var query = "INSERT INTO ChildCards (Card, Main) VALUES (@child, @main)";
                var cmd = new SqlCommand(query, sql);
                cmd.Parameters.AddWithValue("@child", SqlDbType.VarChar).Value = card;
                cmd.Parameters.AddWithValue("@main", SqlDbType.VarChar).Value = Card;
                cmd.Connection.Open();
                var res = cmd.ExecuteNonQuery();
                cmd.Connection.Close();
                if (res == 1) MessageBox.Show("Card added.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else MessageBox.Show("Card is already saved.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void RemoveChildCard()
        {
            var card = childsBox.Text;
            if (childsBox.Items.Contains(card))
            {
                var query = "DELETE FROM ChildCards WHERE Card = @child";
                var cmd = new SqlCommand(query, sql);
                cmd.Parameters.AddWithValue("@child", SqlDbType.VarChar).Value = card;
                cmd.Connection.Open();
                var res = cmd.ExecuteNonQuery();
                cmd.Connection.Close();
                if (res == 1) MessageBox.Show("Card deleted.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else MessageBox.Show("Card doesn't exist.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
