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
    public static class CreatePersonajeSprite
    {
        public record CreatePersonajeSpriteRequest(
            string Nombre,
            string DireccionImagen,
            Guid PersonajeId
        );

        public sealed class Handler
        {
            private readonly CreanovelDbContext _context;

            public Handler(CreanovelDbContext context)
            {
                _context = context;
            }

            public async Task<Guid> HandleAsync(CreatePersonajeSpriteRequest request, CancellationToken ct)
            {
                var personaje = await _context.Personajes.FindAsync(new object[] { request.PersonajeId }, ct);

                if (personaje == null)
                {
                    throw new ExceptionHandler(HttpStatusCode.NotFound, new { message = "Personaje no encontrado" });
                }

                var sprite = new Domain.Models.PersonajeSprite
                {
                    Nombre = request.Nombre,
                    DireccionImagen = request.DireccionImagen,
                    PersonajeId = request.PersonajeId
                };

                await _context.PersonajeSprites.AddAsync(sprite, ct);
                var result = await _context.SaveChangesAsync(ct);

                if (result > 0)
                {
                    return sprite.PersonajeSpriteId;
                }

                throw new ExceptionHandler(HttpStatusCode.BadRequest, new { message = "No se pudo registrar el sprite del personaje" });
            }
        }

        public static class Endpoint
        {
            public static void Map(IEndpointRouteBuilder app) =>
                app.MapPost("api/personajes/sprites", async ([FromBody] CreatePersonajeSpriteRequest request, Handler handler, CancellationToken ct) =>
                        Results.Ok(await handler.HandleAsync(request, ct)))
                    .RequireAuthorization()
                    .WithTags("Personajes");
        }
    }
}
