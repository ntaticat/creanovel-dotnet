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

namespace WebAPI.Features.Backgrounds
{
    public static class CreateBackgroundSprite
    {
        public record CreateBackgroundSpriteRequest(string Nombre, string DireccionImagen, Guid BackgroundId);

        public sealed class Handler
        {
            private readonly CreanovelDbContext _context;

            public Handler(CreanovelDbContext context)
            {
                _context = context;
            }

            public async Task<Guid> HandleAsync(CreateBackgroundSpriteRequest request, CancellationToken ct)
            {
                var background = await _context.Backgrounds.FindAsync(new object[] { request.BackgroundId }, ct);

                if (background == null)
                {
                    throw new ExceptionHandler(HttpStatusCode.NotFound, new { message = "Background no encontrado" });
                }

                var sprite = new Domain.Models.BackgroundSprite
                {
                    Nombre = request.Nombre,
                    DireccionImagen = request.DireccionImagen,
                    BackgroundId = request.BackgroundId
                };

                await _context.BackgroundSprites.AddAsync(sprite, ct);
                var result = await _context.SaveChangesAsync(ct);

                if (result > 0)
                {
                    return sprite.BackgroundSpriteId;
                }

                throw new ExceptionHandler(HttpStatusCode.BadRequest, new { message = "No se pudo registrar el sprite del background" });
            }
        }

        public static class Endpoint
        {
            public static void Map(IEndpointRouteBuilder app) =>
                app.MapPost("api/backgrounds/sprites", async ([FromBody] CreateBackgroundSpriteRequest request, Handler handler, CancellationToken ct) =>
                        Results.Ok(await handler.HandleAsync(request, ct)))
                    .RequireAuthorization()
                    .WithTags("Backgrounds");
        }
    }
}
