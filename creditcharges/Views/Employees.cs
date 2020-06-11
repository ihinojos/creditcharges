using creditcharges.Models;
using DevExpress.XtraBars.Docking2010;
using DevExpress.XtraRichEdit.Layout;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace creditcharges.Views
{
    public partial class Employees : Form
    {
        public Employees()
        {
            InitializeComponent();
            LoadData();
        }

        private void LoadData()
        {
            var names = Data.names.ToArray();
            comboBox1.Items.Clear();
            comboBox1.Items.AddRange(names);
            AutoCompleteStringCollection collection = new AutoCompleteStringCollection();
            collection.AddRange(names);
            comboBox1.AutoCompleteCustomSource = collection;
        }

        private void windowsUIButtonPanel1_ButtonClick(object sender, ButtonEventArgs e)
        {
            var tag = ((WindowsUIButton)e.Button).Tag.ToString();
            switch (tag)
            {
                case "add":
                    AddEmployee();
                    break;
                case "remove":
                    RemoveEmployee();
                    break;
                case "cancel":
                    Dispose();
                    break;
            }
        }

        private void AddEmployee()
        {
            if (!Data.names.Contains(comboBox1.Text))
            {
                using (StreamWriter w = File.AppendText("Resources\\names.txt")) w.WriteLine(comboBox1.Text);
                Data.getData();
                LoadData();
                MessageBox.Show("Employee added");
            }
            else MessageBox.Show("Employee already exists");
        }

        private void RemoveEmployee()
        {
            if (Data.names.Contains(comboBox1.Text))
            {
                using (StreamReader sr = new StreamReader("Resources\\names.txt"))
                {
                    var allText = sr.ReadToEnd();
                    allText = allText.Replace(comboBox1.Text, string.Empty);
                    sr.Close();
                    StreamWriter sw = new StreamWriter("Resources\\names.txt");
                    var array = allText.Split(new char[] { '\n','\r' });
                    foreach (var line in array)
                    {
                        if (!string.IsNullOrEmpty(line))
                        {
                            sw.WriteLine(line);
                        }
                    }
                    sw.Close();
                    Data.getData();
                    LoadData();
                    MessageBox.Show("Employee deleted");
                }
            }
            else MessageBox.Show("Employee does not exist");
        }
    }
}
