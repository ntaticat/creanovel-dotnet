using System;
using System.Collections.Generic;
using System.Net;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Application.Handlers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Persistence;
using WebAPI.Infrastructure.Extensions;

namespace WebAPI.Features.Escenas
{
    public static class UpdateEscena
    {
        public record UpdateEscenaRequest(string Identificador, Guid? NovelaVersionId, bool? PrimerEscena, bool? UltimaEscena, IReadOnlyList<string> Etiquetas = null);

        public sealed class Handler
        {
            private readonly CreanovelDbContext _context;

            public Handler(CreanovelDbContext context)
            {
                _context = context;
            }

            public async Task HandleAsync(Guid escenaId, UpdateEscenaRequest request, Guid? actingUsuarioId, CancellationToken ct)
            {
                var escena = await _context.Escenas.FindAsync(new object[] { escenaId }, ct);

                if (escena == null)
                {
                    throw new ExceptionHandler(HttpStatusCode.NotFound, new { message = "Escena no encontrada" });
                }

                await OwnershipGuard.EnsureEscenaOwnerAsync(_context, actingUsuarioId, escena.EscenaId, ct);

                escena.Identificador = request.Identificador ?? escena.Identificador;
                escena.NovelaVersionId = request.NovelaVersionId ?? escena.NovelaVersionId;
                escena.PrimerEscena = request.PrimerEscena ?? escena.PrimerEscena;
                escena.UltimaEscena = request.UltimaEscena ?? escena.UltimaEscena;

                // `null` = sin cambios, `[]` = quitar todas las etiquetas.
                if (request.Etiquetas != null)
                {
                    escena.Etiquetas = EscenaEtiquetas.Normalizar(request.Etiquetas);
                }

                var result = await _context.SaveChangesAsync(ct);

                if (result <= 0)
                {
                    throw new ExceptionHandler(HttpStatusCode.BadRequest, new { message = "No se actualizó la escena" });
                }
            }
        }

        public static class Endpoint
        {
            public static void Map(IEndpointRouteBuilder app) =>
                app.MapPatch("api/escenas/{escenaId:guid}", async (Guid escenaId, [FromBody] UpdateEscenaRequest request, Handler handler, ClaimsPrincipal user, CancellationToken ct) =>
                    {
                        await handler.HandleAsync(escenaId, request, user.GetActingUsuarioId(), ct);
                        return Results.NoContent();
                    })
                    .RequireAuthorization()
                    .WithTags("Escenas");
        }
    }
}
