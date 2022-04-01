using System;
using System.Collections.Generic;
using Application.Entities.LecturaRecurso.Dtos;

namespace Application.Entities.Lectura.Dtos
{
    public class LecturaDto
    {
        public Guid LecturaId { get; set; }
        public Guid NovelaRegistrosId { get; set; }
        public Guid UsuarioPropietarioId { get; set; }
        public ICollection<LecturaRecursoDto> Recursos { get; set; }
    }
}