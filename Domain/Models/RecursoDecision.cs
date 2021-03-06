using System;
using System.Collections.Generic;

namespace Domain.Models
{
    public class RecursoDecision: Recurso
    {
        public string DecisionMensaje { get; set; }

        public Guid? PersonajeSpriteId { get; set; }
        public virtual PersonajeSprite PersonajeSprite { get; set; }

        public Guid? BackgroundSpriteId { get; set; }
        public virtual BackgroundSprite BackgroundSprite { get; set; }

        public ICollection<RecursoDecisionOpcion> Opciones { get; set; }
    }

    public class RecursoDecisionOpcion
    {
        public Guid RecursoDecisionOpcionId { get; set; }
        public string OpcionMensaje { get; set; }
        public Guid? SiguienteRecursoId { get; set; }
        public virtual Recurso SiguienteRecurso { get; set; }
        public Guid RecursoDecisionId { get; set; }
        public RecursoDecision RecursoDecision { get; set; }
    }
}