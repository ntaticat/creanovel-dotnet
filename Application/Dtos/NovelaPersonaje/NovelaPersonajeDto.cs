using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Dtos.NovelaPersonaje
{
    public record NovelaPersonajeDto
    {
        public Guid NovelaId { get; set; }
        public Guid PersonajeId { get; set; }
    }
}