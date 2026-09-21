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

namespace WebAPI.Features.Novelas
{
    public static class DeleteNovela
    {
        public sealed class Handler
        {
            private readonly CreanovelDbContext _context;

            public Handler(CreanovelDbContext context)
            {
                _context = context;
            }

            public async Task HandleAsync(Guid novelaId, Guid? actingUsuarioId, CancellationToken ct)
            {
                var novela = await _context.Novelas.FindAsync(new object[] { novelaId }, ct);

                if (novela == null)
                {
                    throw new ExceptionHandler(HttpStatusCode.NotFound, new { message = "Novela no encontrada" });
                }

                OwnershipGuard.EnsureNovelaOwner(actingUsuarioId, novela.UsuarioCreadorId);

                _context.Novelas.Remove(novela);
                var result = await _context.SaveChangesAsync(ct);

                if (result <= 0)
                {
                    throw new ExceptionHandler(HttpStatusCode.BadRequest, new { message = "No se eliminó la novela" });
                }
            }
        }

        public static class Endpoint
        {
            public static void Map(IEndpointRouteBuilder app) =>
                app.MapDelete("api/novelas/{novelaId:guid}", async (Guid novelaId, Handler handler, ClaimsPrincipal user, CancellationToken ct) =>
                    {
                        await handler.HandleAsync(novelaId, user.GetActingUsuarioId(), ct);
                        return Results.NoContent();
                    })
                    .RequireAuthorization()
                    .WithTags("Novelas");
        }
    }
}
