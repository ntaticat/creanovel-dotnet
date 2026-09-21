using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Persistence;

namespace Application.Motor
{
    // El destino de un objeto (`UsoObjetoDef.DestinoRecursoId`) es un id de nodo guardado dentro del jsonb de definiciones: no tiene FK,
    // así que hay que mantenerlo a mano cuando los nodos cambian de id (clonar la versión) o desaparecen (borrar un nodo o una escena).
    public static class UsoObjetoDestinos
    {
        public static IEnumerable<(string ObjetoId, Guid RecursoId)> De(DefinicionesJuego defs) =>
            (defs.Objetos ?? new List<ObjetoDef>())
                .Where(o => o.Uso?.DestinoRecursoId != null)
                .Select(o => (o.Id, o.Uso.DestinoRecursoId.Value));

        // Reescribe cada destino con `transformar` (null lo borra). Devuelve el mismo texto si ningún objeto tiene destino.
        private static string Reescribir(string json, Func<Guid, Guid?> transformar)
        {
            var defs = MotorJson.LeerDefiniciones(json);

            if (!De(defs).Any())
            {
                return json;
            }

            var objetos = defs.Objetos.Select(o => o.Uso?.DestinoRecursoId is Guid destino
                ? o with { Uso = o.Uso with { DestinoRecursoId = transformar(destino) } }
                : o).ToList();

            return MotorJson.Serializar(defs with { Objetos = objetos });
        }

        // Al clonar una versión, los nodos reciben ids nuevos: el destino sigue al nodo clonado (o se pierde si ya no existía).
        public static string Remapear(string json, IReadOnlyDictionary<Guid, Guid> viejoANuevo) =>
            Reescribir(json, destino => viejoANuevo.TryGetValue(destino, out var nuevo) ? nuevo : null);

        public static string Quitar(string json, IReadOnlySet<Guid> eliminados) =>
            De(MotorJson.LeerDefiniciones(json)).Any(d => eliminados.Contains(d.RecursoId))
                ? Reescribir(json, destino => eliminados.Contains(destino) ? null : destino)
                : json;

        // Quita de las definiciones de la versión los destinos de objetos que apuntan a nodos que se van a borrar.
        // No guarda: se llama antes del SaveChanges del borrado para que ambos cambios vayan en la misma transacción.
        public static async Task QuitarDeVersionAsync(CreanovelDbContext context, Guid novelaVersionId, IReadOnlyCollection<Guid> eliminados, CancellationToken ct)
        {
            var version = await context.NovelaVersiones.FindAsync(new object[] { novelaVersionId }, ct);

            if (version == null)
            {
                return;
            }

            version.Definiciones = Quitar(version.Definiciones, eliminados.ToHashSet());
        }
    }
}
