using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Domain.Models;

namespace WebAPI.Features.Recursos
{
    public static class RecursoTipos
    {
        public const string Conversacion = "recurso_conversacion";
        public const string Decision = "recurso_decision";
        public const string Entrada = "recurso_entrada";
        public const string Evalua = "recurso_evalua";
        public const string Asigna = "recurso_asigna";
        public const string Explora = "recurso_explora";
        public const string Juega = "recurso_juega";
        public const string Termina = "recurso_termina";

        // Tipos que se crean/editan por los endpoints genéricos (api/recursos) con contenido validado.
        public static readonly string[] Genericos = { Evalua, Asigna, Explora, Juega, Termina };

        // Tipos cuya salida son filas de RecursoDecisionOpcion.
        public static bool TieneOpciones(string tipo) => tipo == Decision || tipo == Evalua || tipo == Explora || tipo == Juega;

        // Tipos cuyas salidas son todas opciones: no usan Recurso.SiguienteRecursoId.
        public static bool SinSiguiente(string tipo) => tipo == Decision || tipo == Explora || tipo == Juega || tipo == Termina;
    }

    public static class RecursoSalidaTipos
    {
        public const string Opcion = "opcion";
        public const string Rama = "rama";
        public const string Zona = "zona";
        public const string Exito = "exito";
        public const string Fallo = "fallo";

        public static readonly string[] Todos = { Opcion, Rama, Zona, Exito, Fallo };
    }

    public static class CondicionModos
    {
        public const string Ocultar = "ocultar";
        public const string Deshabilitar = "deshabilitar";

        public static readonly string[] Todos = { Ocultar, Deshabilitar };
    }

    internal record ConversacionContenido(string Mensaje, string AutorMensaje);
    internal record DecisionContenido(string DecisionMensaje, string AutorDecisionMensaje);
    internal record EntradaContenido(string Etiqueta, string Clave, string Valor, string Placeholder);
    internal record AsignaContenido(JsonElement Efectos);
    internal record ExploraContenido(string Mensaje);
    internal record TerminaContenido(string Final, string Mensaje);
    internal record JuegaContenido(string Mensaje, string VariableResultado, JsonElement Minijuego);

    public record RecursoDto(
        Guid RecursoId, Guid EscenaId, string TipoRecurso, bool PrimerRecurso, bool UltimoRecurso,
        IReadOnlyList<PersonajeEnEscenaDto> Personajes, Guid? BackgroundSpriteId
    );

    public record RecursoConversacionDto(
        Guid RecursoId, Guid EscenaId, string TipoRecurso, bool PrimerRecurso, bool UltimoRecurso,
        IReadOnlyList<PersonajeEnEscenaDto> Personajes, Guid? BackgroundSpriteId,
        string Mensaje, string AutorMensaje, Guid? SiguienteRecursoId
    );

    public record RecursoDecisionDto(
        Guid RecursoId, Guid EscenaId, string TipoRecurso, bool PrimerRecurso, bool UltimoRecurso,
        IReadOnlyList<PersonajeEnEscenaDto> Personajes, Guid? BackgroundSpriteId,
        string DecisionMensaje, string AutorDecisionMensaje, ICollection<RecursoDecisionOpcionDto> Opciones
    );

    public record RecursoDecisionOpcionDto(
        Guid RecursoDecisionOpcionId, string OpcionMensaje, Guid? SiguienteRecursoId, Guid RecursoDecisionId,
        int Orden, string Tipo, JsonElement? Condicion, string CondicionModo, JsonElement? Efectos, JsonElement? Region
    );

    // Evalua: las ramas se evalúan en orden y gana la primera que se cumple; SiguienteRecursoId es el "sino".
    public record RecursoEvaluaDto(
        Guid RecursoId, Guid EscenaId, string TipoRecurso, bool PrimerRecurso, bool UltimoRecurso,
        IReadOnlyList<PersonajeEnEscenaDto> Personajes, Guid? BackgroundSpriteId,
        Guid? SiguienteRecursoId, ICollection<RecursoDecisionOpcionDto> Opciones
    );

    // Explora: fondo con zonas clicables. Cada zona es una opción con `Region`; `Mensaje` es un texto opcional sobre el escenario.
    public record RecursoExploraDto(
        Guid RecursoId, Guid EscenaId, string TipoRecurso, bool PrimerRecurso, bool UltimoRecurso,
        IReadOnlyList<PersonajeEnEscenaDto> Personajes, Guid? BackgroundSpriteId,
        string Mensaje, ICollection<RecursoDecisionOpcionDto> Opciones
    );

