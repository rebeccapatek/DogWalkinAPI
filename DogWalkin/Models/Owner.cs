using System;
using System.Collections.Generic;
using System.Text;

namespace DogWalkers.Models
{
    class Owner
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public int NeighborhoodId { get; set; }
        public string Address { get; set; }
        public string Phone { get; set; }

        public Neighborhood Neighborhood { get; set; }
    }
}
