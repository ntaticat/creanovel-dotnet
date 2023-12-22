using System;
using System.Collections.Generic;
using Application.Dtos.Recurso;

namespace Application.Dtos.Escena
{
    public record EscenaRecursosDto
    {
        public Guid EscenaId { get; set; }
        public string Identificador { get; set; }
        public ICollection<RecursoDto> Recursos { get; set; }
        public Guid NovelaVersionId { get; set; }
        public bool PrimerEscena { get; set; }
        public bool UltimaEscena { get; set; }
    }
}