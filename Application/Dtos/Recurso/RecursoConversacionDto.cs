using System;

namespace Application.Dtos.Recurso
{
    public record RecursoConversacionDto: RecursoDto
    {
        public string Mensaje { get; set; }
        public Guid? SiguienteRecursoId { get; set; }
    }
}