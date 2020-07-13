using DevExpress.Data.Async.Helpers;
using Dropbox.Api.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace creditcharges.Models
{
    class Fuel
    {
        public DateTime Date { get; set; }
        public string Id { get; set; }
        public int Odometer { get; set; }
        public decimal Gallons { get; set; }
        public decimal Amount { get; set; }
        public decimal PPG { get; set; }
        public decimal MPG { get; set; }

    }
}
