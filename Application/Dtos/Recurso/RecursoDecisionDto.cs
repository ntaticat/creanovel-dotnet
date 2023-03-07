using System;
using System.Collections.Generic;

namespace Application.Dtos.Recurso
{
    public record RecursoDecisionDto: RecursoDto
    {
        public string DecisionMensaje { get; set; }
        public ICollection<RecursoDecisionOpcionDto> Opciones { get; set; }
    }
}