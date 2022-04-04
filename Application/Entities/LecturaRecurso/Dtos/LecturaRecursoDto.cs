using System;

namespace Application.Entities.LecturaRecurso.Dtos
{
    public class LecturaRecursoDto
    {
        public Guid LecturaId { get; set; }
        public Guid RecursoId { get; set; }
        public int RecursoOrder { get; set; }
    }
}