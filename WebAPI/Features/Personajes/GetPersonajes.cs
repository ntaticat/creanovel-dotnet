using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace WebAPI.Features.Personajes
{
    public static class GetPersonajes
    {
        public sealed class Handler
        {
            private readonly CreanovelDbContext _context;

            public Handler(CreanovelDbContext context)
            {
                _context = context;
            }

            public async Task<List<PersonajeDto>> HandleAsync(CancellationToken ct)
            {
                var personajes = await _context.Personajes.Include(p => p.Sprites).ToListAsync(ct);

                return personajes
                    .Select(p => new PersonajeDto(
                        p.PersonajeId,
                        p.Nombre,
                        p.Sprites.Select(s => new PersonajeSpriteDto(
                            s.PersonajeSpriteId, s.Nombre, s.DireccionImagen
                        )).ToList()
                    ))
                    .ToList();
            }
        }

        public static class Endpoint
        {
            public static void Map(IEndpointRouteBuilder app) =>
                app.MapGet("api/personajes", async (Handler handler, CancellationToken ct) =>
                        Results.Ok(await handler.HandleAsync(ct)))
                    .AllowAnonymous()
                    .WithTags("Personajes");
        }
    }
}
