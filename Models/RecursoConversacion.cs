using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace CreaNovelNETCore.Models
{
    public class RecursoConversacion: Recurso
    {
        public string Mensaje { get; set; }
        
        public Guid? SiguienteRecursoId { get; set; }
        public virtual Recurso SiguienteRecurso { get; set; }
    }
}