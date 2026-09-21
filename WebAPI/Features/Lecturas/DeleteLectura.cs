using System;
using System.Net;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Application.Handlers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Persistence;
using WebAPI.Infrastructure.Extensions;

namespace WebAPI.Features.Lecturas
{
    public static class DeleteLectura
    {
        public sealed class Handler
        {
            private readonly CreanovelDbContext _context;

            public Handler(CreanovelDbContext context)
            {
                _context = context;
            }

            public async Task HandleAsync(Guid lecturaId, Guid? actingUsuarioId, CancellationToken ct)
            {
                await OwnershipGuard.EnsureLecturaOwnerAsync(_context, actingUsuarioId, lecturaId, ct);

                var lectura = await _context.Lecturas.FindAsync(new object[] { lecturaId }, ct);

                if (lectura == null)
                {
                    throw new ExceptionHandler(HttpStatusCode.NotFound, new { message = "Lectura no encontrada" });
                }

                _context.Lecturas.Remove(lectura);
                var result = await _context.SaveChangesAsync(ct);

                if (result <= 0)
                {
                    throw new ExceptionHandler(HttpStatusCode.BadRequest, new { message = "No se pudo eliminar la lectura" });
                }
            }
        }

        public static class Endpoint
        {
            public static void Map(IEndpointRouteBuilder app) =>
                app.MapDelete("api/lecturas/{lecturaId:guid}", async (Guid lecturaId, Handler handler, ClaimsPrincipal user, CancellationToken ct) =>
                    {
                        await handler.HandleAsync(lecturaId, user.GetActingUsuarioId(), ct);
                        return Results.NoContent();
                    })
                    .RequireAuthorization()
                    .WithTags("Lecturas");
        }
    }
}
