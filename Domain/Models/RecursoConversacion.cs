using System;

namespace Domain.Models
{
    public class RecursoConversacion: Recurso
    {
        public string Mensaje { get; set; }

        public string AutorMensaje { get; set; }
                
        public Guid? PersonajeSpriteId { get; set; }
        public virtual PersonajeSprite PersonajeSprite { get; set; }

        public Guid? BackgroundSpriteId { get; set; }
        public virtual BackgroundSprite BackgroundSprite { get; set; }

        public Guid? SiguienteRecursoId { get; set; }
        public virtual Recurso SiguienteRecurso { get; set; }
    }
}