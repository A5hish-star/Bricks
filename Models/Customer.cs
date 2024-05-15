using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Bricks.Models
{
    public class Customer
    {
        public int Id { get; set;}
        public string CustomerName { get; set;}
        public int MobileNo { get; set;}
        public string Email { get; set;}
        public string ShippingAddress { get; set;}
        public string PAN { get; set;}
    }
}