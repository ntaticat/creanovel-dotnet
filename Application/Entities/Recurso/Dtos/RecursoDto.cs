using System;

namespace Application.Entities.Recurso.Dtos
{
    public class RecursoDto
    {
        public Guid RecursoId { get; set; }
        public Guid EscenaId { get; set; }
        public string TipoRecurso { get; set; }
        public bool PrimerRecurso { get; set; }
        public bool UltimoRecurso { get; set; }
    }
}