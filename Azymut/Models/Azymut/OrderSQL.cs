using CsvHelper.Configuration.Attributes;
using System;

// model for saving orders to SQL database - table Orders

namespace Azymut.Models
{
    public class OrderSQL
    {
        public int Id { get; set; }
        public int SynchronizationId { get; set; }
        public int OrderId { get; set; }
        public DateTime Date { get; set; }
        public string IssueId { get; set; }
        public string Code { get; set; }
        public string TypeId { get; set; }
        public string Mail { get; set; }
        public int Status { get; set; }
    }
}
