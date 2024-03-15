using CsvHelper.Configuration.Attributes;
using System;

// model to save synchronization parameters - date of last synchronization and updated items between each synchronization stage

namespace Azymut.Models
{
    public class DatabaseSync
    {
        public DateTime Date {  get; set; }
        public int ItemsChanged {  get; set; }
    }
}
