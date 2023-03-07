using System;

namespace Application.Dtos.LecturaRecurso
{
    public record LecturaRecursoDto
    {
        public Guid LecturaId { get; set; }
        public Guid RecursoId { get; set; }
        public int RecursoOrder { get; set; }
    }
}