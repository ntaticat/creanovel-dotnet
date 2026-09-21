using System;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Application.Handlers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace WebAPI.Features.Backgrounds
{
    public static class GetBackgroundById
    {
        public sealed class Handler
        {
            private readonly CreanovelDbContext _context;

            public Handler(CreanovelDbContext context)
            {
                _context = context;
            }

            public async Task<BackgroundDto> HandleAsync(Guid backgroundId, CancellationToken ct)
            {
                var background = await _context.Backgrounds
                    .Include(b => b.Sprites)
                    .FirstOrDefaultAsync(b => b.BackgroundId == backgroundId, ct);

                if (background == null)
                {
                    throw new ExceptionHandler(HttpStatusCode.NotFound, new { message = "Background no encontrado" });
                }

                return new BackgroundDto(
                    background.BackgroundId,
                    background.Descripcion,
                    background.Sprites.Select(s => new BackgroundSpriteDto(s.BackgroundSpriteId, s.Nombre, s.DireccionImagen)).ToList()
                );
            }
        }

        public static class Endpoint
        {
            public static void Map(IEndpointRouteBuilder app) =>
                app.MapGet("api/backgrounds/{backgroundId:guid}", async (Guid backgroundId, Handler handler, CancellationToken ct) =>
                        Results.Ok(await handler.HandleAsync(backgroundId, ct)))
                    .AllowAnonymous()
                    .WithTags("Backgrounds");
        }
    }
}
