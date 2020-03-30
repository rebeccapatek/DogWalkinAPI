using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
namespace DogWalkin.Models
{
    public class Walk
    { 
            public int Id { get; set; }
            public System.DateTime Date { get; set; }

            public int Duration { get; set; }
            public string Notes { get; set; }

            public int WalkerId { get; set; }

            public Walker Walker { get; set; }

            public int DogId { get; set; }

            public Dog Dog { get; set; }
        }
    }