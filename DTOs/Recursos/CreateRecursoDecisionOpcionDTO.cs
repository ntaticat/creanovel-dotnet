using System;

namespace CreaNovelNETCore.DTOs.Recursos
{
    public class CreateRecursoDecisionOpcionDTO
    {
        public string OpcionMensaje { get; set; }
        public Guid? SiguienteRecursoId { get; set; }
        public Guid RecursoDecisionId { get; set; }
    }
}