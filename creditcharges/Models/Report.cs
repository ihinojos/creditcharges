using System;

namespace creditcharges.Models
{
    public class Report
    {
        public DateTime T_date { get; set; }
        public string Card { get; set; }
        public string Location { get; set; }
        public string Concept { get; set; }
        public decimal Amount { get; set; }
        public string Id { get; set; }
    }
}