    // Juega: un minijuego con dos salidas (opciones `exito` y `fallo`). `VariableResultado` recibe el puntaje si se define.
    public record RecursoJuegaDto(
        Guid RecursoId, Guid EscenaId, string TipoRecurso, bool PrimerRecurso, bool UltimoRecurso,
        IReadOnlyList<PersonajeEnEscenaDto> Personajes, Guid? BackgroundSpriteId,
        string Mensaje, string VariableResultado, JsonElement? Minijuego, ICollection<RecursoDecisionOpcionDto> Opciones
    );

    // Termina: un final de la historia. `Final` es el id de un final del catálogo (Definiciones.finales); `Mensaje` es un texto opcional.
    public record RecursoTerminaDto(
        Guid RecursoId, Guid EscenaId, string TipoRecurso, bool PrimerRecurso, bool UltimoRecurso,
        IReadOnlyList<PersonajeEnEscenaDto> Personajes, Guid? BackgroundSpriteId,
        string Final, string Mensaje
    );

    public record RecursoAsignaDto(
        Guid RecursoId, Guid EscenaId, string TipoRecurso, bool PrimerRecurso, bool UltimoRecurso,
        IReadOnlyList<PersonajeEnEscenaDto> Personajes, Guid? BackgroundSpriteId,
        JsonElement Efectos, Guid? SiguienteRecursoId
    );

    public record RecursoEntradaDto(
        Guid RecursoId, Guid EscenaId, string TipoRecurso, bool PrimerRecurso, bool UltimoRecurso,
        IReadOnlyList<PersonajeEnEscenaDto> Personajes, Guid? BackgroundSpriteId,
        string Etiqueta, string Clave, string Valor, string Placeholder, Guid? SiguienteRecursoId
    );

    // Recurso is a single table with a TipoRecurso discriminator and a free-form Contenido (jsonb)
    // column; this maps each known type to its own DTO shape by deserializing Contenido. Any unknown
    // future TipoRecurso falls back to the common RecursoDto fields only.
    public static class RecursoMapping
    {
        public static object ToDto(Recurso recurso) => recurso.TipoRecurso switch
        {
            RecursoTipos.Conversacion => MapConversacion(recurso),
            RecursoTipos.Decision => MapDecision(recurso),
            RecursoTipos.Entrada => MapEntrada(recurso),
            RecursoTipos.Evalua => MapEvalua(recurso),
            RecursoTipos.Asigna => MapAsigna(recurso),
            RecursoTipos.Explora => MapExplora(recurso),
            RecursoTipos.Juega => MapJuega(recurso),
            RecursoTipos.Termina => MapTermina(recurso),
            _ => new RecursoDto(
                recurso.RecursoId, recurso.EscenaId, recurso.TipoRecurso, recurso.PrimerRecurso, recurso.UltimoRecurso,
                PersonajesEnEscena.Leer(recurso.Personajes), recurso.BackgroundSpriteId
            )
        };

        private static RecursoConversacionDto MapConversacion(Recurso recurso)
        {
            var contenido = JsonSerializer.Deserialize<ConversacionContenido>(recurso.Contenido);

            return new RecursoConversacionDto(
                recurso.RecursoId, recurso.EscenaId, recurso.TipoRecurso, recurso.PrimerRecurso, recurso.UltimoRecurso,
                PersonajesEnEscena.Leer(recurso.Personajes), recurso.BackgroundSpriteId,
                contenido.Mensaje, contenido.AutorMensaje, recurso.SiguienteRecursoId
            );
        }

        private static RecursoDecisionDto MapDecision(Recurso recurso)
        {
            var contenido = JsonSerializer.Deserialize<DecisionContenido>(recurso.Contenido);

            return new RecursoDecisionDto(
                recurso.RecursoId, recurso.EscenaId, recurso.TipoRecurso, recurso.PrimerRecurso, recurso.UltimoRecurso,
                PersonajesEnEscena.Leer(recurso.Personajes), recurso.BackgroundSpriteId,
                contenido.DecisionMensaje, contenido.AutorDecisionMensaje,
                MapOpciones(recurso)
            );
        }

