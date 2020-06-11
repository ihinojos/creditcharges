using creditcharges.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace creditcharges
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Data.getData();
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            var instance = Controller.controller.logIn = new Views.LogIn();
            Application.Run(instance);
        }
    }
}
