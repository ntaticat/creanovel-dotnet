using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Application.Handlers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Persistence;
using WebAPI.Features.Recursos;

namespace WebAPI.Features.Personajes
{
    public static class DeletePersonajeSprite
    {
        public sealed class Handler
        {
            private readonly CreanovelDbContext _context;

            public Handler(CreanovelDbContext context)
            {
                _context = context;
            }

            public async Task HandleAsync(Guid spriteId, CancellationToken ct)
            {
                var sprite = await _context.PersonajeSprites.FindAsync(new object[] { spriteId }, ct);

                if (sprite == null)
                {
                    throw new ExceptionHandler(HttpStatusCode.NotFound, new { message = "Sprite no encontrado" });
                }

                // Los nodos que lo mostraban dejan de mostrarlo (su id vive dentro de un jsonb, sin FK). Todo en una transacción:
                // si el borrado falla, los nodos conservan su personaje.
                await using var transaction = await _context.Database.BeginTransactionAsync(ct);

                await PersonajesEnEscena.QuitarSpriteAsync(_context, spriteId, ct);

                _context.PersonajeSprites.Remove(sprite);
                var result = await _context.SaveChangesAsync(ct);

                if (result <= 0)
                {
                    throw new ExceptionHandler(HttpStatusCode.BadRequest, new { message = "No se eliminó el sprite" });
                }

                await transaction.CommitAsync(ct);
            }
        }

        public static class Endpoint
        {
            public static void Map(IEndpointRouteBuilder app) =>
                app.MapDelete("api/personajes/sprites/{spriteId:guid}", async (Guid spriteId, Handler handler, CancellationToken ct) =>
                    {
                        await handler.HandleAsync(spriteId, ct);
                        return Results.NoContent();
                    })
                    .RequireAuthorization()
                    .WithTags("Personajes");
        }
    }
}