        private static RecursoEvaluaDto MapEvalua(Recurso recurso) => new(
            recurso.RecursoId, recurso.EscenaId, recurso.TipoRecurso, recurso.PrimerRecurso, recurso.UltimoRecurso,
            PersonajesEnEscena.Leer(recurso.Personajes), recurso.BackgroundSpriteId,
            recurso.SiguienteRecursoId, MapOpciones(recurso)
        );

        private static RecursoExploraDto MapExplora(Recurso recurso)
        {
            var contenido = JsonSerializer.Deserialize<ExploraContenido>(recurso.Contenido);

            return new RecursoExploraDto(
                recurso.RecursoId, recurso.EscenaId, recurso.TipoRecurso, recurso.PrimerRecurso, recurso.UltimoRecurso,
                PersonajesEnEscena.Leer(recurso.Personajes), recurso.BackgroundSpriteId,
                contenido?.Mensaje ?? "", MapOpciones(recurso)
            );
        }

        private static RecursoTerminaDto MapTermina(Recurso recurso)
        {
            var contenido = JsonSerializer.Deserialize<TerminaContenido>(recurso.Contenido);

            return new RecursoTerminaDto(
                recurso.RecursoId, recurso.EscenaId, recurso.TipoRecurso, recurso.PrimerRecurso, recurso.UltimoRecurso,
                PersonajesEnEscena.Leer(recurso.Personajes), recurso.BackgroundSpriteId,
                contenido?.Final ?? "", contenido?.Mensaje ?? ""
            );
        }

        private static RecursoJuegaDto MapJuega(Recurso recurso)
        {
            var contenido = JsonSerializer.Deserialize<JuegaContenido>(recurso.Contenido);

            return new RecursoJuegaDto(
                recurso.RecursoId, recurso.EscenaId, recurso.TipoRecurso, recurso.PrimerRecurso, recurso.UltimoRecurso,
                PersonajesEnEscena.Leer(recurso.Personajes), recurso.BackgroundSpriteId,
                contenido?.Mensaje ?? "", contenido?.VariableResultado,
                // Un contenido vacío o dañado no debe romper la carga de toda la versión: sin configuración, null.
                contenido != null && contenido.Minijuego.ValueKind == JsonValueKind.Object ? contenido.Minijuego : null,
                MapOpciones(recurso)
            );
        }

        private static RecursoAsignaDto MapAsigna(Recurso recurso)
        {
            var contenido = JsonSerializer.Deserialize<AsignaContenido>(recurso.Contenido);
            var efectos = contenido != null && contenido.Efectos.ValueKind == JsonValueKind.Array
                ? contenido.Efectos
                : JsonSerializer.SerializeToElement(Array.Empty<object>());

            return new RecursoAsignaDto(
                recurso.RecursoId, recurso.EscenaId, recurso.TipoRecurso, recurso.PrimerRecurso, recurso.UltimoRecurso,
                PersonajesEnEscena.Leer(recurso.Personajes), recurso.BackgroundSpriteId,
                efectos, recurso.SiguienteRecursoId
            );
        }

        private static List<RecursoDecisionOpcionDto> MapOpciones(Recurso recurso) =>
            (recurso.Opciones ?? new List<RecursoDecisionOpcion>())
                .OrderBy(o => o.Orden)
                .Select(MapOpcion)
                .ToList();

        public static RecursoDecisionOpcionDto MapOpcion(RecursoDecisionOpcion o) => new(
            o.RecursoDecisionOpcionId, o.OpcionMensaje, o.SiguienteRecursoId, o.RecursoDecisionId,
            o.Orden, o.Tipo, Application.Motor.MotorJson.ParseNullable(o.Condicion), o.CondicionModo,
            Application.Motor.MotorJson.ParseNullable(o.Efectos), Application.Motor.MotorJson.ParseNullable(o.Region)
        );

        private static RecursoEntradaDto MapEntrada(Recurso recurso)
        {
            var contenido = JsonSerializer.Deserialize<EntradaContenido>(recurso.Contenido);

            return new RecursoEntradaDto(
                recurso.RecursoId, recurso.EscenaId, recurso.TipoRecurso, recurso.PrimerRecurso, recurso.UltimoRecurso,
                PersonajesEnEscena.Leer(recurso.Personajes), recurso.BackgroundSpriteId,
                contenido.Etiqueta, contenido.Clave, contenido.Valor, contenido.Placeholder, recurso.SiguienteRecursoId
            );
        }
    }
}
