using System;

namespace Domain
{
    public class LecturaRecursos
    {
        public Guid LecturaId { get; set; }
        public Guid RecursoId { get; set; }
        public Lectura Lectura { get; set; }
        public Recurso Recurso { get; set; }
    }
}