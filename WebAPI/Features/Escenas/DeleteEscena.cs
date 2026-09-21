using System;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Application.Handlers;
using Application.Motor;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Persistence;
using WebAPI.Infrastructure.Extensions;

namespace WebAPI.Features.Escenas
{
    public static class DeleteEscena
    {
        public sealed class Handler
        {
            private readonly CreanovelDbContext _context;

            public Handler(CreanovelDbContext context)
            {
                _context = context;
            }

            public async Task HandleAsync(Guid escenaId, Guid? actingUsuarioId, CancellationToken ct)
            {
                var escena = await _context.Escenas.FindAsync(new object[] { escenaId }, ct);

                if (escena == null)
                {
                    throw new ExceptionHandler(HttpStatusCode.NotFound, new { message = "Escena no encontrada" });
                }

                await OwnershipGuard.EnsureEscenaOwnerAsync(_context, actingUsuarioId, escena.EscenaId, ct);

                // Los objetos que llevaban a un nodo de esta escena pierden su destino (el id vive dentro del jsonb de definiciones).
                var recursoIds = await _context.Recursos.Where(r => r.EscenaId == escenaId).Select(r => r.RecursoId).ToListAsync(ct);
                await UsoObjetoDestinos.QuitarDeVersionAsync(_context, escena.NovelaVersionId, recursoIds, ct);

                _context.Escenas.Remove(escena);
                var result = await _context.SaveChangesAsync(ct);

                if (result <= 0)
                {
                    throw new ExceptionHandler(HttpStatusCode.BadRequest, new { message = "No se eliminó la escena" });
                }
            }
        }

        public static class Endpoint
        {
            public static void Map(IEndpointRouteBuilder app) =>
                app.MapDelete("api/escenas/{escenaId:guid}", async (Guid escenaId, Handler handler, ClaimsPrincipal user, CancellationToken ct) =>
                    {
                        await handler.HandleAsync(escenaId, user.GetActingUsuarioId(), ct);
                        return Results.NoContent();
                    })
                    .RequireAuthorization()
                    .WithTags("Escenas");
        }
    }
}
