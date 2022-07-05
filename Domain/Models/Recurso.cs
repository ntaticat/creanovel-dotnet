using System;
using System.Collections.Generic;

namespace Domain.Models
{
    public abstract class Recurso
    {
        public Guid RecursoId { get; set; }
        
        public Guid EscenaId { get; set; }
        public Escena Escena { get; set; }

        public bool PrimerRecurso { get; set; }
        public bool UltimoRecurso { get; set; }
        public string TipoRecurso { get; set; }

        public ICollection<LecturaRecursos> Lecturas { get; set; }
    }
}