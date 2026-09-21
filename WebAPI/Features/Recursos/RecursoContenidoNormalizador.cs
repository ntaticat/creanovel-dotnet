using System.Collections.Generic;
using System.Text.Json;
using Application.Motor;

namespace WebAPI.Features.Recursos
{
    // Valida y normaliza el `Contenido` de los tipos que se crean por los endpoints genéricos.
    internal static class RecursoContenidoNormalizador
    {
        // Devuelve el JSON a guardar en Recurso.Contenido; agrega a `errores` si la entrada no es válida.
        public static string Normalizar(string tipo, JsonElement? contenido, ReglasContexto ctx, List<string> errores)
        {
            switch (tipo)
            {
                case RecursoTipos.Asigna:
                    var efectos = LeerEfectos(contenido);

                    if (efectos == null)
                    {
                        errores.Add("contenido.efectos: se requiere una lista de efectos");
                        return "{}";
                    }

                    MotorValidador.ValidarEfectos(efectos.Value, ctx, "contenido.efectos", errores);
                    return JsonSerializer.Serialize(new AsignaContenido(efectos.Value));

                case RecursoTipos.Explora:
                    var mensaje = LeerTexto(contenido, "mensaje") ?? "";

                    if (mensaje.Length > 300)
                    {
                        errores.Add("contenido.mensaje: el texto supera los 300 caracteres");
                    }

                    return JsonSerializer.Serialize(new ExploraContenido(mensaje.Trim()));

                case RecursoTipos.Termina:
                    var final = LeerTexto(contenido, "final");
                    var cierre = LeerTexto(contenido, "mensaje") ?? "";

                    if (string.IsNullOrWhiteSpace(final) || !ctx.Finales.Contains(final))
                    {
                        errores.Add($"contenido.final: '{final}' no es un final definido (créalo antes en «Mundo»)");
                    }

                    if (cierre.Length > 500)
                    {
                        errores.Add("contenido.mensaje: el texto supera los 500 caracteres");
                    }

                    return JsonSerializer.Serialize(new TerminaContenido(final ?? "", cierre.Trim()));

                case RecursoTipos.Juega:
                    var texto = LeerTexto(contenido, "mensaje") ?? "";
                    var resultado = LeerTexto(contenido, "variableResultado");
                    var minijuego = LeerPropiedad(contenido, "minijuego");

                    if (texto.Length > 300)
                    {
                        errores.Add("contenido.mensaje: el texto supera los 300 caracteres");
                    }

                    if (minijuego == null)
                    {
                        errores.Add("contenido.minijuego: falta la configuración del minijuego");
                    }
                    else
                    {
                        MinijuegoValidador.Validar(minijuego.Value, "contenido.minijuego", errores);
                    }

                    // El puntaje se guarda en una variable numérica; si se nombra una, debe existir.
                    if (!string.IsNullOrWhiteSpace(resultado)
                        && (!ctx.Variables.TryGetValue(resultado, out var variable) || variable.Tipo != VariableTipos.Numero))
                    {
                        errores.Add($"contenido.variableResultado: '{resultado}' debe ser una variable numérica definida");
                    }

                    return minijuego == null
                        ? "{}"
                        : JsonSerializer.Serialize(new JuegaContenido(texto.Trim(), string.IsNullOrWhiteSpace(resultado) ? null : resultado, minijuego.Value));

                default:
                    return "{}";
            }
        }

        private static JsonElement? LeerPropiedad(JsonElement? contenido, string propiedad)
        {
            if (contenido == null || contenido.Value.ValueKind != JsonValueKind.Object)
            {
                return null;
            }

            foreach (var p in contenido.Value.EnumerateObject())
            {
                if (string.Equals(p.Name, propiedad, System.StringComparison.OrdinalIgnoreCase))
                {
                    return p.Value;
                }
            }

            return null;
        }

        private static string LeerTexto(JsonElement? contenido, string propiedad)
        {
            if (contenido == null || contenido.Value.ValueKind != JsonValueKind.Object)
            {
                return null;
            }

            foreach (var p in contenido.Value.EnumerateObject())
            {
                if (string.Equals(p.Name, propiedad, System.StringComparison.OrdinalIgnoreCase) && p.Value.ValueKind == JsonValueKind.String)
                {
                    return p.Value.GetString();
                }
            }

            return null;
        }

        // Lee la lista de efectos de un contenido `{ "efectos": [...] }` (cliente) o `{ "Efectos": [...] }` (guardado).
        public static JsonElement? LeerEfectos(JsonElement? contenido)
        {
            if (contenido == null || contenido.Value.ValueKind != JsonValueKind.Object)
            {
                return null;
            }

            foreach (var propiedad in contenido.Value.EnumerateObject())
            {
                if (propiedad.NameEquals("efectos") || propiedad.Name == "Efectos")
                {
                    return propiedad.Value.ValueKind == JsonValueKind.Array ? propiedad.Value : null;
                }
            }

            return null;
        }
    }
}
