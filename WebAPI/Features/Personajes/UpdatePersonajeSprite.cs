using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Application.Handlers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Persistence;

namespace WebAPI.Features.Personajes
{
    public static class UpdatePersonajeSprite
    {
        public record UpdatePersonajeSpriteRequest(
            string Nombre,
            string DireccionImagen
        );

        public sealed class Handler
        {
            private readonly CreanovelDbContext _context;

            public Handler(CreanovelDbContext context)
            {
                _context = context;
            }

            public async Task HandleAsync(Guid spriteId, UpdatePersonajeSpriteRequest request, CancellationToken ct)
            {
                var sprite = await _context.PersonajeSprites.FindAsync(new object[] { spriteId }, ct);

                if (sprite == null)
                {
                    throw new ExceptionHandler(HttpStatusCode.NotFound, new { message = "Sprite no encontrado" });
                }

                sprite.Nombre = request.Nombre ?? sprite.Nombre;
                sprite.DireccionImagen = request.DireccionImagen ?? sprite.DireccionImagen;

                var result = await _context.SaveChangesAsync(ct);

                if (result <= 0)
                {
                    throw new ExceptionHandler(HttpStatusCode.BadRequest, new { message = "No se actualizó el sprite" });
                }
            }
        }

        public static class Endpoint
        {
            public static void Map(IEndpointRouteBuilder app) =>
                app.MapPatch("api/personajes/sprites/{spriteId:guid}", async (Guid spriteId, [FromBody] UpdatePersonajeSpriteRequest request, Handler handler, CancellationToken ct) =>
                    {
                        await handler.HandleAsync(spriteId, request, ct);
                        return Results.NoContent();
                    })
                    .RequireAuthorization()
                    .WithTags("Personajes");
        }
    }
}
