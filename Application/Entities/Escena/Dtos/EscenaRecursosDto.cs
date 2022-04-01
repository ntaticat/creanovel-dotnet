using System;
using System.Collections.Generic;
using Application.Entities.Recurso.Dtos;

namespace Application.Entities.Escena.Dtos
{
    public class EscenaRecursosDto
    {
        public Guid EscenaId { get; set; }
        public string Identificador { get; set; }
        public ICollection<RecursoDto> Recursos { get; set; }
        public Guid NovelaId { get; set; }
    }
}