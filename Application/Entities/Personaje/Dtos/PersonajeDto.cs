using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.Entities.PersonajeSprite.Dtos;

namespace Application.Entities.Personaje.Dtos
{
    public class PersonajeDto
    {
        public Guid PersonajeId { get; set; }
        public string Nombre { get; set; }
        public ICollection<PersonajeSpriteDto> Sprites { get; set; }
    }
}