using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Entities.NovelaPersonaje.Dtos
{
    public class NovelaPersonajeDto
    {
        public Guid NovelaId { get; set; }
        public Guid PersonajeId { get; set; }
    }
}