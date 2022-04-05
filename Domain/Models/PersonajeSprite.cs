using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Domain.Models
{
    public class PersonajeSprite
    {
        public Guid PersonajeSpriteId { get; set; }
        public string Nombre { get; set; }
        public string DireccionImagen { get; set; }
        public Guid PersonajeId { get; set; }
        public Personaje Personaje { get; set; }
    }
}