using System;

namespace Domain.Models;

// Una "salida" de un recurso con varias ramas: opción de un Selecciona o rama de un Evalua.
public class RecursoDecisionOpcion
{
    public Guid RecursoDecisionOpcionId { get; set; }
    public string OpcionMensaje { get; set; }
    public Guid? SiguienteRecursoId { get; set; }
    public virtual Recurso SiguienteRecurso { get; set; }
    public Guid RecursoDecisionId { get; set; }
    public Recurso RecursoDecision { get; set; }

    // Orden de presentación (Selecciona) o de evaluación (Evalua: gana la primera rama que se cumple).
    public int Orden { get; set; }
    // "opcion" (Selecciona) | "rama" (Evalúa) | "zona" (Explora)
    public string Tipo { get; set; } = "opcion";
    // AST JSON de la condición (null = siempre). Ver Application/Motor/MotorValidador.
    public string Condicion { get; set; }
    // Qué hace una opción de Selecciona cuando su condición falla: "ocultar" | "deshabilitar".
    public string CondicionModo { get; set; } = "ocultar";
    // Lista JSON de efectos que se aplican al elegir la opción (null = ninguno).
    public string Efectos { get; set; }
    // Solo en zonas de un Explora: rectángulo clicable en % del escenario, `{ x, y, ancho, alto }`.
    public string Region { get; set; }
}
