using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;

namespace creditcharges.Models
{
    internal class Data
    {

        #region Attributes

        private static SqlConnection sql;
        private static string[] concepts = {"Alojamiento","Atención Médica", "Combustible/Vehículo", "Internet", "Mercancía", "Otra Opción",
            "Otros Servicios", "Pago/Crédito", "Seguros", "Servicios Profesionales", "Servicios Púbilcos", "Teléfono/Cable", "Herramientas/Mantenimiento"};

        //public static string cn = @"Server=INTLGXCUU24\INTELOGIX;Initial Catalog=TESTTRANS;MultipleActiveResultSets=true;Persist Security Info=True;User ID=Intelogix;Password=Intelogix20XX!";
        public static string cn = @"Server=INTLGXCUU24\INTELOGIX;Initial Catalog=PRODTRANS;MultipleActiveResultSets=true;Persist Security Info=True;User ID=Intelogix;Password=Intelogix20XX!";

        public static List<string> accountType { get; set; }
        public static List<string> entities { get; set; }
        public static List<string> jobNames { get; set; }
        public static List<string> jobNumbers { get; set; }
        public static List<string> classes { get; set; }
        public static List<string> names { get; set; }
        public static List<string> childCards { get; set; }
        public static List<string> mainCards { get; set; }
        public static List<string> concept { get; set; }
        public static List<string> plates { get; set; }
        public static List<string> vNames { get; set; }

        #endregion

        #region Methods
        public static void getData()
        {
            concept = new List<string>(concepts);

            accountType = new List<string>(Properties.Resources.QBAT.Split(new char[] { ',' }));

            jobNames = new List<string>();

            jobNumbers = new List<string>();

            var jobs = new List<string>(Properties.Resources.JOBS.Split(new char[] { ',' }));

            foreach (var job in jobs)

            {
                var num = job.Split(new char[] { '	' });
                jobNumbers.Add(num[0].Trim().ToString());
                jobNames.Add(num[1].Trim().ToString());
            }

            sql = new SqlConnection(cn);
            var cmd = new SqlCommand("SELECT * FROM ChildCards", sql);

            if (cmd.Connection.State != ConnectionState.Open) cmd.Connection.Open();
            using (var reader = cmd.ExecuteReader())
            {
                childCards = new List<string>();
                mainCards = new List<string>();
                while (reader.Read())
                {
                    childCards.Add(reader[0] as string);
                    if (!mainCards.Contains(reader[1] as string)) mainCards.Add(reader[1] as string);
                }
            }

            cmd.CommandText = "SELECT Name FROM Entities ORDER BY Name ASC";
            using (var reader = cmd.ExecuteReader())
            {
                entities = new List<string>();
                while (reader.Read()) entities.Add(reader[0] as string);
            }

            cmd.CommandText = "Select Name FROM Employees ORDER BY Name ASC";
            using (var reader = cmd.ExecuteReader())
            {
                names = new List<string>();
                while (reader.Read()) names.Add(reader[0] as string);
            }
            cmd.CommandText = "Select Class FROM Classes ORDER BY Class ASC";
            using (var reader = cmd.ExecuteReader())
            {
                classes = new List<string>();
                while (reader.Read()) classes.Add(reader[0] as string);
            }
            cmd.CommandText = "SELECT Plate, VName FROM Vehicles";
            using (var reader = cmd.ExecuteReader())
            {
                plates = new List<string>();
                vNames = new List<string>();
                while (reader.Read())
                {
                    if (!string.IsNullOrEmpty(reader[0] as string)) plates.Add(reader[0] as string);
                    if (!string.IsNullOrEmpty(reader[1] as string)) vNames.Add(reader[1] as string);
                }
            }
            cmd.Connection.Close();
        }

        #endregion
    }
}