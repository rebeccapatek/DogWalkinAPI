using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using Xunit;

namespace DogWalkin.Models
{
    public class Owner
    {
        public int Id { get; set; }
        [Required]
        [StringLength(40, MinimumLength = 2)]
        
        public string Name { get; set; }
        [Required]

        public int NeighborhoodId { get; set; }
        [Required]
        public string Address { get; set; }
        [RegularExpression("^[0-9]*$", ErrorMessage = "Phone number must conatin digits")]
        public string Phone { get; set; }

        public Neighborhood Neighborhood { get; set; }

    }
}
