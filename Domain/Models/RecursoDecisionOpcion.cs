using System;

namespace Domain.Models;

public class RecursoDecisionOpcion
{
    public Guid RecursoDecisionOpcionId { get; set; }
    public string OpcionMensaje { get; set; }
    public Guid? SiguienteRecursoId { get; set; }
    public virtual Recurso SiguienteRecurso { get; set; }
    public Guid RecursoDecisionId { get; set; }
    public RecursoDecision RecursoDecision { get; set; }
}