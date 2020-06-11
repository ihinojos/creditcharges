using creditcharges.Models;
using DevExpress.Utils.Serializing;
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
    public partial class AddCard : Form
    {
        private readonly SqlConnection sql;
        public AddCard()
        {
            InitializeComponent();
            sql = new SqlConnection(Data.cn);

            AutoCompleteStringCollection childs = new AutoCompleteStringCollection();
            childs.AddRange(Data.childCards.ToArray());
            childCard.AutoCompleteCustomSource = childs;

            AutoCompleteStringCollection mains = new AutoCompleteStringCollection();
            mains.AddRange(Data.mainCards.ToArray());
            mainCard.AutoCompleteCustomSource = mains;
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

            }
        }

        private void SaveCard()
        {
            try
            {
                var Main = mainCard.Text;
                var Child = childCard.Text;
                if (!(string.IsNullOrEmpty(Main) || string.IsNullOrEmpty(Child)))
                {
                    var query = "SELECT * FROM MainCards WHERE Card = @main";
                    SqlCommand cmd = new SqlCommand(query, sql);
                    cmd.Parameters.AddWithValue("@main", SqlDbType.VarChar).Value = Main;
                    cmd.Connection.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            query = "SELECT * FROM ChildCards WHERE Card = @child AND ";
                            cmd.CommandText = query;
                            cmd.Parameters.AddWithValue("@child", SqlDbType.VarChar).Value = Child;
                            reader.Close();
                            using (var read = cmd.ExecuteReader())
                            {
                                if (read.Read())
                                {
                                    MessageBox.Show("Card already exists.");
                                }
                                else
                                {
                                    read.Close();
                                    query = "INSERT INTO ChildCards (Card, Main) VALUES (@child, @main)";
                                    cmd.CommandText = query;
                                    int res = cmd.ExecuteNonQuery();
                                    if (res == 1) MessageBox.Show("Card added successfully.");
                                    else MessageBox.Show("There was an error, please check internet connection");
                                }
                            }
                        }
                        else
                        {
                            reader.Close();
                            query = "INSERT INTO MainCards (Card) VALUES (@main)";
                            cmd.CommandText = query;
                            int res = cmd.ExecuteNonQuery();
                            if (res == 1)
                            {
                                query = "INSERT INTO ChildCards (Card, Main) VALUES (@child, @main)";
                                cmd.CommandText = query;
                                cmd.Parameters.AddWithValue("@child", SqlDbType.VarChar).Value = Child;
                                res = cmd.ExecuteNonQuery();
                                if (res == 1) MessageBox.Show("Card added successfully.");
                                else MessageBox.Show("There was an error, please check internet connection");
                            }
                        }
                        childCard.Text = "";
                        cmd.Connection.Close();
                    }
                }
                else MessageBox.Show("There are empty fields.", "Error");
            }
            catch (FormatException)
            {
                MessageBox.Show("Please check card numbers.", "Error");
            }
        }
    }
}
