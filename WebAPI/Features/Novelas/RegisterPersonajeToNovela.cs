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
    public static class RegisterPersonajeToNovela
    {
        public record RegisterPersonajeToNovelaRequest(Guid PersonajeId);

        public sealed class Handler
        {
            private readonly CreanovelDbContext _context;

            public Handler(CreanovelDbContext context)
            {
                _context = context;
            }

            public async Task HandleAsync(Guid novelaId, RegisterPersonajeToNovelaRequest request, Guid? actingUsuarioId, CancellationToken ct)
            {
                var novela = await _context.Novelas.FindAsync(new object[] { novelaId }, ct);

                if (novela == null)
                {
                    throw new ExceptionHandler(HttpStatusCode.NotFound, new { message = "Novela no encontrada" });
                }

                OwnershipGuard.EnsureNovelaOwner(actingUsuarioId, novela.UsuarioCreadorId);

                var novelaPersonaje = new Domain.Models.NovelaPersonaje { NovelaId = novelaId, PersonajeId = request.PersonajeId };
                await _context.NovelaPersonaje.AddAsync(novelaPersonaje, ct);

                var result = await _context.SaveChangesAsync(ct);

                if (result <= 0)
                {
                    throw new ExceptionHandler(HttpStatusCode.BadRequest, new { message = "No se pudo registrar la relación de Novela y Personaje" });
                }
            }
        }

        public static class Endpoint
        {
            public static void Map(IEndpointRouteBuilder app) =>
                app.MapPost("api/novelas/{novelaId:guid}/personajes", async (Guid novelaId, [FromBody] RegisterPersonajeToNovelaRequest request, Handler handler, ClaimsPrincipal user, CancellationToken ct) =>
                    {
                        await handler.HandleAsync(novelaId, request, user.GetActingUsuarioId(), ct);
                        return Results.NoContent();
                    })
                    .RequireAuthorization()
                    .WithTags("Novelas");
        }
    }
}
