using System;
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

namespace WebAPI.Features.Novelas
{
    public static class UpdateNovela
    {
        public record UpdateNovelaRequest(string Titulo, string Descripcion, bool? Disponible, string PortadaImagenUrl, Guid? UsuarioCreadorId);

        public sealed class Handler
        {
            private readonly CreanovelDbContext _context;

            public Handler(CreanovelDbContext context)
            {
                _context = context;
            }

            public async Task HandleAsync(Guid novelaId, UpdateNovelaRequest request, Guid? actingUsuarioId, CancellationToken ct)
            {
                var novela = await _context.Novelas.FindAsync(new object[] { novelaId }, ct);

                if (novela == null)
                {
                    throw new ExceptionHandler(HttpStatusCode.NotFound, new { message = "Novela no encontrada" });
                }

                OwnershipGuard.EnsureNovelaOwner(actingUsuarioId, novela.UsuarioCreadorId);

                novela.Titulo = request.Titulo ?? novela.Titulo;
                novela.Descripcion = request.Descripcion ?? novela.Descripcion;
                novela.Disponible = request.Disponible ?? novela.Disponible;
                novela.PortadaImagenUrl = request.PortadaImagenUrl ?? novela.PortadaImagenUrl;
                novela.UsuarioCreadorId = request.UsuarioCreadorId ?? novela.UsuarioCreadorId;

                var result = await _context.SaveChangesAsync(ct);

                if (result <= 0)
                {
                    throw new ExceptionHandler(HttpStatusCode.BadRequest, new { message = "No se actualizó la novela" });
                }
            }
        }

        public static class Endpoint
        {
            public static void Map(IEndpointRouteBuilder app) =>
                app.MapPatch("api/novelas/{novelaId:guid}", async (Guid novelaId, [FromBody] UpdateNovelaRequest request, Handler handler, ClaimsPrincipal user, CancellationToken ct) =>
                    {
                        await handler.HandleAsync(novelaId, request, user.GetActingUsuarioId(), ct);
                        return Results.NoContent();
                    })
                    .RequireAuthorization()
                    .WithTags("Novelas");
        }
    }
}
