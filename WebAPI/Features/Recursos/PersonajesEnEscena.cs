using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Application.Handlers;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace WebAPI.Features.Recursos
{
    // Un personaje colocado en el escenario de un nodo. `X` e `Y` son el centro del sprite en % del escenario (16:9), como las zonas
    // de un Explora; `Escala` multiplica el tamaño base (el sprite entero cabe en el escenario, sin recortarse) y `Espejo` lo voltea.
    // El orden de la lista es el orden de apilado: el último se dibuja encima.
    public record PersonajeEnEscenaDto(Guid PersonajeSpriteId, double X, double Y, double Escala, bool Espejo);

    public static class PersonajesEnEscena
    {
        public const int Maximo = 8;
        public const double EscalaMinima = 0.1;
        public const double EscalaMaxima = 4;

        private static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web);

        // Lee lo guardado; ante datos corruptos o vacíos devuelve una lista vacía.
        public static IReadOnlyList<PersonajeEnEscenaDto> Leer(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                return Array.Empty<PersonajeEnEscenaDto>();
            }

            try
            {
                return JsonSerializer.Deserialize<List<PersonajeEnEscenaDto>>(json, Options) ?? new List<PersonajeEnEscenaDto>();
            }
            catch (JsonException)
            {
                return Array.Empty<PersonajeEnEscenaDto>();
            }
        }

        public static string Serializar(IEnumerable<PersonajeEnEscenaDto> personajes) => JsonSerializer.Serialize(personajes, Options);

        // Valida la lista que envía el editor y devuelve el jsonb a guardar. `null` significa "sin cambios" en las actualizaciones
        // (lo decide quien llama); aquí una lista vacía es simplemente "sin personajes".
        public static async Task<string> NormalizarAsync(CreanovelDbContext context, IReadOnlyList<PersonajeEnEscenaDto> personajes, CancellationToken ct)
        {
            personajes ??= Array.Empty<PersonajeEnEscenaDto>();
            var errores = new List<string>();

            if (personajes.Count > Maximo)
            {
                errores.Add($"Se permiten como máximo {Maximo} personajes por escenario");
            }

            for (var i = 0; i < personajes.Count; i++)
            {
                var p = personajes[i];

                if (!EstaEntre(p.X, 0, 100) || !EstaEntre(p.Y, 0, 100))
                {
                    errores.Add($"Personaje {i + 1}: la posición debe estar entre 0 y 100 (% del escenario)");
                }

                if (!EstaEntre(p.Escala, EscalaMinima, EscalaMaxima))
                {
                    errores.Add($"Personaje {i + 1}: la escala debe estar entre {EscalaMinima} y {EscalaMaxima}");
                }
            }

            var ids = personajes.Select(p => p.PersonajeSpriteId).Distinct().ToList();

            if (ids.Count > 0)
            {
                var existentes = await context.PersonajeSprites.CountAsync(s => ids.Contains(s.PersonajeSpriteId), ct);

                if (existentes != ids.Count)
                {
                    errores.Add("Un personaje del escenario usa un sprite que no existe");
                }
            }

            if (errores.Count > 0)
            {
                throw new ExceptionHandler(HttpStatusCode.BadRequest, new { message = "Los personajes del escenario no son válidos", errores });
            }

            return Serializar(personajes);
        }

        private static bool EstaEntre(double valor, double min, double max) => double.IsFinite(valor) && valor >= min && valor <= max;

        // Quita de todos los nodos las apariciones de un sprite que se va a borrar. El id vive dentro de un jsonb (sin FK),
        // así que hay que limpiarlo a mano. No guarda: se llama antes del SaveChanges del borrado para que vaya en la misma transacción.
        public static async Task QuitarSpriteAsync(CreanovelDbContext context, Guid personajeSpriteId, CancellationToken ct)
        {
            if (context.Database.IsRelational())
            {
                await context.Database.ExecuteSqlInterpolatedAsync($@"
                    UPDATE ""Recursos""
                    SET ""Personajes"" = COALESCE((
                        SELECT jsonb_agg(elemento ORDER BY posicion)
                        FROM jsonb_array_elements(""Personajes"") WITH ORDINALITY AS t(elemento, posicion)
                        WHERE elemento->>'personajeSpriteId' IS DISTINCT FROM {personajeSpriteId.ToString()}
                    ), '[]'::jsonb)
                    WHERE ""Personajes"" @> jsonb_build_array(jsonb_build_object('personajeSpriteId', {personajeSpriteId.ToString()}::text))", ct);
                return;
            }

            // Proveedores sin SQL (las pruebas con InMemory): mismo resultado recorriendo los nodos.
            var afectados = (await context.Recursos.ToListAsync(ct))
                .Where(r => Leer(r.Personajes).Any(p => p.PersonajeSpriteId == personajeSpriteId));

            foreach (var recurso in afectados)
            {
                recurso.Personajes = Serializar(Leer(recurso.Personajes).Where(p => p.PersonajeSpriteId != personajeSpriteId));
            }
        }
    }
}
