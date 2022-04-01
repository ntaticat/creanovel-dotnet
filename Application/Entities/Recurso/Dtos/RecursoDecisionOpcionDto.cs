using System;

namespace Application.Entities.Recurso.Dtos
{
    public class RecursoDecisionOpcionDto
    {
        public Guid RecursoDecisionOpcionId { get; set; }
        public string OpcionMensaje { get; set; }
        public Guid? SiguienteRecursoId { get; set; }
        public Guid RecursoDecisionId { get; set; }
    }
}