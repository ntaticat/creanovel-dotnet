using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace WebAPI.Features.Backgrounds
{
    public static class GetBackgrounds
    {
        public sealed class Handler
        {
            private readonly CreanovelDbContext _context;

            public Handler(CreanovelDbContext context)
            {
                _context = context;
            }

            public async Task<List<BackgroundDto>> HandleAsync(CancellationToken ct)
            {
                var backgrounds = await _context.Backgrounds.Include(b => b.Sprites).ToListAsync(ct);

                return backgrounds
                    .Select(b => new BackgroundDto(
                        b.BackgroundId,
                        b.Descripcion,
                        b.Sprites.Select(s => new BackgroundSpriteDto(s.BackgroundSpriteId, s.Nombre, s.DireccionImagen)).ToList()
                    ))
                    .ToList();
            }
        }

        public static class Endpoint
        {
            public static void Map(IEndpointRouteBuilder app) =>
                app.MapGet("api/backgrounds", async (Handler handler, CancellationToken ct) =>
                        Results.Ok(await handler.HandleAsync(ct)))
                    .AllowAnonymous()
                    .WithTags("Backgrounds");
        }
    }
}
