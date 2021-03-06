﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Text;

namespace DogWalkin.Models
{
    public class Walker
    {
        public int Id { get; set; }
        [Required]
        [StringLength(40, MinimumLength = 2)]
        public string Name { get; set; }
        [Required]

        public int NeighborhoodId { get; set; }

        public Neighborhood Neighborhood { get; set; }
       
        public List<Walk> Walks { get; set; }

    }
}