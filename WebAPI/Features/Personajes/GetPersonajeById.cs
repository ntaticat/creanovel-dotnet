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

namespace WebAPI.Features.Personajes
{
    public static class GetPersonajeById
    {
        public sealed class Handler
        {
            private readonly CreanovelDbContext _context;

            public Handler(CreanovelDbContext context)
            {
                _context = context;
            }

            public async Task<PersonajeDto> HandleAsync(Guid personajeId, CancellationToken ct)
            {
                var personaje = await _context.Personajes
                    .Include(p => p.Sprites)
                    .FirstOrDefaultAsync(p => p.PersonajeId == personajeId, ct);

                if (personaje == null)
                {
                    throw new ExceptionHandler(HttpStatusCode.NotFound, new { message = "Personaje no encontrado" });
                }

                return new PersonajeDto(
                    personaje.PersonajeId,
                    personaje.Nombre,
                    personaje.Sprites.Select(s => new PersonajeSpriteDto(
                        s.PersonajeSpriteId, s.Nombre, s.DireccionImagen
                    )).ToList()
                );
            }
        }

        public static class Endpoint
        {
            public static void Map(IEndpointRouteBuilder app) =>
                app.MapGet("api/personajes/{personajeId:guid}", async (Guid personajeId, Handler handler, CancellationToken ct) =>
                        Results.Ok(await handler.HandleAsync(personajeId, ct)))
                    .AllowAnonymous()
                    .WithTags("Personajes");
        }
    }
}
