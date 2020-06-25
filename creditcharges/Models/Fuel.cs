using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace creditcharges.Models
{
    class Fuel
    {
        public DateTime Date { get; set; }
        public string Id { get; set; }
        public int Odometer { get; set; }
        public string Plate { get; set; }
        public decimal Gallons { get; set; }
        public string Model { get; set; }
        public decimal Amount { get; set; }
    }
}
