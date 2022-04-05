using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Domain.Models
{
    public class NovelaPersonaje
    {
        public Guid NovelaId { get; set; }
        public Guid PersonajeId { get; set; }
        public Novela Novela { get; set; }
        public Personaje Personaje { get; set; }
    }
}