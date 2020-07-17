using DevExpress.Data.TreeList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace creditcharges.Models
{
    public class Vehicle
    {
        #region Attributes
        public string Id { get; set; }
        public string Plate { get; set; }
        public string VName { get; set; }
        public string Model { get; set; }
        public string VType { get; set; }
        public decimal AvgMPG { get; set; }
        #endregion
    }
}
