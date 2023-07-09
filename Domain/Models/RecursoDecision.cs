using System.Collections.Generic;

namespace Domain.Models
{
    public class RecursoDecision: Recurso
    {
        public string DecisionMensaje { get; set; }
        public string AutorDecisionMensaje { get; set; }

        public ICollection<RecursoDecisionOpcion> Opciones { get; set; }
    }
}