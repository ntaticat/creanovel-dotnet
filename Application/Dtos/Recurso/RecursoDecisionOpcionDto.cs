using System;

namespace Application.Dtos.Recurso
{
    public record RecursoDecisionOpcionDto
    {
        public Guid RecursoDecisionOpcionId { get; set; }
        public string OpcionMensaje { get; set; }
        public Guid? SiguienteRecursoId { get; set; }
        public Guid RecursoDecisionId { get; set; }
    }
}