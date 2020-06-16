using System;
using System.ComponentModel.DataAnnotations;

namespace creditcharges.Models
{
    public class Transaction
    {
        [System.ComponentModel.DataAnnotations.Key, Display(AutoGenerateField = false)]
        public string Id { get; set; }

        [Required]
        public string Employee { get; set; }

        [Required]
        public string Card { get; set; }

        public string Location { get; set; }
        public string Concept { get; set; }
        public decimal Amount { get; set; }
        public DateTime Date { get; set; }
        public string Notes { get; set; }
        public string MainCard { get; set; }
        public string Account { get; set; }
        public string Entity { get; set; }
        public string Class { get; set; }
        public string JobNumber { get; set; }
        public string JobName { get; set; }
        public string Author { get; set; }
    }
}