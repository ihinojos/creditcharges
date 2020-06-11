using creditcharges.Models;
using DevExpress.Utils.DragDrop;
using DevExpress.XtraBars.Docking2010;
using DevExpress.XtraBars.Ribbon.Gallery;
using DevExpress.XtraEditors.Filtering.Templates;
using DevExpress.XtraPrinting;
using DevExpress.XtraTab.Drawing;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace creditcharges.Views
{
    public partial class EditTransaction : Form
    {
        private static string Id;
        private readonly SqlConnection sql;
        private bool imgInf;
        private int imgCount = 0;
        private List<string> imgPaths = new List<string>();
        public EditTransaction(string id, string location, string concept)
        {
            Id = id;
            sql = new SqlConnection(Data.cn);
            InitializeComponent();
            mainPictureBox.AllowDrop = true;
            sidePic1.AllowDrop = true;
            sidePic2.AllowDrop = true;
            sidePic3.AllowDrop = true;
            sidePic4.AllowDrop = true;
            AddAutoCompleteOptions();

            sidePic1.Click += new EventHandler(pictureBox_Click);
            sidePic2.Click += new EventHandler(pictureBox_Click);
            sidePic3.Click += new EventHandler(pictureBox_Click);
            sidePic4.Click += new EventHandler(pictureBox_Click);

            mainPictureBox.DragEnter += new DragEventHandler(PictureBox_DragEnter);
            sidePic1.DragEnter += new DragEventHandler(PictureBox_DragEnter);
            sidePic2.DragEnter += new DragEventHandler(PictureBox_DragEnter);
            sidePic3.DragEnter += new DragEventHandler(PictureBox_DragEnter);
            sidePic4.DragEnter += new DragEventHandler(PictureBox_DragEnter);

            mainPictureBox.DragDrop += new DragEventHandler(PictureBox_DragGrop);
            sidePic1.DragDrop += new DragEventHandler(PictureBox_DragGrop);
            sidePic2.DragDrop += new DragEventHandler(PictureBox_DragGrop);
            sidePic3.DragDrop += new DragEventHandler(PictureBox_DragGrop);
            sidePic4.DragDrop += new DragEventHandler(PictureBox_DragGrop);


            LoadInfo();
            if (!(string.IsNullOrEmpty(location) && string.IsNullOrEmpty(concept)))
            {
                locationBox.Text = location;
                conceptBox.Text = concept;
            }
        }

        private void AddAutoCompleteOptions()
        {
            //Employee name box
            AutoCompleteStringCollection names = new AutoCompleteStringCollection();
            names.AddRange(Data.names.ToArray());
            employeeBox.AutoCompleteCustomSource = names;

            //QB Account type
            AutoCompleteStringCollection accounts = new AutoCompleteStringCollection();
            accounts.AddRange(Data.accountType.ToArray());
            accountBox.AutoCompleteCustomSource = accounts;

            //Entity
            AutoCompleteStringCollection entities = new AutoCompleteStringCollection();
            entities.AddRange(Data.entities.ToArray());
            entityBox.AutoCompleteCustomSource = entities;

            //Class 
            AutoCompleteStringCollection classes = new AutoCompleteStringCollection();
            classes.AddRange(Data.classes.ToArray());
            classBox.AutoCompleteCustomSource = classes;

            //Job Number 
            jobNumBox.Items.AddRange(Data.jobNumbers.ToArray());

            //Job Name
            jobNameBox.Items.AddRange(Data.jobNames.ToArray());


        }

        private void windowsUIButtonPanel1_ButtonClick(object sender, DevExpress.XtraBars.Docking2010.ButtonEventArgs e)
        {
            var tag = ((WindowsUIButton)e.Button).Tag.ToString();
            switch (tag)
            {
                case "cancel":
                    Dispose();
                    break;
                case "save":
                    SaveInfo();
                    break;
                case "delete":
                    Delete();
                    break;
            }
        }

        private void Delete()
        {
            string message = "Do you want to delete this record?";
            MessageBoxButtons buttons = MessageBoxButtons.YesNo;
            DialogResult result = MessageBox.Show(message, "", buttons);
            if (result == DialogResult.Yes)
            {
                var query = "DELETE FROM QuickBooks WHERE Id = @id";
                SqlCommand cmd = new SqlCommand(query, sql);
                cmd.Parameters.AddWithValue("@id", SqlDbType.VarChar).Value = Id;
                cmd.Connection.Open();
                var res = cmd.ExecuteNonQuery();
                cmd.CommandText = "DELETE FROM Images WHERE Id = @id";
                var ret = cmd.ExecuteNonQuery();
                cmd.CommandText = "DELETE FROM Records WHERE Id = @id";
                var rel = cmd.ExecuteNonQuery();
                cmd.Connection.Close();
                if (res > 0 && rel > 0 && ret > 0) MessageBox.Show("Record deleted.");
                Dispose();
            }
        }

        private void LoadInfo()
        {
            var query = "SELECT * FROM Records WHERE Id = @id";
            SqlCommand cmd = new SqlCommand(query, sql);
            cmd.Parameters.AddWithValue("@id", SqlDbType.VarChar).Value = Id;
            cmd.Connection.Open();
            using (var reader = cmd.ExecuteReader())
            {
                if (reader.Read())
                {
                    employeeBox.Text = reader[1] as string;
                    employeeBox.Enabled = false;
                    cardBoxNum.Text = reader[2] as string;
                    cardBoxNum.Enabled = false;
                    amountBox.Text = string.Format(System.Globalization.CultureInfo.CurrentCulture, "{0:C2}", reader.GetDecimal(3));
                    conceptBox.Text = reader[4] as string;
                    locationBox.Text = reader[5] as string;
                    dateBox.Value = reader.GetDateTime(6);
                    notesBox.Text = reader[7] as string;
                }
            }
            query = "SELECT * FROM QuickBooks WHERE Id = @id";
            cmd.CommandText = query;
            using (var reader = cmd.ExecuteReader())
            {
                if (reader.Read())
                {
                    accountBox.Text = reader[1] as string;
                    entityBox.Text = reader[2] as string;
                    classBox.Text = reader[3] as string;
                    jobNumBox.Text = reader[4] as string;
                    jobNameBox.Text = reader[5] as string;
                    mainCardBox.Text = reader[6] as string;
                    mainCardBox.Enabled = false;
                }
            }
            query = "SELECT * FROM Images WHERE Id = @id";
            cmd.CommandText = query;
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    imgInf = true;
                    imgPaths.Add(reader.GetString(1));
                }
                cmd.Connection.Close();
            }
            if (!imgInf) sidePic1.Visible = true;
            else DownloadImages();

        }

        private void SaveInfo()
        {
            var employee = employeeBox.Text;
            var childCard = cardBoxNum.Text;
            var concept = conceptBox.Text;
            var location = locationBox.Text;
            var notes = notesBox.Text;
            var qbAcc = accountBox.Text;
            var entity = entityBox.Text;
            var _class = classBox.Text;
            var jobNumber = jobNumBox.Text;
            var jobName = jobNameBox.Text;
            var mainCard = mainCardBox.Text;
            var date = dateBox.Value;

            var query = "UPDATE Records SET Concept = @concept, Location = @location, Notes = @notes " +
                "WHERE Id = @id";
            SqlCommand cmd = new SqlCommand(query, sql);
            cmd.Parameters.AddWithValue("@id", SqlDbType.VarChar).Value = Id;
            cmd.Parameters.AddWithValue("@concept", SqlDbType.VarChar).Value = concept;
            cmd.Parameters.AddWithValue("@location", SqlDbType.VarChar).Value = location;
            cmd.Parameters.AddWithValue("@notes", SqlDbType.VarChar).Value = notes;

            cmd.Connection.Open();
            cmd.ExecuteNonQuery();

            query = "UPDATE QuickBooks SET Account = @account, Entity = @entity, Class = @class, " +
                "JobNumber = @jobNumber, JobName = @jobName WHERE Id = @id";
            cmd.CommandText = query;

            cmd.Parameters.AddWithValue("@account", SqlDbType.VarChar).Value = qbAcc;
            cmd.Parameters.AddWithValue("@entity", SqlDbType.VarChar).Value = entity;
            cmd.Parameters.AddWithValue("@class", SqlDbType.VarChar).Value = _class;
            cmd.Parameters.AddWithValue("@jobNumber", SqlDbType.VarChar).Value = jobNumber;
            cmd.Parameters.AddWithValue("@jobName", SqlDbType.VarChar).Value = jobName;
            cmd.ExecuteNonQuery();
            cmd.Connection.Close();

            SaveImagesAsync(employee, mainCard, childCard, date);
            MessageBox.Show("The record has been saved.", "Success");
            Dispose();
        }

        private void DownloadImages()
        {
            Task task;
            if (imgPaths.Count > 0)
            {
                DropBoxAPI.sFileName = Path.GetFileName(imgPaths.ElementAt(0));
                DropBoxAPI.sDropBoxPath = Path.GetDirectoryName(imgPaths.ElementAt(0)).Replace("\\", "/");
                task = Task.Run(DropBoxAPI.DropBoxDownload);
                task.Wait();
                sidePic1.Image.Dispose();
                sidePic1.Image = Image.FromFile(Path.Combine(Path.GetTempPath(), DropBoxAPI.sFileName));
                sidePic1.Tag = Path.Combine(Path.GetTempPath(), DropBoxAPI.sFileName);
                sidePic1.ImageLocation = sidePic1.Tag.ToString();
                sidePic1.Visible = true;
            }
            if (imgPaths.Count > 1)
            {
                DropBoxAPI.sFileName = Path.GetFileName(imgPaths.ElementAt(1));
                DropBoxAPI.sDropBoxPath = Path.GetDirectoryName(imgPaths.ElementAt(1)).Replace("\\", "/");
                task = Task.Run(DropBoxAPI.DropBoxDownload);
                task.Wait();
                sidePic2.Image.Dispose();
                sidePic2.Image = Image.FromFile(Path.Combine(Path.GetTempPath(), DropBoxAPI.sFileName));
                sidePic2.Tag = Path.Combine(Path.GetTempPath(), DropBoxAPI.sFileName);
                sidePic2.ImageLocation = sidePic2.Tag.ToString();
                sidePic2.Visible = true;
            }
            if (imgPaths.Count > 2)
            {
                DropBoxAPI.sFileName = Path.GetFileName(imgPaths.ElementAt(2));
                DropBoxAPI.sDropBoxPath = Path.GetDirectoryName(imgPaths.ElementAt(2)).Replace("\\", "/");
                task = Task.Run(DropBoxAPI.DropBoxDownload);
                task.Wait();
                sidePic3.Image.Dispose();
                sidePic3.Image = Image.FromFile(Path.Combine(Path.GetTempPath(), DropBoxAPI.sFileName));
                sidePic3.Tag = Path.Combine(Path.GetTempPath(), DropBoxAPI.sFileName);
                sidePic3.ImageLocation = sidePic3.Tag.ToString();
                sidePic3.Visible = true;
            }
            if (imgPaths.Count > 3)
            {
                DropBoxAPI.sFileName = Path.GetFileName(imgPaths.ElementAt(3));
                DropBoxAPI.sDropBoxPath = Path.GetDirectoryName(imgPaths.ElementAt(3)).Replace("\\", "/");
                task = Task.Run(DropBoxAPI.DropBoxDownload);
                task.Wait();
                sidePic4.Image.Dispose();
                sidePic4.Image = Image.FromFile(Path.Combine(Path.GetTempPath(), DropBoxAPI.sFileName));
                sidePic4.Tag = Path.Combine(Path.GetTempPath(), DropBoxAPI.sFileName);
                sidePic1.ImageLocation = sidePic1.Tag.ToString();
                sidePic4.Visible = true;
            }
            mainPictureBox.Image = Image.FromFile(sidePic1.ImageLocation);
        }

        private void SaveImagesAsync(string employee, string main, string child, DateTime date)
        {
            var query = "DELETE FROM Images WHERE Id = @id";
            SqlCommand cmd = new SqlCommand(query, sql);
            cmd.Parameters.AddWithValue("@id", SqlDbType.VarChar).Value = Id;
            cmd.Connection.Open();
            cmd.ExecuteNonQuery();
            var stamp = ((DateTimeOffset)date).ToUnixTimeSeconds();
            mainPictureBox.Image.Dispose();
            original.Image.Dispose();
            Task task;
            DropBoxAPI.sDropBoxPath = $"/DAILY PICK UP LOADS/Credit Card Tickets/{DateTime.Today:yyyy}/";
            var filename = main + "_" + child + "_" + employee + "_" + stamp;
            if (!string.IsNullOrEmpty(sidePic1.ImageLocation))
            {
                var ext = Path.GetExtension(sidePic1.ImageLocation);
                sidePic1.Image.Dispose();
                DropBoxAPI.sFileName = filename + " (1)" + ext;
                DropBoxAPI.imagePath = sidePic1.ImageLocation;
                task = Task.Run(DropBoxAPI.DropBoxSave);
                task.Wait();
                var sqlPath = DropBoxAPI.sDropBoxPath + DropBoxAPI.sFileName;
                query = "INSERT INTO Images (Id, ImgPath) VALUES (@id, @path)";
                cmd.CommandText = query;
                cmd.Parameters.AddWithValue("@path", SqlDbType.VarChar).Value = sqlPath;
                cmd.ExecuteNonQuery();
                sidePic1.Image = Image.FromFile(sidePic1.ImageLocation);
            }
            if (!string.IsNullOrEmpty(sidePic2.ImageLocation))
            {
                var ext = Path.GetExtension(sidePic2.ImageLocation);
                sidePic2.Image.Dispose();
                DropBoxAPI.sFileName = filename + " (2)" + ext;
                DropBoxAPI.imagePath = sidePic2.ImageLocation;
                task = Task.Run((Func<Task>)DropBoxAPI.DropBoxSave);
                task.Wait();
                var sqlPath = DropBoxAPI.sDropBoxPath + DropBoxAPI.sFileName;
                cmd.Parameters["@path"].Value = sqlPath;
                cmd.ExecuteNonQuery();
                sidePic2.Image = Image.FromFile(sidePic2.ImageLocation);
            }
            if (!string.IsNullOrEmpty(sidePic3.ImageLocation))
            {
                var ext = Path.GetExtension(sidePic3.ImageLocation);
                sidePic3.Image.Dispose();
                DropBoxAPI.sFileName = filename + " (3)" + ext;
                DropBoxAPI.imagePath = sidePic3.ImageLocation;
                task = Task.Run((Func<Task>)DropBoxAPI.DropBoxSave);
                task.Wait();
                var sqlPath = DropBoxAPI.sDropBoxPath + DropBoxAPI.sFileName;
                cmd.Parameters["@path"].Value = sqlPath;
                cmd.ExecuteNonQuery();
                sidePic3.Image = Image.FromFile(sidePic3.ImageLocation);
            }
            if (!string.IsNullOrEmpty(sidePic4.ImageLocation))
            {
                var ext = Path.GetExtension(sidePic4.ImageLocation);
                sidePic4.Image.Dispose();
                DropBoxAPI.sFileName = filename + " (4)" + ext;
                DropBoxAPI.imagePath = sidePic4.ImageLocation;
                task = Task.Run((Func<Task>)DropBoxAPI.DropBoxSave);
                task.Wait();
                var sqlPath = DropBoxAPI.sDropBoxPath + DropBoxAPI.sFileName;
                cmd.Parameters["@path"].Value = sqlPath;
                cmd.ExecuteNonQuery();
                sidePic4.Image = Image.FromFile(sidePic4.ImageLocation);
            }
            cmd.Connection.Close();
            mainPictureBox.Image = Image.FromFile(sidePic1.ImageLocation);
        }

        private void pictureBox_Click(object sender, EventArgs e)
        {
            PictureBox pic = (PictureBox)sender;
            try
            {
                mainPictureBox.Image.Dispose();
                mainPictureBox.Image = Image.FromFile(pic.Tag.ToString());
                mainPictureBox.Refresh();
                original.Image.Dispose();
                original.Image = Image.FromFile(pic.Tag.ToString());
                original.Tag = pic.Tag;
                original.Refresh();
            }
            catch
            {
                OpenFileDialog open = new OpenFileDialog
                {
                    Filter = "Image files (*.jpg, *.jpeg, *.jpe, *.jfif, *.png) | *.jpg; *.jpeg; *.jpe; *.jfif; *.png"
                };
                if (open.ShowDialog() == DialogResult.OK)
                {
                    var img = Image.FromFile(open.FileName);
                    img.Tag = open.FileName;

                    mainPictureBox.Image.Dispose();
                    mainPictureBox.Image = img;
                    mainCardBox.Refresh();
                    original.Image.Dispose();
                    original.Image = img;
                    original.Tag = img.Tag;
                    original.Refresh();
                    if (pic == sidePic1)
                    {
                        sidePic1.Image.Dispose();
                        sidePic1.Image = img;
                        sidePic1.Tag = img.Tag;
                        sidePic1.ImageLocation = img.Tag.ToString();
                        sidePic1.Refresh();
                        sidePic2.Visible = true;
                        imgCount = 1;
                    }
                    else if (pic == sidePic2)
                    {
                        sidePic2.Image.Dispose();
                        sidePic2.Image = img;
                        sidePic2.Tag = img.Tag;
                        sidePic2.ImageLocation = img.Tag.ToString();
                        sidePic2.Refresh();
                        sidePic3.Visible = true;
                        imgCount = 2;
                    }
                    else if (pic == sidePic3)
                    {
                        sidePic3.Image.Dispose();
                        sidePic3.Image = img;
                        sidePic3.Tag = img.Tag;
                        sidePic3.ImageLocation = img.Tag.ToString();
                        sidePic3.Refresh();
                        sidePic4.Visible = true;
                        imgCount = 3;
                    }
                    else if (pic == sidePic4)
                    {
                        sidePic4.Image.Dispose();
                        sidePic4.Image = img;
                        sidePic4.Tag = img.Tag;
                        sidePic4.ImageLocation = img.Tag.ToString();
                        sidePic4.Refresh();
                        imgCount = 0;
                    }
                }

            }
        }

        private void PictureBox_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.Copy;
        }

        private void PictureBox_DragGrop(object sender, DragEventArgs e)
        {
            try
            {
                var data = e.Data.GetData(DataFormats.FileDrop);
                if (data != null)
                {
                    var fileNames = data as string[];
                    if (fileNames.Length > 0)
                    {
                        var img = Image.FromFile(fileNames[0]);
                        img.Tag = fileNames[0];
                        mainPictureBox.Image.Dispose();
                        mainPictureBox.Image = img;
                        mainPictureBox.Refresh();
                        original.Image.Dispose();
                        original.Image = img;
                        original.Tag = img.Tag;
                        original.Refresh();
                        if (imgCount == 0)
                        {
                            sidePic1.Image.Dispose();
                            sidePic1.Image = img;
                            sidePic1.Tag = img.Tag;
                            sidePic1.ImageLocation = img.Tag.ToString();
                            sidePic1.Refresh();
                            imgCount = 1;
                            sidePic2.Visible = true;
                        }
                        else if (imgCount == 1)
                        {
                            sidePic2.Image.Dispose();
                            sidePic2.Image = img;
                            sidePic2.Tag = img.Tag;
                            sidePic2.ImageLocation = img.Tag.ToString();
                            sidePic2.Refresh();
                            imgCount = 2;
                            sidePic3.Visible = true;
                        }
                        else if (imgCount == 2)
                        {
                            sidePic3.Image.Dispose();
                            sidePic3.Image = img;
                            sidePic3.Tag = img.Tag;
                            sidePic3.ImageLocation = img.Tag.ToString();
                            sidePic3.Refresh();
                            imgCount = 3;
                            sidePic4.Visible = true;
                        }
                        else if (imgCount == 3)
                        {
                            sidePic4.Image.Dispose();
                            sidePic4.Image = img;
                            sidePic4.Tag = img.Tag;
                            sidePic4.ImageLocation = img.Tag.ToString();
                            sidePic4.Refresh();
                            imgCount = 0;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void jobNumBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            var idx = jobNumBox.SelectedIndex;
            jobNameBox.SelectedIndex = idx;
        }

        private void EditTransaction_Load(object sender, EventArgs e)
        {
            trackBar1.Minimum = 1;
            trackBar1.Maximum = 300;
            trackBar1.SmallChange = 10;
            trackBar1.LargeChange = 10;
            trackBar1.UseWaitCursor = false;

            this.DoubleBuffered = true;
            original = new PictureBox
            {
                Image = mainPictureBox.Image
            };
        }

        Image ZoomPicture(Image img, Size size)
        {
            Bitmap bmp = new Bitmap(img, Convert.ToInt32(img.Width * size.Width / 100), Convert.ToInt32(img.Height * size.Height / 100));
            Graphics graphics = Graphics.FromImage(bmp);
            graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            return bmp;
        }

        PictureBox original;

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            if (trackBar1.Value != 0)
            {
                mainPictureBox.Image = null;
                mainPictureBox.Image = ZoomPicture(original.Image, new Size(trackBar1.Value, trackBar1.Value));
            }
        }
    }
}
