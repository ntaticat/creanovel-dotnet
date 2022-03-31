using System;
using System.Collections.Generic;

namespace WebAPI.DTOs.Recursos
{
    public class RecursoDecisionDTO: RecursoDTO
    {
        public string DecisionMensaje { get; set; }
        public ICollection<RecursoDecisionOpcionDTO> Opciones { get; set; }
        public bool PrimerRecurso { get; set; }
        public bool UltimoRecurso { get; set; }
    }
}