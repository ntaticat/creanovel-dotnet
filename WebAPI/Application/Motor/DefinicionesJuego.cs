using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Application.Motor
{
    public static class VariableTipos
    {
        public const string Numero = "numero";
        public const string Booleano = "booleano";
        public const string Texto = "texto";

        public static readonly string[] Todos = { Numero, Booleano, Texto };
    }

    public static class VariableHud
    {
        public const string Oculto = "oculto";
        public const string Numero = "numero";
        public const string Barra = "barra";

        public static readonly string[] Todos = { Oculto, Numero, Barra };
    }

    // Una variable del estado de juego (salud, afecto, tiene_pareja, tiempo...). `Clave` es el
    // identificador estable que usan condiciones y efectos.
    public record VariableDef(
        string Clave,
        string Etiqueta,
        string Tipo,
        JsonElement? Inicial,
        double? Min,
        double? Max,
        List<string> Valores,
        string Hud
    );

    // Un objeto del inventario. `Id` es el identificador estable que citan condiciones y efectos.
    // Un objeto no apilable se tiene o no se tiene (cantidad 0 o 1); uno apilable admite `Max` (sin tope si es null).
    public record ObjetoDef(
        string Id,
        string Nombre,
        string Descripcion,
        string ImagenUrl,
        bool Apilable,
        int? Max,
        int Inicial,
        UsoObjetoDef Uso = null
    );

    // Lo que ocurre cuando el jugador usa un objeto desde la mochila: aplica `Efectos` (mismo formato que los de una opción),
    // gasta una unidad si `Consumir` y, si hay `DestinoRecursoId`, lleva la historia a ese nodo. `Condicion` decide cuándo se puede usar
    // (p. ej. solo en cierta ubicación); sin ella siempre. Es la única referencia a un nodo que vive dentro del jsonb: `CreateDraftFromVersion`
    // la remapea y al borrar el nodo se limpia (ver `UsoObjetoDestinos`).
    public record UsoObjetoDef(
        string Etiqueta,
        JsonElement? Condicion,
        JsonElement? Efectos,
        bool Consumir,
        Guid? DestinoRecursoId
    );

    // Un lugar del mundo. Si tiene fondo, un Explora sin fondo propio lo usa mientras el jugador está allí.
    public record UbicacionDef(string Id, string Nombre, Guid? BackgroundSpriteId);

    // Un logro (se desbloquea con un efecto) o un final (lo registra un nodo Termina). Solo cambian de significado:
    // el progreso de ambos vive en el estado de la partida y sobrevive a "Empezar de nuevo".
    public record MetaDef(string Id, string Nombre, string Descripcion);

    public record DefinicionesJuego(
        List<VariableDef> Variables,
        List<ObjetoDef> Objetos = null,
        List<UbicacionDef> Ubicaciones = null,
        string UbicacionInicial = null,
        List<MetaDef> Logros = null,
        List<MetaDef> Finales = null
    );

    // Lo que las condiciones y efectos pueden citar: variables, objetos y ubicaciones definidos en la versión.
    public record ReglasContexto(
        IReadOnlyDictionary<string, VariableDef> Variables,
        IReadOnlyDictionary<string, ObjetoDef> Objetos,
        IReadOnlySet<string> Ubicaciones,
        IReadOnlySet<string> Logros,
        IReadOnlySet<string> Finales
    )
    {
        public static ReglasContexto Desde(DefinicionesJuego defs) => new(
            MotorJson.PorClave(defs),
            (defs.Objetos ?? new List<ObjetoDef>()).Where(o => !string.IsNullOrEmpty(o.Id)).GroupBy(o => o.Id).ToDictionary(g => g.Key, g => g.First()),
            (defs.Ubicaciones ?? new List<UbicacionDef>()).Where(u => !string.IsNullOrEmpty(u.Id)).Select(u => u.Id).ToHashSet(),
            (defs.Logros ?? new List<MetaDef>()).Where(m => !string.IsNullOrEmpty(m.Id)).Select(m => m.Id).ToHashSet(),
            (defs.Finales ?? new List<MetaDef>()).Where(m => !string.IsNullOrEmpty(m.Id)).Select(m => m.Id).ToHashSet()
        );

        public static ReglasContexto SoloVariables(IReadOnlyDictionary<string, VariableDef> variables) =>
            new(variables, new Dictionary<string, ObjetoDef>(), new HashSet<string>(), new HashSet<string>(), new HashSet<string>());
    }

    public static class MotorJson
    {
        public static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web);

        public static readonly Regex ClaveRegex = new("^[a-z][a-z0-9_]{0,39}$", RegexOptions.Compiled);

        public const int MaxVariables = 100;
        public const int MaxValoresTexto = 30;
        public const int MaxObjetos = 100;
        public const int MaxUbicaciones = 50;
        public const int MaxCantidad = 9999;

        // Las imágenes de objetos solo pueden ser archivos subidos por la propia plataforma.
        public static readonly Regex ImagenObjetoRegex = new("^/uploads/objetos/[A-Za-z0-9._-]+$", RegexOptions.Compiled);

        public const int MaxMetas = 50;
        public const int MaxEtiquetaUso = 30;
        public const string EtiquetaUsoPorDefecto = "Usar";

        // Palabras con significado en la sintaxis de texto (condiciones, efectos e interpolación): un id no puede llamarse así
        // o `no > 3` o `{si}` serían ambiguos. `a` no está: en `ir a patio` se distingue por el contexto.
        public static readonly HashSet<string> PalabrasReservadas = new()
        {
            "y", "o", "no", "en", "tengo", "objeto", "verdadero", "falso", "alternar", "avanzar",
            "dar", "quitar", "ir", "logro", "final", "si", "ubicacion"
        };

        public static DefinicionesJuego Vacias() => new(new List<VariableDef>(), new List<ObjetoDef>(), new List<UbicacionDef>(), null, new List<MetaDef>(), new List<MetaDef>());

        // Lee el JSON guardado; ante datos corruptos o vacíos devuelve definiciones vacías.
        public static DefinicionesJuego LeerDefiniciones(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                return Vacias();
            }

            try
            {
                var defs = JsonSerializer.Deserialize<DefinicionesJuego>(json, Options);

                // Las versiones anteriores a los objetos y ubicaciones solo traen `variables`.
                return defs == null
                    ? Vacias()
                    : new DefinicionesJuego(
                        defs.Variables ?? new List<VariableDef>(),
                        defs.Objetos ?? new List<ObjetoDef>(),
                        defs.Ubicaciones ?? new List<UbicacionDef>(),
                        defs.UbicacionInicial,
                        defs.Logros ?? new List<MetaDef>(),
                        defs.Finales ?? new List<MetaDef>());
            }
            catch (JsonException)
            {
                return Vacias();
            }
        }

        public static string Serializar(DefinicionesJuego defs) => JsonSerializer.Serialize(defs, Options);

        public static Dictionary<string, VariableDef> PorClave(DefinicionesJuego defs) =>
            defs.Variables.Where(v => !string.IsNullOrEmpty(v.Clave))
                .GroupBy(v => v.Clave)
                .ToDictionary(g => g.Key, g => g.First());

        public static JsonElement? ParseNullable(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                return null;
            }

            using var doc = JsonDocument.Parse(json);
            return doc.RootElement.Clone();
        }

        // Rellena los valores omitidos por el cliente y devuelve la lista de errores de validación.
        public static (DefinicionesJuego Normalizadas, List<string> Errores) NormalizarYValidar(DefinicionesJuego entrada)
        {
            var errores = new List<string>();
            var variables = entrada?.Variables ?? new List<VariableDef>();

            if (variables.Count > MaxVariables)
            {
                errores.Add($"Se permiten como máximo {MaxVariables} variables");
            }

            var normalizadas = new List<VariableDef>();
            var vistas = new HashSet<string>();

            foreach (var v in variables)
            {
                var clave = v.Clave?.Trim();

                if (string.IsNullOrEmpty(clave) || !ClaveRegex.IsMatch(clave))
                {
                    errores.Add($"Clave de variable inválida '{v.Clave}': usa minúsculas, números y guion bajo, empezando con letra (máx. 40)");
                    continue;
                }

                if (PalabrasReservadas.Contains(clave))
                {
                    errores.Add($"'{clave}' es una palabra reservada de la sintaxis de texto; elige otra clave");
                    continue;
                }

                if (!vistas.Add(clave))
                {
                    errores.Add($"La variable '{clave}' está repetida");
                    continue;
                }

                if (!VariableTipos.Todos.Contains(v.Tipo))
                {
                    errores.Add($"Variable '{clave}': tipo inválido '{v.Tipo}'");
                    continue;
                }

                var etiqueta = string.IsNullOrWhiteSpace(v.Etiqueta) ? clave : v.Etiqueta.Trim();

                if (etiqueta.Length > 60)
                {
                    errores.Add($"Variable '{clave}': la etiqueta supera los 60 caracteres");
                }

                var hud = string.IsNullOrEmpty(v.Hud) ? VariableHud.Oculto : v.Hud;

                if (!VariableHud.Todos.Contains(hud))
                {
                    errores.Add($"Variable '{clave}': hud inválido '{v.Hud}'");
                    continue;
                }

                var valores = v.Tipo == VariableTipos.Texto
                    ? (v.Valores ?? new List<string>()).Select(x => x?.Trim()).Where(x => !string.IsNullOrEmpty(x)).Distinct().ToList()
                    : new List<string>();

                if (valores.Count > MaxValoresTexto)
                {
                    errores.Add($"Variable '{clave}': se permiten como máximo {MaxValoresTexto} valores");
                }

                double? min = v.Tipo == VariableTipos.Numero ? v.Min : null;
                double? max = v.Tipo == VariableTipos.Numero ? v.Max : null;

                if (min.HasValue && max.HasValue && min > max)
                {
                    errores.Add($"Variable '{clave}': el mínimo es mayor que el máximo");
                }

                if (hud == VariableHud.Barra && (v.Tipo != VariableTipos.Numero || !min.HasValue || !max.HasValue))
                {
                    errores.Add($"Variable '{clave}': la barra requiere una variable numérica con mínimo y máximo");
                }

                var inicial = NormalizarInicial(v, clave, min, max, valores, errores);

                normalizadas.Add(new VariableDef(clave, etiqueta, v.Tipo, inicial, min, max, valores, hud));
            }

            var objetos = NormalizarObjetos(entrada?.Objetos, errores);
            var ubicaciones = NormalizarUbicaciones(entrada?.Ubicaciones, errores);

            var ubicacionInicial = string.IsNullOrWhiteSpace(entrada?.UbicacionInicial) ? null : entrada.UbicacionInicial.Trim();
            if (ubicacionInicial != null && !ubicaciones.Any(u => u.Id == ubicacionInicial))
            {
                errores.Add($"La ubicación inicial '{ubicacionInicial}' no está definida");
                ubicacionInicial = null;
            }

            var logros = NormalizarMetas(entrada?.Logros, "logro", errores);
            var finales = NormalizarMetas(entrada?.Finales, "final", errores);

            var resultado = new DefinicionesJuego(normalizadas, objetos, ubicaciones, ubicacionInicial, logros, finales);

            // Los usos citan variables, objetos, ubicaciones y logros del propio catálogo: solo se pueden comprobar cuando ya está completo.
            ValidarUsos(objetos, ReglasContexto.Desde(resultado), errores);

            return (resultado, errores);
        }

        private static List<MetaDef> NormalizarMetas(List<MetaDef> entrada, string tipo, List<string> errores)
        {
            var metas = entrada ?? new List<MetaDef>();

            if (metas.Count > MaxMetas)
            {
                errores.Add($"Se permiten como máximo {MaxMetas} {tipo}s");
            }

            var resultado = new List<MetaDef>();
            var vistos = new HashSet<string>();

            foreach (var m in metas)
            {
                var id = m.Id?.Trim();

                if (string.IsNullOrEmpty(id) || !ClaveRegex.IsMatch(id))
                {
                    errores.Add($"Id de {tipo} inválido '{m.Id}': usa minúsculas, números y guion bajo, empezando con letra (máx. 40)");
                    continue;
                }

                if (PalabrasReservadas.Contains(id))
                {
                    errores.Add($"'{id}' es una palabra reservada de la sintaxis de texto; elige otro id");
                    continue;
                }

                if (!vistos.Add(id))
                {
                    errores.Add($"El {tipo} '{id}' está repetido");
                    continue;
                }

                var nombre = string.IsNullOrWhiteSpace(m.Nombre) ? id : m.Nombre.Trim();
                var descripcion = m.Descripcion?.Trim() ?? "";

                if (nombre.Length > 60) errores.Add($"El {tipo} '{id}': el nombre supera los 60 caracteres");
                if (descripcion.Length > 300) errores.Add($"El {tipo} '{id}': la descripción supera los 300 caracteres");

                resultado.Add(new MetaDef(id, nombre, descripcion));
            }

            return resultado;
        }

        private static List<ObjetoDef> NormalizarObjetos(List<ObjetoDef> entrada, List<string> errores)
        {
            var objetos = entrada ?? new List<ObjetoDef>();

            if (objetos.Count > MaxObjetos)
            {
                errores.Add($"Se permiten como máximo {MaxObjetos} objetos");
            }

            var resultado = new List<ObjetoDef>();
            var vistos = new HashSet<string>();

            foreach (var o in objetos)
            {
                var id = o.Id?.Trim();

                if (string.IsNullOrEmpty(id) || !ClaveRegex.IsMatch(id))
                {
                    errores.Add($"Id de objeto inválido '{o.Id}': usa minúsculas, números y guion bajo, empezando con letra (máx. 40)");
                    continue;
                }

                if (PalabrasReservadas.Contains(id))
                {
                    errores.Add($"'{id}' es una palabra reservada de la sintaxis de texto; elige otro id");
                    continue;
                }

                if (!vistos.Add(id))
                {
                    errores.Add($"El objeto '{id}' está repetido");
                    continue;
                }

                var nombre = string.IsNullOrWhiteSpace(o.Nombre) ? id : o.Nombre.Trim();
                var descripcion = o.Descripcion?.Trim() ?? "";
                var imagen = string.IsNullOrWhiteSpace(o.ImagenUrl) ? null : o.ImagenUrl.Trim();

                if (nombre.Length > 60) errores.Add($"Objeto '{id}': el nombre supera los 60 caracteres");
                if (descripcion.Length > 300) errores.Add($"Objeto '{id}': la descripción supera los 300 caracteres");

                if (imagen != null && !ImagenObjetoRegex.IsMatch(imagen))
                {
                    errores.Add($"Objeto '{id}': la imagen debe ser un archivo subido a la plataforma");
                    imagen = null;
                }

                int? max = o.Apilable ? o.Max : null;
                if (o.Apilable && max.HasValue && (max < 1 || max > MaxCantidad))
                {
                    errores.Add($"Objeto '{id}': el máximo debe estar entre 1 y {MaxCantidad}");
                }

                var tope = !o.Apilable ? 1 : max ?? MaxCantidad;
                if (o.Inicial < 0 || o.Inicial > tope)
                {
                    errores.Add($"Objeto '{id}': la cantidad inicial debe estar entre 0 y {tope}");
                }

                resultado.Add(new ObjetoDef(id, nombre, descripcion, imagen, o.Apilable, max, o.Inicial, NormalizarUso(o.Uso, id, errores)));
            }

            return resultado;
        }

        private static bool EsNulo(JsonElement? valor) =>
            !valor.HasValue || valor.Value.ValueKind == JsonValueKind.Null || valor.Value.ValueKind == JsonValueKind.Undefined;

        // Deja el uso en su forma canónica: etiqueta con valor por defecto, sin condición ni efectos vacíos.
        private static UsoObjetoDef NormalizarUso(UsoObjetoDef uso, string id, List<string> errores)
        {
            if (uso == null)
            {
                return null;
            }

            var etiqueta = string.IsNullOrWhiteSpace(uso.Etiqueta) ? EtiquetaUsoPorDefecto : uso.Etiqueta.Trim();

            if (etiqueta.Length > MaxEtiquetaUso)
            {
                errores.Add($"Objeto '{id}': el texto del botón de uso supera los {MaxEtiquetaUso} caracteres");
            }

            var condicion = EsNulo(uso.Condicion) ? (JsonElement?)null : uso.Condicion;
            var efectos = EsNulo(uso.Efectos) || (uso.Efectos.Value.ValueKind == JsonValueKind.Array && uso.Efectos.Value.GetArrayLength() == 0)
                ? (JsonElement?)null
                : uso.Efectos;

            if (efectos == null && !uso.Consumir && uso.DestinoRecursoId == null)
            {
                errores.Add($"Objeto '{id}': el uso no hace nada; añade efectos, un destino o márcalo como consumible");
            }

            return new UsoObjetoDef(etiqueta, condicion, efectos, uso.Consumir, uso.DestinoRecursoId);
        }

        private static void ValidarUsos(List<ObjetoDef> objetos, ReglasContexto ctx, List<string> errores)
        {
            foreach (var objeto in objetos.Where(o => o.Uso != null))
            {
                var mensajes = new List<string>();

                if (objeto.Uso.Condicion.HasValue)
                {
                    MotorValidador.ValidarCondicion(objeto.Uso.Condicion.Value, ctx, "condicion", mensajes);
                }

                if (objeto.Uso.Efectos.HasValue)
                {
                    MotorValidador.ValidarEfectos(objeto.Uso.Efectos.Value, ctx, "efectos", mensajes);
                }

                errores.AddRange(mensajes.Select(m => $"Objeto '{objeto.Id}' (uso): {m}"));
            }
        }

        private static List<UbicacionDef> NormalizarUbicaciones(List<UbicacionDef> entrada, List<string> errores)
        {
            var ubicaciones = entrada ?? new List<UbicacionDef>();

            if (ubicaciones.Count > MaxUbicaciones)
            {
                errores.Add($"Se permiten como máximo {MaxUbicaciones} ubicaciones");
            }

            var resultado = new List<UbicacionDef>();
            var vistas = new HashSet<string>();

            foreach (var u in ubicaciones)
            {
                var id = u.Id?.Trim();

                if (string.IsNullOrEmpty(id) || !ClaveRegex.IsMatch(id))
                {
                    errores.Add($"Id de ubicación inválido '{u.Id}': usa minúsculas, números y guion bajo, empezando con letra (máx. 40)");
                    continue;
                }

                if (PalabrasReservadas.Contains(id))
                {
                    errores.Add($"'{id}' es una palabra reservada de la sintaxis de texto; elige otro id");
                    continue;
                }

                if (!vistas.Add(id))
                {
                    errores.Add($"La ubicación '{id}' está repetida");
                    continue;
                }

                var nombre = string.IsNullOrWhiteSpace(u.Nombre) ? id : u.Nombre.Trim();

                if (nombre.Length > 60) errores.Add($"Ubicación '{id}': el nombre supera los 60 caracteres");

                resultado.Add(new UbicacionDef(id, nombre, u.BackgroundSpriteId));
            }

            return resultado;
        }

        private static JsonElement? NormalizarInicial(VariableDef v, string clave, double? min, double? max, List<string> valores, List<string> errores)
        {
            var inicial = v.Inicial;
            var tieneInicial = inicial.HasValue && inicial.Value.ValueKind != JsonValueKind.Null && inicial.Value.ValueKind != JsonValueKind.Undefined;

            switch (v.Tipo)
            {
                case VariableTipos.Numero:
                    if (!tieneInicial) return JsonSerializer.SerializeToElement(min ?? 0d);
                    if (inicial.Value.ValueKind != JsonValueKind.Number)
                    {
                        errores.Add($"Variable '{clave}': el valor inicial debe ser un número");
                        return null;
                    }
                    var n = inicial.Value.GetDouble();
                    if ((min.HasValue && n < min) || (max.HasValue && n > max))
                    {
                        errores.Add($"Variable '{clave}': el valor inicial está fuera del rango");
                    }
                    return inicial;

                case VariableTipos.Booleano:
                    if (!tieneInicial) return JsonSerializer.SerializeToElement(false);
                    if (inicial.Value.ValueKind != JsonValueKind.True && inicial.Value.ValueKind != JsonValueKind.False)
                    {
                        errores.Add($"Variable '{clave}': el valor inicial debe ser verdadero o falso");
                        return null;
                    }
                    return inicial;

                default:
                    if (!tieneInicial) return JsonSerializer.SerializeToElement(valores.FirstOrDefault() ?? "");
                    if (inicial.Value.ValueKind != JsonValueKind.String)
                    {
                        errores.Add($"Variable '{clave}': el valor inicial debe ser texto");
                        return null;
                    }
                    if (valores.Count > 0 && !valores.Contains(inicial.Value.GetString()))
                    {
                        errores.Add($"Variable '{clave}': el valor inicial no está entre los valores permitidos");
                    }
                    return inicial;
            }
        }
    }
}
