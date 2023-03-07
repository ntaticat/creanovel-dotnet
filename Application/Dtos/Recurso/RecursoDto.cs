using System;

namespace Application.Dtos.Recurso
{
    public record RecursoDto
    {
        public Guid RecursoId { get; set; }
        public Guid EscenaId { get; set; }
        public string TipoRecurso { get; set; }
        public bool PrimerRecurso { get; set; }
        public bool UltimoRecurso { get; set; }
    }
}