using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.Dtos.PersonajeSprite;

namespace Application.Dtos.Personaje
{
    public record PersonajeDto
    {
        public Guid PersonajeId { get; set; }
        public string Nombre { get; set; }
        public ICollection<PersonajeSpriteDto> Sprites { get; set; }
    }
}