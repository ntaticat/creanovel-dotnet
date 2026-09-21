using System;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Application.Handlers;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace WebAPI.Features.Recursos
{
    internal static class RecursoEnlaces
    {
        // Un enlace a un recurso de otra versión (o inexistente) rompería el draft/publicación.
        public static async Task EnsureMismaVersionAsync(CreanovelDbContext context, Guid novelaVersionId, Guid? destinoId, CancellationToken ct)
        {
            if (destinoId == null)
            {
                return;
            }

            var existe = await context.Recursos
                .AnyAsync(r => r.RecursoId == destinoId && r.Escena.NovelaVersionId == novelaVersionId, ct);

            if (!existe)
            {
                throw new ExceptionHandler(HttpStatusCode.BadRequest, new { message = "El recurso de destino no existe en esta versión" });
            }
        }
    }
}
