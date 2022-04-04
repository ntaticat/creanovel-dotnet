using System;

namespace Domain.Models
{
    public class LecturaRecursos
    {
        public Guid LecturaId { get; set; }
        public Guid RecursoId { get; set; }
        public int RecursoOrder { get; set; }

        public Lectura Lectura { get; set; }
        public Recurso Recurso { get; set; }
    }
}