using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;

namespace creditcharges.Models
{
    internal class Data
    {
        private static SqlConnection sql;
        private static string[] concepts = {"Alojamiento","Atención Médica", "Gasolina/Automóvil", "Internet", "Mercancía", "Otra Opción",
            "Otros Servicios", "Pago/crédito", "Seguros", "Servicios Profesionales", "Servicios Púbilcos", "Teléfono/Cable"};

        //public static string cn = @"Server=INTLGXCUU24\INTELOGIX;Initial Catalog=TESTTRANS;MultipleActiveResultSets=true;Persist Security Info=True;User ID=Intelogix;Password=Intelogix20XX!";
        public static string cn = @"Server=INTLGXCUU24\INTELOGIX;Initial Catalog=PRODTRANS;MultipleActiveResultSets=true;Persist Security Info=True;User ID=Intelogix;Password=Intelogix20XX!";

        //more data bout cards
            // number
            // distributor 
            // account 

        //more data about employees
            // name
            // dept
            // avg compras (implemented outside database) 

        //company data (David needs to proide further information).
            // name 

        //quickbook settings dependant on the entity selected.
            //will have to check around which one goes where

        //performance of trucks 
            // (currrent miles - last miles) / gallons = mpg

        public static List<string> accountType { get; set; }
        public static List<string> entities { get; set; }
        public static List<string> jobNames { get; set; }
        public static List<string> jobNumbers { get; set; }
        public static List<string> classes { get; set; }
        public static List<string> names { get; set; }
        public static List<string> childCards { get; set; }
        public static List<string> mainCards { get; set; }
        public static List<string> concept { get; set; }
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

            classes = new List<string>(Properties.Resources.CLASS.Split(new char[] { ',' }));

            

            sql = new SqlConnection(cn);
            var cmd = new SqlCommand("SELECT * FROM ChildCards", sql);

            cmd.Connection.Open();
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


            cmd.CommandText = "SELECT Name FROM Entities";
            using (var reader = cmd.ExecuteReader())
            {
                entities = new List<string>();
                while (reader.Read()) entities.Add(reader[0] as string);
            }

            cmd.CommandText = "Select Name FROM Employees";
            using (var reader = cmd.ExecuteReader())
            {
                names = new List<string>();
                while (reader.Read()) names.Add(reader[0] as string);
            }
            
            cmd.Connection.Close();
        }


        //To save daata about cards i need the following

        //Card Number **** **** **** 5656
        //Date of Expiration MM/YY
        //Person assigned to card 
        //Main Card
        //Bank of the Card
        //Username associated with them card
    }
}