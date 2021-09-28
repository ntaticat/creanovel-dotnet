using System;

namespace CreaNovelNETCore.Models
{
    public class LecturaRecursos
    {
        public Guid LecturaId { get; set; }
        public Guid RecursoId { get; set; }
        public Lectura Lectura { get; set; }
        public Recurso Recurso { get; set; }
    }
}