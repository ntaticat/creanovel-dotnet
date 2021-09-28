using System;

namespace CreaNovelNETCore.DTOs.Recursos
{
    public class RecursoDecisionOpcionDTO
    {
        public Guid RecursoDecisionOpcionId { get; set; }
        public string OpcionMensaje { get; set; }
        public Guid? SiguienteRecursoId { get; set; }
        public Guid RecursoDecisionId { get; set; }
    }
}