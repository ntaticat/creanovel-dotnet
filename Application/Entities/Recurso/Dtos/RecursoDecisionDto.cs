using System;
using System.Collections.Generic;

namespace Application.Entities.Recurso.Dtos
{
    public class RecursoDecisionDto: RecursoDto
    {
        public string DecisionMensaje { get; set; }
        public ICollection<RecursoDecisionOpcionDto> Opciones { get; set; }
    }
}