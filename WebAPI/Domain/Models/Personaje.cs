using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Domain.Models
{
    public class Personaje
    {
        public Guid PersonajeId { get; set; }
        public string Nombre { get; set; }
        public ICollection<PersonajeSprite> Sprites { get; set; }
        public ICollection<NovelaPersonaje> Novelas { get; set; }
    }
}