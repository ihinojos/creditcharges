using creditcharges.Models;
using DevExpress.XtraBars;
using DevExpress.XtraBars.Docking2010;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace creditcharges.Views
{
    public partial class AddCard : Form
    {
        private readonly SqlConnection sql;
        private string[] banks = { "American Express", "Barclay Bank" , "Capital One", "Chase Bank" };

        public AddCard()
        {
            InitializeComponent();
            sql = new SqlConnection(Data.cn);
            entityBox.Items.AddRange(Data.entities.ToArray());
            GetCards();
        }

        private void windowsUIButtonPanel1_ButtonClick(object sender, DevExpress.XtraBars.Docking2010.ButtonEventArgs e)
        {
            var tag = ((WindowsUIButton)e.Button).Tag.ToString();
            Console.WriteLine(tag);
            switch (tag)
            {
                case "save":
                    SaveCard();
                    break;

                case "cancel":
                    Dispose();
                    break;

                case "delete":
                    Delete();
                    break;
            }
        }

        private void UpdateCard()
        {
            try
            {
                var num = numberBox.Text;
                var bank = bankBox.Text;
                var uname = unameBox.Text;
                var entity = entityBox.Text;

                if (!(string.IsNullOrEmpty(num) || string.IsNullOrEmpty(bank)
                    || string.IsNullOrEmpty(uname) || string.IsNullOrEmpty(entity)))
                {
                    var query = "UPDATE MainCards SET Bank = @bank, Uname = @uname, Entity = @entity " +
                        "WHERE Number = @num";
                    var cmd = new SqlCommand(query, sql);
                    cmd.Parameters.AddWithValue("@num", SqlDbType.VarChar).Value = num;
                    cmd.Parameters.AddWithValue("@bank", SqlDbType.VarChar).Value = bank;
                    cmd.Parameters.AddWithValue("@uname", SqlDbType.VarChar).Value = uname;
                    cmd.Parameters.AddWithValue("@entity", SqlDbType.VarChar).Value = entity;

                    cmd.Connection.Open();
                    var res = cmd.ExecuteNonQuery();
                    cmd.Connection.Close();

                    if (res == 1)
                    {
                        MessageBox.Show("Card saved successfully", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                else MessageBox.Show("Invalid data, please check fields.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SaveCard()
        {
            if (cardBox.Text == "New card")
                try
                {
                    var num = numberBox.Text;
                    var bank = bankBox.Text;
                    var uname = unameBox.Text;
                    var entity = entityBox.Text;

                    if (!(string.IsNullOrEmpty(num) || string.IsNullOrEmpty(bank)
                        || string.IsNullOrEmpty(uname) || string.IsNullOrEmpty(entity)))
                    {
                        var query = "INSERT INTO MainCards(Number, Bank, Uname, Entity) VALUES (@num, @bank, @uname, @entity)";
                        var cmd = new SqlCommand(query, sql);
                        cmd.Parameters.AddWithValue("@num", SqlDbType.VarChar).Value = num;
                        cmd.Parameters.AddWithValue("@bank", SqlDbType.VarChar).Value = bank;
                        cmd.Parameters.AddWithValue("@uname", SqlDbType.VarChar).Value = uname;
                        cmd.Parameters.AddWithValue("@entity", SqlDbType.VarChar).Value = entity;

                        cmd.Connection.Open();
                        var res = cmd.ExecuteNonQuery();
                        cmd.Connection.Close();

                        if (res == 1)
                        {
                            MessageBox.Show("Card saved successfully", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        GetCards();
                    }
                    else MessageBox.Show("Invalid data, please check fields.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            else UpdateCard();
        }

        private void Delete()
        {
            var main = cardBox.Text;
            string message = "Do you want to delete this card and all childs associated?";
            MessageBoxButtons buttons = MessageBoxButtons.YesNo;
            DialogResult result = MessageBox.Show(message, "", buttons, MessageBoxIcon.Warning);
            if (result == DialogResult.Yes)
            {
                int res = 0;
                var query = "DELETE FROM ChildCards WHERE Main = @main";
                var cmd = new SqlCommand(query, sql);
                cmd.Parameters.AddWithValue("@main", SqlDbType.VarChar).Value = main;
                cmd.Connection.Open();
                res += cmd.ExecuteNonQuery();

                query = "DELETE FROM MainCards WHERE Number = @main";
                cmd.CommandText = query;
                res += cmd.ExecuteNonQuery();
                cmd.Connection.Close();

                if (res > 0) MessageBox.Show("Card deleted.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void GetCards()
        {
            cardBox.Items.Clear();
            bankBox.Items.Clear();
            cardBox.Items.Add("New card");
            bankBox.Items.AddRange(banks);
            cardBox.SelectedIndex = 0;
            var query = "SELECT * FROM MainCards";
            var cmd = new SqlCommand(query, sql);
            cmd.Connection.Open();
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    cardBox.Items.Add(reader[0] as string);
                }
                cmd.Connection.Close();
            }
        }

        private void cardBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            var card = cardBox.SelectedItem.ToString();
            if (card == "New card")
            {
                numberBox.Enabled = true;
                numberBox.Text = string.Empty;
                entityBox.SelectedItem = null;
                unameBox.Text = string.Empty;
                bankBox.Text = string.Empty;
            }
            else
            {
                numberBox.Enabled = false;
                var query = "SELECT * FROM MainCards WHERE Number = @number";
                var cmd = new SqlCommand(query, sql);
                cmd.Parameters.AddWithValue("@number", SqlDbType.VarChar).Value = card;
                cmd.Connection.Open();
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        numberBox.Text = reader[0] as string;
                        bankBox.Text = reader[1] as string;
                        unameBox.Text = reader[2] as string;
                        entityBox.Text = reader[3] as string;
                    }
                    cmd.Connection.Close();
                }
            }
        }

        private void view_cards_Btn_Click(object sender, EventArgs e)
        {
            try
            {
                var card = cardBox.SelectedItem.ToString();
                var instance = Controller.controller.childCards;
                if (instance != null) instance.Dispose();
                instance = Controller.controller.childCards = new ChildCards(card);
                instance.Show();
            }
            catch { }
        }
    }
}