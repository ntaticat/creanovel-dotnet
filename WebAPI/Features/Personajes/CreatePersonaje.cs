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
    public static class CreatePersonaje
    {
        public record CreatePersonajeRequest(string Nombre);

        public sealed class Handler
        {
            private readonly CreanovelDbContext _context;

            public Handler(CreanovelDbContext context)
            {
                _context = context;
            }

            public async Task<Guid> HandleAsync(CreatePersonajeRequest request, CancellationToken ct)
            {
                var personaje = new Domain.Models.Personaje { Nombre = request.Nombre };
                await _context.Personajes.AddAsync(personaje, ct);
                var result = await _context.SaveChangesAsync(ct);

                if (result > 0)
                {
                    return personaje.PersonajeId;
                }

                throw new ExceptionHandler(HttpStatusCode.BadRequest, new { message = "No se pudo registrar el personaje" });
            }
        }

        public static class Endpoint
        {
            public static void Map(IEndpointRouteBuilder app) =>
                app.MapPost("api/personajes", async ([FromBody] CreatePersonajeRequest request, Handler handler, CancellationToken ct) =>
                        Results.Ok(await handler.HandleAsync(request, ct)))
                    .RequireAuthorization()
                    .WithTags("Personajes");
        }
    }
}
