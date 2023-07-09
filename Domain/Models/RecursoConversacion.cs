using System;

namespace Domain.Models
{
    public class RecursoConversacion: Recurso
    {
        public string Mensaje { get; set; }

        public string AutorMensaje { get; set; }

        public Guid? SiguienteRecursoId { get; set; }
        public virtual Recurso SiguienteRecurso { get; set; }
    }
}