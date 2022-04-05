using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Domain.Models
{
    public class NovelaBackground
    {
        public Guid NovelaId { get; set; }
        public Guid BackgroundId { get; set; }
        public Novela Novela { get; set; }
        public Background Background { get; set; }
    }
}