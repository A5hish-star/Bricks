using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Bricks.Models
{
    public class Production
    {
        public int Id { get; set;}
        public string Date {get; set;}
        public string ProductName { get; set;}
        public string Catagory { get; set;}
        public int Quantity { get; set;}
        public string Unit { get; set;}
    }
}