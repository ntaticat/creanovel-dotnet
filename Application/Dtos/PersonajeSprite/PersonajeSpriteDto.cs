using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Dtos.PersonajeSprite
{
    public record PersonajeSpriteDto
    {
        public Guid PersonajeSpriteId { get; set; }
        public string Nombre { get; set; }
        public string DireccionImagen { get; set; }
    }
}