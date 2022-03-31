using System;

namespace WebAPI.DTOs.Recursos
{
    public class RecursoConversacionDTO: RecursoDTO
    {
        public string Mensaje { get; set; }
        public Guid? SiguienteRecursoId { get; set; }
        public bool PrimerRecurso { get; set; }
        public bool UltimoRecurso { get; set; }
    }
}