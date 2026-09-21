using System;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Application.Handlers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Persistence;
using WebAPI.Infrastructure.Extensions;

namespace WebAPI.Features.Lecturas
{
    public static class RemoveRecursoFromLectura
    {
        public record RemoveRecursoFromLecturaRequest(Guid LecturaId, Guid RecursoId);

        public sealed class Handler
        {
            private readonly CreanovelDbContext _context;

            public Handler(CreanovelDbContext context)
            {
                _context = context;
            }

            public async Task HandleAsync(RemoveRecursoFromLecturaRequest request, Guid? actingUsuarioId, CancellationToken ct)
            {
                await OwnershipGuard.EnsureLecturaOwnerAsync(_context, actingUsuarioId, request.LecturaId, ct);

                // Un recurso puede aparecer varias veces en una lectura (hubs, bucles): se quitan todos los pasos.
                var pasos = await _context.LecturaRecurso
                    .Where(lr => lr.LecturaId == request.LecturaId && lr.RecursoId == request.RecursoId)
                    .ToListAsync(ct);

                if (pasos.Count == 0)
                {
                    throw new ExceptionHandler(HttpStatusCode.NotFound, new { message = "Lectura no encontrada" });
                }

                _context.LecturaRecurso.RemoveRange(pasos);
                var result = await _context.SaveChangesAsync(ct);

                if (result <= 0)
                {
                    throw new ExceptionHandler(HttpStatusCode.BadRequest, new { message = "No se pudo eliminar la relación de Lectura y Recurso" });
                }
            }
        }

        public static class Endpoint
        {
            public static void Map(IEndpointRouteBuilder app) =>
                app.MapDelete("api/lecturas/recursos", async ([FromBody] RemoveRecursoFromLecturaRequest request, Handler handler, ClaimsPrincipal user, CancellationToken ct) =>
                    {
                        await handler.HandleAsync(request, user.GetActingUsuarioId(), ct);
                        return Results.NoContent();
                    })
                    .RequireAuthorization()
                    .WithTags("Lecturas");
        }
    }
}
