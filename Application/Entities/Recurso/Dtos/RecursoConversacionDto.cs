using System;

namespace Application.Entities.Recurso.Dtos
{
    public class RecursoConversacionDto: RecursoDto
    {
        public string Mensaje { get; set; }
        public Guid? SiguienteRecursoId { get; set; }
    }
}