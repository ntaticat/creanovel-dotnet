using System;

namespace CreaNovelNETCore.DTOs.Recursos
{
    public class CreateRecursoDTO
    {
        public Guid EscenaId { get; set; }
        public string TipoRecurso { get; set; }
        public string Mensaje { get; set; }
        public Guid? SiguienteRecursoId { get; set; }
        public string DecisionMensaje { get; set; }
        public bool PrimerRecurso { get; set; }
        public bool UltimoRecurso { get; set; }
    }
}