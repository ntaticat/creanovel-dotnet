using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Application.Handlers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Persistence;

namespace WebAPI.Features.Backgrounds
{
    public static class DeleteBackgroundSprite
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
                var sprite = await _context.BackgroundSprites.FindAsync(new object[] { spriteId }, ct);

                if (sprite == null)
                {
                    throw new ExceptionHandler(HttpStatusCode.NotFound, new { message = "Sprite no encontrado" });
                }

                _context.BackgroundSprites.Remove(sprite);
                var result = await _context.SaveChangesAsync(ct);

                if (result <= 0)
                {
                    throw new ExceptionHandler(HttpStatusCode.BadRequest, new { message = "No se eliminó el sprite" });
                }
            }
        }

        public static class Endpoint
        {
            public static void Map(IEndpointRouteBuilder app) =>
                app.MapDelete("api/backgrounds/sprites/{spriteId:guid}", async (Guid spriteId, Handler handler, CancellationToken ct) =>
                    {
                        await handler.HandleAsync(spriteId, ct);
                        return Results.NoContent();
                    })
                    .RequireAuthorization()
                    .WithTags("Backgrounds");
        }
    }
}
