using creditcharges.Models;
using System;
using System.Windows.Forms;

namespace creditcharges
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        private static void Main()
        {
            Data.getData();
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            //Create singleton instance for login view

            var instance = Controller.controller.logIn = new Views.LogIn();
            Application.Run(instance);
        }
    }
}