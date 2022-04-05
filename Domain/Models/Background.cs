using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Domain.Models
{
    public class Background
    {
        public Guid BackgroundId { get; set; }
        public string Descripcion { get; set; }
        public ICollection<NovelaBackground> Novelas { get; set; }
    }
}