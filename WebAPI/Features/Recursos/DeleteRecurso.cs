using System;
using System.Net;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using Application.Handlers;
using Application.Motor;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Persistence;
using WebAPI.Infrastructure.Extensions;

namespace WebAPI.Features.Recursos
{
    public static class DeleteRecurso
    {
        public sealed class Handler
        {
            private readonly CreanovelDbContext _context;

            public Handler(CreanovelDbContext context)
            {
                _context = context;
            }

            public async Task HandleAsync(Guid recursoId, Guid? actingUsuarioId, CancellationToken ct)
            {
                var recurso = await _context.Recursos.FindAsync(new object[] { recursoId }, ct);

                if (recurso == null)
                {
                    throw new ExceptionHandler(HttpStatusCode.NotFound, new { message = "Recurso no encontrado" });
                }

                await OwnershipGuard.EnsureRecursoOwnerAsync(_context, actingUsuarioId, recurso.RecursoId, ct);

                // Un objeto que llevaba a este nodo pierde su destino (no hay FK: el id vive dentro del jsonb de definiciones).
                var novelaVersionId = await _context.Escenas
                    .Where(e => e.EscenaId == recurso.EscenaId)
                    .Select(e => e.NovelaVersionId)
                    .FirstAsync(ct);
                await UsoObjetoDestinos.QuitarDeVersionAsync(_context, novelaVersionId, new[] { recursoId }, ct);

                _context.Recursos.Remove(recurso);
                var result = await _context.SaveChangesAsync(ct);

                if (result <= 0)
                {
                    throw new ExceptionHandler(HttpStatusCode.NotFound, new { message = "No se eliminó el recurso" });
                }
            }
        }

        public static class Endpoint
        {
            public static void Map(IEndpointRouteBuilder app) =>
                app.MapDelete("api/recursos/{recursoId:guid}", async (Guid recursoId, Handler handler, ClaimsPrincipal user, CancellationToken ct) =>
                    {
                        await handler.HandleAsync(recursoId, user.GetActingUsuarioId(), ct);
                        return Results.NoContent();
                    })
                    .RequireAuthorization()
                    .WithTags("Recursos");
        }
    }
}
