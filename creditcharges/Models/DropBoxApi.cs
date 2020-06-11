using Dropbox.Api;
using Dropbox.Api.Files;
using System;
using System.IO;
using System.Threading.Tasks;

namespace creditcharges.Models
{
    class DropBoxAPI
    {
        private static string sToken = "H6ShxgYjLJAAAAAAAAaM3XfEGX0t1E52OiIfKns6fF9PbyCUP7rrolnbe8ckx5KZ";
        public static string sFileName { get; set; }
        public static string sDropBoxPath { get; set; }
        public static string imagePath { get; set; }
        public static async Task DropBoxDownload()
        {
            var client = new DropboxClient(sToken);
            //string folder = $"/DAILY PICK UP LOADS/Credit Card Tickets/{DateTime.Today.ToString("yyyy")}/";
            string folder = sDropBoxPath;
            string localFile = Path.Combine(Path.GetTempPath(), sFileName);
            try
            {
                using (var response = await client.Files.DownloadAsync(folder + "/" + sFileName))
                {
                    using (var fileStream = File.Create(localFile))
                    {
                        (await response.GetContentAsStreamAsync()).CopyTo(fileStream);
                        fileStream.Close();
                    }
                }
            }
            catch (Exception ex)
            {
            }
        }
        public static async Task DropBoxSave()
        {
           // bool bFolderExist = true;
            var client = new DropboxClient(sToken);
            //string folder = $"/DAILY PICK UP LOADS/Credit Card Tickets/{DateTime.Today.ToString("yyyy")}/";
            string imageDropboxPath = sDropBoxPath;
            /***********  Checks for existing DropBox folder ***********/
            //var list = await client.Files.ListFolderAsync(imageDropboxPath);
            //foreach (var item in list.Entries.Where(i => i.IsFolder))
            //{
            //    if (item.Name.ToString() == "Tickets")
            //    {
            //        bFolderExist = true;
            //    }
            //    Console.WriteLine("D  {0}/", item.Name);
            //}
            //imageDropboxPath += "/Tickets";
            /*************  Creates folder if doesn't exist *************/
            //if (!bFolderExist)
            //{
            //    Console.WriteLine("--- Creating Folder ---");
            //    var folderArg = new CreateFolderArg(imageDropboxPath);
            //    var folder = await client.Files.CreateFolderV2Async(folderArg);
            //    Console.WriteLine("Folder: " + imageDropboxPath + " created!");
            //}
            /************** Copy image file to DropBox folder   ******************/
            try
            {
                using (var file = new FileStream(imagePath, FileMode.Open))
                {
                    var updated = await client.Files.UploadAsync(
                        imageDropboxPath + sFileName,
                        WriteMode.Overwrite.Instance,
                        body: file);
                    file.Close();
                }
            }
            catch (Exception ex) { }
        }
    }
}