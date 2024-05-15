using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Bricks.Models
{
    public class Sales
    {
        public int Id { get; set;}
        public string Date { get; set;}
        public string BuyerName { get; set;}
        public string ProductName { get; set;}
        public string Catagory { get; set;}
        public int AmountOfBricks { get; set;}
        public string Unit {get; set;}
        public int PriceperBrick { get; set;}
        public int Price { get; set;}
        public int AmountPaid { get; set;}
        public int AmountRemaining { get; set;}
        public string ModeOfPayment { get; set;}
        public string Status { get; set;}
    }
}