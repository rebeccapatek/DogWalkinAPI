using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace DogWalkin.Models
{
    public class Dog
    {
        public int Id { get; set; }
        [Required]
        [StringLength(40, MinimumLength = 2)]
        public String Name { get; set; }
        public String Breed { get; set; }

        public String Notes { get; set; }
        [Required]

        public int OwnerId { get; set; }
        public Owner Owner { get; set; }


    }
}
