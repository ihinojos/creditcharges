
using creditcharges.Models;
using DevExpress.Accessibility;
using DevExpress.Data.Extensions;
using DevExpress.Utils.CodedUISupport;
using DevExpress.Utils.MVVM;
using DevExpress.XtraBars.Docking2010;
using DevExpress.XtraRichEdit.Import.Doc;
using Ghostscript.NET.Rasterizer;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace creditcharges.Views
{
    public partial class EditTransaction : Form
    {
        private static string Id;
        private string location;
        private string concept;
        private readonly SqlConnection sql;
        private bool imgInf;
        private List<string> imgPaths;
        private double value;
        private ImageList imgl;
        private int lastOdo = 0;
        private string lastModel = string.Empty;

        public EditTransaction(string id, string location, string concept)
        {
            Id = id;
            sql = new SqlConnection(Data.cn);
            InitializeComponent();

            imgl = new ImageList
            {
                ImageSize = new Size(75, 50),
                ColorDepth = ColorDepth.Depth32Bit
            };
            imgPaths = new List<string>();

            listView1.View = View.SmallIcon;
            listView1.Columns.Add("Image", 250);

            mainPictureBox.AllowDrop = true;
            AddAutoCompleteOptions();

            mainPictureBox.DragEnter += new DragEventHandler(Picture_DragEnter);
            panel3.DragEnter += new DragEventHandler(Picture_DragEnter);
            listView1.DragEnter += new DragEventHandler(Picture_DragEnter);
            mainPictureBox.DragDrop += new DragEventHandler(Picture_DragDrop);
            panel3.DragDrop += new DragEventHandler(Picture_DragDrop);
            listView1.DragDrop += new DragEventHandler(Picture_DragDrop);

            if (!string.IsNullOrEmpty(Id))
            {
                if (!(string.IsNullOrEmpty(location) && string.IsNullOrEmpty(concept)))
                {
                    this.location = locationBox.Text = location;
                    this.concept = conceptBox.Text = concept;
                }
                LoadInfo();
            }
            else
            {
                dateBox.Value = DateTime.Now;
                label1.Text = "Add New Transaction";
                Text = "AddTransaction";
                //sidePic1.Visible = true;
            }
        }

        private void AddAutoCompleteOptions()
        {
            //Employee name box
            employeeBox.Items.AddRange(Data.names.ToArray());

            //Card number box
            cardBoxNum.Items.AddRange(Data.childCards.ToArray());

            //QB Account type
            accountBox.Items.AddRange(Data.accountType.ToArray());

            //Entity
            entityBox.Items.AddRange(Data.entities.ToArray());

            //Class
            classBox.Items.AddRange(Data.classes.ToArray());

            //Job Number
            jobNumBox.Items.AddRange(Data.jobNumbers.ToArray());

            //Job Name
            jobNameBox.Items.AddRange(Data.jobNames.ToArray());

            //Concept Box
            conceptBox.Items.AddRange(Data.concept.ToArray());
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
                    if (!string.IsNullOrEmpty(Id))
                        SaveInfo(Id);
                    else SaveNewInfo();
                    break;

                case "delete":
                    Delete();
                    break;
            }
        }

        private void ShowSavedRecord()
        {
            var query = "SELECT CardHolder, Card, Amount, TDate, Inc FROM Records WHERE Id = @id";
            var cmd = new SqlCommand(query, sql);
            cmd.Parameters.AddWithValue("@id", SqlDbType.VarChar).Value = Id;
            cmd.Connection.Open();
            using (var reader = cmd.ExecuteReader())
            {
                if (reader.Read())
                {
                    var name = reader[0] as string;
                    var card = reader[1] as string;
                    var amount = reader.GetDecimal(2);
                    var date = reader.GetDateTime(3);
                    var inc = reader.GetInt32(4);
                    if (card.Length == 3) card = "**** **** **** 0" + card;
                    if (card.Length == 4) card = "**** **** **** " + card;
                    if (card.Length == 5) card = "***** ***** " + card;
                    var msg = $"{name}\n" +
                      $"Card: {card}\n" +
                      $"Amount: {amount:C2}\n" +
                      $"On: {date}\n" +
                      $"Auth #: {inc}";
                    reader.Close();
                    cmd.Connection.Close();
                    Clipboard.SetText(msg);
                    MessageBox.Show(msg, "New transaction", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        private void Delete()
        {
            string message = "Do you want to delete this record?";
            MessageBoxButtons buttons = MessageBoxButtons.YesNo;
            DialogResult result = MessageBox.Show(message, "", buttons);
            if (result == DialogResult.Yes)
            {


                var query = "SELECT * FROM Images WHERE Id = @id";
                SqlCommand cmd = new SqlCommand(query, sql);
                cmd.Parameters.AddWithValue("@id", SqlDbType.VarChar).Value = Id;
                cmd.Connection.Open();
                Task task;
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var path = reader[1] as string;
                        DropBoxAPI.imagePath = path;
                        task = Task.Run(DropBoxAPI.DropBoxDelete);
                        task.Wait();
                    }
                }

                query = "DELETE FROM QuickBooks WHERE Id = @id";
                cmd.CommandText = query;
                var res = cmd.ExecuteNonQuery();
                cmd.CommandText = "DELETE FROM Images WHERE Id = @id";
                var ret = cmd.ExecuteNonQuery();
                cmd.CommandText = "DELETE FROM Records WHERE Id = @id";
                var rel = cmd.ExecuteNonQuery();
                try
                {
                    cmd.CommandText = "DELETE FROM Fuel WHERE Id = @id";
                    cmd.ExecuteNonQuery();
                }
                catch { }
                cmd.Connection.Close();
                if (res > 0 && rel > 0 && ret > 0) MessageBox.Show("Record deleted.");
                Controller.controller.mainForm.LoadTable();
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
                    cardBoxNum.Text = reader[2] as string;
                    amountBox.Text = string.Format(System.Globalization.CultureInfo.CurrentCulture, "{0:C2}", reader.GetDecimal(3));
                    conceptBox.Text = reader[4] as string == "" ? concept : reader[4] as string;
                    locationBox.Text = reader[5] as string == "" ? location : reader[5] as string;
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
                }
            }
            query = "SELECT * FROM Fuel WHERE Id = @id";
            cmd.CommandText = query;
            using (var reader = cmd.ExecuteReader())
            {
                if (reader.Read())
                {
                    odometerBox.Text = reader.GetInt32(1).ToString();
                    plateBox.Text = reader[2] as string;
                    modelBox.Text = reader[3] as string;
                    gallonsBox.Text = reader.GetDecimal(4).ToString();
                }
                LoadFuelTable();
            }
            if (cmd.Connection.State != ConnectionState.Open) cmd.Connection.Open();
            query = "SELECT * FROM Images WHERE Id = @id";
            cmd.CommandText = query;
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    imgInf = true;
                    imgPaths.Add(reader.GetString(1));
                }
            }
            cmd.Connection.Close();
            if (!imgInf) Console.WriteLine();
            else DownloadImages();
        }

        private void LoadFuelTable()
        {
            var plate = plateBox.Text.ToUpper();
            plateBox.Text = plate;
            var query = "SELECT R.Id, R.Amount, R.TDate, F.Odometer, F.Plate, F.Model, F.Gallons FROM Records R LEFT JOIN Fuel F On R.Id = F.Id WHERE F.Plate = @plate ORDER BY R.TDate ASC";
            SqlCommand cmd = new SqlCommand(query, sql);
            if (cmd.Connection.State != ConnectionState.Open) cmd.Connection.Open();
            cmd.Parameters.AddWithValue("@plate", SqlDbType.VarChar).Value = plate;
            BindingList<Fuel> fuel = new BindingList<Fuel>();
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    fuel.Add(new Fuel()
                    {
                        Id = reader[0] as string,
                        Amount = reader.GetDecimal(1),
                        Date = reader.GetDateTime(2),
                        Odometer = reader.GetInt32(3),
                        Plate = reader[4] as string,
                        Model = reader[5] as string,
                        Gallons = reader.GetDecimal(6)
                    });

                    lastOdo = reader.GetInt32(3);
                    modelBox.Text = reader[5] as string;
                    lastModel = reader[5] as string;

                }
                if (fuelControl.DataSource != null) fuelControl.DataSource = null;
                fuelView.Columns.Clear();
                fuelControl.DataSource = fuel;
                fuelView.Columns[1].Visible = false;
                fuelView.Columns[3].Visible = false;
                fuelView.Columns[5].Visible = false;
                DevExpress.XtraGrid.Columns.GridColumn price = fuelView.Columns["Amount"];
                price.DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
                price.DisplayFormat.FormatString = "c2";
                fuelView.BestFitColumns();
            }
            cmd.Connection.Close();
        }

        private void ImagesFromList(List<string> list)
        {
            try
            {
                var tmpList = new List<string>();
                if (listView1.Items.Count > 0)
                    for (int i = 0; i < listView1.Items.Count; i++)
                    {
                        try
                        {
                            var file = listView1.Items[i].Tag.ToString();
                            if (!list.Contains(file))
                                list.Add(listView1.Items[i].Tag.ToString());
                        }
                        catch { }
                    }
                listView1.Items.Clear();
                listView1.Clear();
                imgl.Images.Clear();
                imgl.Dispose();

                for (int i = 0; i < list.Count; i++)
                {
                    var file = list.ElementAt(i);
                    var ext = Path.GetExtension(file);
                    if (ext == ".jpg" || ext == ".jpeg" || ext == ".png" || ext == ".jfif" || ext == ".jpe")
                    {
                        imgl.Images.Add(Path.GetFileNameWithoutExtension(file), Image.FromFile(file));
                        listView1.Items.Add(Path.GetFileNameWithoutExtension(file), listView1.Items.Count);
                        listView1.Items[listView1.Items.Count - 1].Tag = file;
                        tmpList.Add(file);
                    }
                }

                var img = Image.FromFile(tmpList.ElementAt(0));
                img.Tag = tmpList.ElementAt(0);
                mainPictureBox.Image.Dispose();
                mainPictureBox.Image = img;
                mainPictureBox.Refresh();
                original.Image.Dispose();
                original.Image = img;
                original.Tag = img.Tag;
                original.Refresh();
                listView1.SmallImageList = imgl;
                listView1.Refresh();
                imgPaths.Clear();
                imgPaths.AddRange(tmpList);
            }
            catch { }
        }

        private List<string> PDFImage(string path)
        {
            List<string> images = new List<string>();

            using (var rasterizer = new GhostscriptRasterizer())
            {
                rasterizer.Open(path);
                for (var page = 1; page <= rasterizer.PageCount; page++)
                {
                    var imgPath = Path.Combine(Path.GetTempPath(), Path.GetTempFileName()).Replace("tmp", "jpg");
                    var img = rasterizer.GetPage(96, 96, page);
                    img.Save(imgPath, ImageFormat.Jpeg);
                    images.Add(imgPath);
                }
            }
            return images;
        }

        private void SaveNewInfo()
        {
            string maincard = "";
            var id = Guid.NewGuid().ToString("N");
           
            var employee = employeeBox.Text;
            var number = cardBoxNum.Text;
            var val = value;
            var concept = conceptBox.Text;
            var date = dateBox.Value;
            var notes = notesBox.Text;
            var user = Controller.controller.mainForm.User;
            var status = listView1.Items.Count == 0 ? "New" : "Finished";

            var query = "INSERT INTO Records (Id, CardHolder, Card, Concept, Amount, TDate, Notes, Author, Status) " +
                            "VALUES (@id, @name, @card, @concept, @amount, @date, @notes, @user, @status)";

            SqlCommand cmd = new SqlCommand(query, sql);
            cmd.Parameters.AddWithValue("@id", SqlDbType.VarChar).Value = id;
            cmd.Parameters.AddWithValue("@name", SqlDbType.VarChar).Value = employee;
            cmd.Parameters.AddWithValue("@card", SqlDbType.Int).Value = number;
            cmd.Parameters.AddWithValue("@concept", SqlDbType.VarChar).Value = concept;
            cmd.Parameters.AddWithValue("@amount", SqlDbType.Decimal).Value = val;
            cmd.Parameters.AddWithValue("@date", SqlDbType.DateTime).Value = date;
            cmd.Parameters.AddWithValue("@notes", SqlDbType.Text).Value = notes;
            cmd.Parameters.AddWithValue("@user", SqlDbType.VarChar).Value = user;
            cmd.Parameters.AddWithValue("@status", SqlDbType.VarChar).Value = status;

            if (concept == "Gasolina/Automóvil" && !checkBox1.Checked)
            {
                MessageBox.Show("Por favor, captura la información del combustible en la pestaña correspondiente.", "Advertencia", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (!(string.IsNullOrEmpty(employee) || string.IsNullOrEmpty(number) || string.IsNullOrEmpty(concept) || string.IsNullOrEmpty(number)))
            {
                Id = id;
                cmd.Connection.Open();
                var rel = cmd.ExecuteNonQuery();
                if (rel > 0)
                {
                    query = "SELECT Main FROM ChildCards WHERE Card = @card";
                    cmd.CommandText = query;
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            maincard = reader[0] as string;
                            reader.Close();
                        }
                        else
                        {
                            maincard = mainCardBox.Text;
                        }
                    }

                    var qbAcc = accountBox.Text;
                    var entity = entityBox.Text;
                    var _class = classBox.Text;
                    var jobNumber = jobNumBox.Text;
                    var jobName = jobNameBox.Text;
                    query = "INSERT INTO QuickBooks (Id, Account, Entity, Class, JobNumber, JobName, Card) VALUES " +
                        "(@id, @account, @entity, @class, @jobNumber, @jobName, @main)";
                    cmd.Parameters.AddWithValue("@account", SqlDbType.VarChar).Value = qbAcc;
                    cmd.Parameters.AddWithValue("@entity", SqlDbType.VarChar).Value = entity;
                    cmd.Parameters.AddWithValue("@class", SqlDbType.VarChar).Value = _class;
                    cmd.Parameters.AddWithValue("@jobNumber", SqlDbType.VarChar).Value = jobNumber;
                    cmd.Parameters.AddWithValue("@jobName", SqlDbType.VarChar).Value = jobName;
                    cmd.CommandText = query;
                    cmd.Parameters.AddWithValue("@main", SqlDbType.VarChar).Value = maincard;
                    var res = cmd.ExecuteNonQuery();
                    if (imgPaths.Count > 0)
                        SaveImagesAsync(entity, value.ToString(), number, date);
                    if (checkBox1.Checked)
                    {
                        query = "INSERT INTO Fuel (Id, Odometer, Plate, Model, Gallons) VALUES (@id, @odometer, @plate, @model, @gallons)";
                        var odometer = int.Parse(odometerBox.Text);
                        var plate = plateBox.Text;
                        var model = modelBox.Text;
                        var gallons = decimal.Parse(gallonsBox.Text);
                        cmd.CommandText = query;
                        cmd.Parameters.AddWithValue("@odometer", SqlDbType.Int).Value = odometer;
                        cmd.Parameters.AddWithValue("@plate", SqlDbType.VarChar).Value = plate;
                        cmd.Parameters.AddWithValue("@model", SqlDbType.VarChar).Value = model;
                        cmd.Parameters.AddWithValue("@gallons", SqlDbType.Decimal).Value = gallons;

                        if (cmd.Connection.State != ConnectionState.Open) cmd.Connection.Open();
                        cmd.ExecuteNonQuery();

                        if(model != lastModel)
                        {
                            cmd.CommandText = "UPDATE Fuel SET Model = @model WHERE Plate = @plate";
                            cmd.ExecuteNonQuery();
                        }

                    }

                    if (res == 1) MessageBox.Show("Record saved successfully.", "Success");
                    else MessageBox.Show("Please check your internet connection.", "Error");
                }
                cmd.Connection.Close();
                Controller.controller.mainForm.LoadTable();
                ShowSavedRecord();
                Dispose();
            }
            else MessageBox.Show("There are empty fields.");
        }

        private void SaveInfo(string Id)
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
            var amount = amountBox.Text.Replace("$", "").Replace(",", "");
            var card = cardBoxNum.Text;
            var author = Controller.controller.mainForm.User;
            var status = listView1.Items.Count == 0 ? "New" : "Finished";

            var query = "UPDATE Records SET Concept = @concept, Location = @location, Notes = @notes, Amount = @amount, Card = @card, Author = @author," +
                "TDate = @date, CardHolder = @employee, Status = @status WHERE Id = @id";
            SqlCommand cmd = new SqlCommand(query, sql);
            cmd.Parameters.AddWithValue("@id", SqlDbType.VarChar).Value = Id;
            cmd.Parameters.AddWithValue("@concept", SqlDbType.VarChar).Value = concept;
            cmd.Parameters.AddWithValue("@location", SqlDbType.VarChar).Value = location;
            cmd.Parameters.AddWithValue("@notes", SqlDbType.VarChar).Value = notes;
            cmd.Parameters.AddWithValue("@amount", SqlDbType.Decimal).Value = amount;
            cmd.Parameters.AddWithValue("@card", SqlDbType.VarChar).Value = card;
            cmd.Parameters.AddWithValue("@author", SqlDbType.VarChar).Value = author;
            cmd.Parameters.AddWithValue("@date", SqlDbType.DateTime).Value = date;
            cmd.Parameters.AddWithValue("@employee", SqlDbType.VarChar).Value = employee;
            cmd.Parameters.AddWithValue("@status", SqlDbType.VarChar).Value = status;

            cmd.Connection.Open();
            cmd.ExecuteNonQuery();

            query = "UPDATE QuickBooks SET Account = @account, Entity = @entity, Class = @class, " +
                "JobNumber = @jobNumber, JobName = @jobName, Card = @mainCard WHERE Id = @id";
            cmd.CommandText = query;

            cmd.Parameters.AddWithValue("@account", SqlDbType.VarChar).Value = qbAcc;
            cmd.Parameters.AddWithValue("@entity", SqlDbType.VarChar).Value = entity;
            cmd.Parameters.AddWithValue("@class", SqlDbType.VarChar).Value = _class;
            cmd.Parameters.AddWithValue("@jobNumber", SqlDbType.VarChar).Value = jobNumber;
            cmd.Parameters.AddWithValue("@jobName", SqlDbType.VarChar).Value = jobName;
            cmd.Parameters.AddWithValue("@mainCard", SqlDbType.VarChar).Value = mainCard;

            cmd.ExecuteNonQuery();

            if (checkBox1.Checked)
            {
                try
                {
                    query = "UPDATE Fuel SET Odometer = @odometer, Plate = @plate, Model = @model, Gallons = @gallons WHERE Id = @id";
                    cmd.CommandText = query;
                    var odometer = int.Parse(odometerBox.Text);
                    var plate = plateBox.Text;
                    var model = modelBox.Text;
                    var gallons = decimal.Parse(gallonsBox.Text);

                    cmd.CommandText = query;
                    cmd.Parameters.AddWithValue("@odometer", SqlDbType.Int).Value = odometer;
                    cmd.Parameters.AddWithValue("@plate", SqlDbType.VarChar).Value = plate;
                    cmd.Parameters.AddWithValue("@model", SqlDbType.VarChar).Value = model;
                    cmd.Parameters.AddWithValue("@gallons", SqlDbType.Decimal).Value = gallons;
                    cmd.ExecuteNonQuery();
                }
                catch
                {
                    MessageBox.Show("Error while saving fuel data, please try again.", "Error");
                    cmd.Connection.Close();
                }
            }

            cmd.Connection.Close();
            SaveImagesAsync(entity, amount, childCard, date);
            MessageBox.Show("The record has been saved.", "Success");
            Controller.controller.mainForm.LoadTable();
            ShowSavedRecord();
            Dispose();
        }

        private void DownloadImages()
        {

            Task task;
            try
            {
                var iter = imgPaths.Count;
                for (int i = 0; i < iter; i++)
                {
                    var path = imgPaths.ElementAt(i);
                    DropBoxAPI.sFileName = Path.GetFileName(path);
                    DropBoxAPI.sDropBoxPath = Path.GetDirectoryName(path).Replace("\\", "/");
                    task = Task.Run(DropBoxAPI.DropBoxDownload);
                    task.Wait();
                    imgPaths.RemoveAt(i);
                    path = Path.Combine(Path.GetTempPath(), DropBoxAPI.sFileName);
                    imgl.Images.Add(Path.GetFileNameWithoutExtension(path), Image.FromFile(path));
                    listView1.Items.Add(Path.GetFileNameWithoutExtension(path), i);
                    listView1.Items[i].Tag = path;
                    imgPaths.Insert(i, path);
                }
                mainPictureBox.Image = Image.FromFile(Path.Combine(Path.GetTempPath(), DropBoxAPI.sFileName));
                listView1.SmallImageList = imgl;
                listView1.Refresh();
            }
            catch
            {
                MessageBox.Show("Error downloading images, the file could've been moved or deleted.", "Error");
            }
        }

        private void SaveImagesAsync(string entity, string amount, string child, DateTime date)
        {
            var query = "SELECT * FROM Images WHERE Id = @id";
            SqlCommand cmd = new SqlCommand(query, sql);
            cmd.Parameters.AddWithValue("@id", SqlDbType.VarChar).Value = Id;
            if (cmd.Connection.State != ConnectionState.Open)
                cmd.Connection.Open();
            Task task;
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    var path = reader[1] as string;
                    DropBoxAPI.imagePath = path;
                    task = Task.Run(DropBoxAPI.DropBoxDelete);
                    task.Wait();
                }
            }
            query = "DELETE FROM Images WHERE Id = @id";
            cmd.CommandText = query;
            cmd.ExecuteNonQuery();
            var stamp = date.ToString("MM.dd.y");
            mainPictureBox.Image.Dispose();
            original.Image.Dispose();
            DropBoxAPI.sDropBoxPath = $"/Tickets/{DateTime.Today:yyyy}/";

            var filename = stamp + "_" + amount + "_" + child;
            if (!string.IsNullOrEmpty(entity))
            {
                filename += "_" + entity;
                DropBoxAPI.sDropBoxPath = $"/Tickets/" + entity + "/";
            }
            filename = filename.Replace(" ", "_");

            query = "INSERT INTO Images (Id, ImgPath) VALUES (@id, @path)";
            cmd.Parameters.AddWithValue("@path", SqlDbType.VarChar);
            for (int i = 0; i < imgPaths.Count; i++)
            {
                var file = imgPaths.ElementAt(i);
                var ext = Path.GetExtension(file);
                DropBoxAPI.sFileName = filename + "_(" + i + ")" + ext;
                DropBoxAPI.imagePath = file;
                task = Task.Run(DropBoxAPI.DropBoxSave);
                task.Wait();
                var sqlPath = DropBoxAPI.sDropBoxPath + DropBoxAPI.sFileName;
                cmd.CommandText = query;
                cmd.Parameters["@path"].Value = sqlPath;
                cmd.ExecuteNonQuery();
            }
            cmd.Connection.Close();
            try
            {
                mainPictureBox.Image = Image.FromFile(imgPaths.ElementAt(0));
            }
            catch { }
        }

        private void Picture_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.Copy;
        }

        private void Picture_DragDrop(object sender, DragEventArgs e)
        {
            try
            {
                var data = e.Data.GetData(DataFormats.FileDrop);
                if (data != null)
                {
                    var fileNames = data as string[];
                    var images = new List<string>();
                    if (fileNames.Length > 0)
                    {
                        foreach (var image in fileNames)
                        {
                            if (Path.GetExtension(image) == ".pdf")
                            {
                                ImagesFromList(PDFImage(image));
                            }
                            else
                            {
                                images.Add(image);
                            }
                        }
                        if (images.Count > 0) ImagesFromList(images);
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
            zoomBar.Properties.Minimum = 100;
            zoomBar.Properties.Maximum = 3000;
            zoomBar.Properties.SmallChange = 100;
            zoomBar.Properties.LargeChange = 100;
            zoomBar.UseWaitCursor = false;
            this.DoubleBuffered = true;
            original = new PictureBox
            {
                Image = mainPictureBox.Image
            };
            zoomBar.Value = 200;
            zoomBar_Scroll(null, null);
        }

        private Image ZoomPicture(Image img, Size size)
        {
            Bitmap bmp = new Bitmap(img, Convert.ToInt32((img.Width + size.Width) / 2), Convert.ToInt32((img.Height + size.Height) / 2));
            Graphics graphics = Graphics.FromImage(bmp);
            graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            return bmp;
        }

        private PictureBox original;

        private void zoomBar_Scroll(object sender, EventArgs e)
        {
            if (zoomBar.Value != 0)
            {
                mainPictureBox.Image = null;
                mainPictureBox.Image = ZoomPicture(original.Image, new Size(zoomBar.Value, zoomBar.Value));
            }
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            odometerBox.Enabled = checkBox1.Checked;
            plateBox.Enabled = checkBox1.Checked;
            modelBox.Enabled = checkBox1.Checked;
            gallonsBox.Enabled = checkBox1.Checked;
        }

        private void amountBox_Leave(object sender, EventArgs e)
        {
            if (double.TryParse(amountBox.Text, out value))
                amountBox.Text = string.Format(System.Globalization.CultureInfo.CurrentCulture, "{0:C2}", value);
            else
                amountBox.Text = string.Empty;
        }

        public void LoadReport(DateTime date, string card, string location, string concept, string amount)
        {
            dateBox.Value = date;
            cardBoxNum.Text = card;
            locationBox.Text = location;
            conceptBox.Text = concept;
            value = double.Parse(amount);
            amountBox.Text = "$" + value;
        }

        private void jobNameBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            var idx = jobNameBox.SelectedIndex;
            jobNumBox.SelectedIndex = idx;
        }

        private void windowsUIButtonPanel2_ButtonClick(object sender, ButtonEventArgs e)
        {
            var tag = ((WindowsUIButton)e.Button).Tag.ToString();
            switch (tag)
            {
                case "add":
                    AddImages();
                    break;
                case "remove":
                    RemoveImage();
                    break;
                case "clear":
                    ClearImages();
                    break;
            }
        }

        private void AddImages()
        {
            OpenFileDialog open = new OpenFileDialog
            {
                Filter = "Image files (*.jpg, *.jpeg, *.jpe, *.jfif, *.png) | *.jpg; *.jpeg; *.jpe; *.jfif; *.png|PDF Files (*.pdf) | *.pdf",
                Multiselect = true
            };
            if (open.ShowDialog() == DialogResult.OK)
            {
                var fileNames = open.FileNames;
                var images = new List<string>();
                if (fileNames.Length > 0)
                {
                    foreach (var image in fileNames)
                    {
                        if (Path.GetExtension(image) == ".pdf")
                        {
                            ImagesFromList(PDFImage(image));
                        }
                        else
                        {
                            images.Add(image);
                        }
                    }
                    if (images.Count > 0) ImagesFromList(images);
                }
            }
        }

        private void RemoveImage()
        {
            var selected = listView1.SelectedItems.Count;
            if (selected > 0)
            {
                for (int i = selected - 1; i > -1; i--)
                {
                    var idx = listView1.Items.IndexOf(listView1.SelectedItems[i]);
                    listView1.Items.RemoveAt(idx);
                    imgPaths.RemoveAt(idx);
                }
                ImagesFromList(imgPaths);
                if (imgPaths.Count == 0)
                {
                    mainPictureBox.Image.Dispose();
                    mainPictureBox.Image = Properties.Resources.No_Picture1;
                    original.Image.Dispose();
                    original.Image = Properties.Resources.No_Picture1;
                }
            }
        }

        private void ClearImages()
        {
            mainPictureBox.Image.Dispose();
            mainPictureBox.Image = Properties.Resources.No_Picture1;
            original.Image.Dispose();
            original.Image = Properties.Resources.No_Picture1;
            imgPaths.Clear();
            listView1.Items.Clear();
            imgl.Images.Clear();
            listView1.Refresh();
        }

        private void listView1_DoubleClick(object sender, EventArgs e)
        {
            var selected = listView1.SelectedItems.Count;
            if (selected > 0)
            {
                var idx = listView1.Items.IndexOf(listView1.SelectedItems[0]);
                try
                {
                    mainPictureBox.Image.Dispose();
                    mainPictureBox.Image = Image.FromFile(imgPaths.ElementAt(idx));
                    mainPictureBox.Refresh();
                    original.Image.Dispose();
                    original.Image = Image.FromFile(imgPaths.ElementAt(idx));
                    original.Tag = imgPaths.ElementAt(idx);
                    original.Refresh();
                }
                catch
                {

                }
            }
        }

        private void plateBox_Leave(object sender, EventArgs e)
        {
            if (plateBox.Text.Split(new char[] { ' ' }).Length != 2)
            {
                MessageBox.Show("El formato de las placas es erróneo, debe llevar espacio entre los valores, ejemplo: \"ABC 1234\"." +
                    "\nPor favor vuelva a reintroducir el valor.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

                plateBox.Text = string.Empty;
            }
            else
            {
                try
                {
                    LoadFuelTable();
                }
                catch { }
            }
        }

        private void odometerBox_Leave(object sender, EventArgs e)
        {
            try
            {
                var odo = int.Parse(odometerBox.Text);
                if (odo <= lastOdo)
                {
                    MessageBox.Show("Las millas ingresadas son menores al último registro.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    odometerBox.Text = string.Empty;
                }
            }catch
            {
                odometerBox.Text = string.Empty;
            }
        }
    }
}