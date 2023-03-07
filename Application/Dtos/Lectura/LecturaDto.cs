using System;
using System.Collections.Generic;
using Application.Dtos.LecturaRecurso;

namespace Application.Dtos.Lectura
{
    public record LecturaDto
    {
        public Guid LecturaId { get; set; }
        public Guid NovelaRegistrosId { get; set; }
        public Guid UsuarioPropietarioId { get; set; }
        public ICollection<LecturaRecursoDto> Recursos { get; set; }
    }
}