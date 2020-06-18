using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;

namespace creditcharges.Models
{
    internal class Data
    {
        private static SqlConnection sql;

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
        public static void getData()
        {
            accountType = new List<string>(Properties.Resources.QBAT.Split(new char[] { ',' }));
            entities = new List<string>(Properties.Resources.ENTITIES.Split(new char[] { ',' }));
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
            names = File.ReadAllLines("Resources\\names.txt").ToList();
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
                cmd.Connection.Close();
                sql.Dispose();
            }
        }
    }
}