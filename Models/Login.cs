using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Bricks.Models
{
    public class Login
    {
        public int Id{get; set;}
        [Required]
        [RegularExpression(@"^[A-Z]+[a-zA-Z\s]*$")]
        public string Name {get; set;}
        [Required]
        public string Password {get; set;}
    }
}