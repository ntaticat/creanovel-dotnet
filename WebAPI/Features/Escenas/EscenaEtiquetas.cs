using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Text.RegularExpressions;
using Application.Handlers;

namespace WebAPI.Features.Escenas
{
    // Las etiquetas de una escena (jsonb: lista de strings) sirven al autor para agruparlas y filtrarlas en el editor; el motor no las usa.
    // Los límites se repiten en el frontend (`escenas-etiquetas.ts`): mantenerlos iguales.
    public static class EscenaEtiquetas
    {
        public const int Maximo = 10;
        public const int LongitudMaxima = 30;

        private static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web);
        private static readonly Regex Espacios = new(@"\s+", RegexOptions.Compiled);

        // Lee lo guardado; ante datos corruptos o vacíos devuelve una lista vacía.
        public static IReadOnlyList<string> Leer(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                return Array.Empty<string>();
            }

            try
            {
                return JsonSerializer.Deserialize<List<string>>(json, Options) ?? new List<string>();
            }
            catch (JsonException)
            {
                return Array.Empty<string>();
            }
        }

        // Limpia la lista que envía el editor (recorta y junta espacios, descarta vacías y repetidas sin distinguir mayúsculas; la
        // primera escritura es la que se conserva) y devuelve el jsonb a guardar. `null` significa "sin cambios" en las actualizaciones
        // (lo decide quien llama); aquí una lista vacía es simplemente "sin etiquetas".
        public static string Normalizar(IReadOnlyList<string> etiquetas)
        {
            var limpias = new List<string>();

            foreach (var etiqueta in etiquetas ?? Array.Empty<string>())
            {
                var limpia = Espacios.Replace((etiqueta ?? string.Empty).Trim(), " ");

                if (limpia.Length == 0 || limpias.Contains(limpia, StringComparer.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (limpia.Length > LongitudMaxima)
                {
                    throw new ExceptionHandler(HttpStatusCode.BadRequest, new { message = $"Una etiqueta puede tener como máximo {LongitudMaxima} caracteres" });
                }

                limpias.Add(limpia);
            }

            if (limpias.Count > Maximo)
            {
                throw new ExceptionHandler(HttpStatusCode.BadRequest, new { message = $"Una escena admite como máximo {Maximo} etiquetas" });
            }

            return JsonSerializer.Serialize(limpias, Options);
        }
    }
}
