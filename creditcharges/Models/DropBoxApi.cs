using DevExpress.XtraBars.Ribbon.ViewInfo;
using Dropbox.Api;
using Dropbox.Api.Files;
using System;
using System.IO;
using System.Threading.Tasks;

namespace creditcharges.Models
{
    internal class DropBoxAPI
    {
        private static string sToken = "faXwDUEbOEAAAAAAAAD8BCI7IPCVS9XCH75LfvVIJiWKkSu06rEM-4EF_CmGLOdP";
        public static string sFileName { get; set; }
        public static string sDropBoxPath { get; set; }
        public static string imagePath { get; set; }
        public static string fromPath { get; set; }
        public static string toPath { get; set; }

        public static async Task DropBoxDownload()
        {
            var client = new DropboxClient(sToken);
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
            catch
            {
            }
        }

        private static async Task<FileStream> GetStreamAsync(string path)
        {
            try
            {
                return new FileStream(path, FileMode.Open, FileAccess.Read);
            }
            catch (IOException)
            {
                await Task.Delay(TimeSpan.FromSeconds(1));
                return await GetStreamAsync(path);
            }
        }

        public static async Task DropBoxSave()
        {
            var client = new DropboxClient(sToken);
            string imageDropboxPath = sDropBoxPath;
            try
            {
                using (var file = await GetStreamAsync(imagePath))
                {
                    var updated = await client.Files.UploadAsync(
                        imageDropboxPath + sFileName,
                        WriteMode.Overwrite.Instance,
                        body: file);
                    file.Close();
                }
            }
            catch{
            }
        }

        public static async Task DropBoxDelete()
        {
            try
            {
                var client = new DropboxClient(sToken);
                await client.Files.DeleteV2Async(imagePath);
            }
            catch { }
        }

    }
}