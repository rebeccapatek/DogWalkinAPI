using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DogWalkin.Models
{
    public class Dog
    {
        public int Id { get; set; }
        public String Name { get; set; }
        public String Breed { get; set; }

        public String Notes { get; set; }

        public int OwnerId { get; set; }


    }
}
