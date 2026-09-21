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
    public static class UpdatePersonaje
    {
        public record UpdatePersonajeRequest(string Nombre);

        public sealed class Handler
        {
            private readonly CreanovelDbContext _context;

            public Handler(CreanovelDbContext context)
            {
                _context = context;
            }

            public async Task HandleAsync(Guid personajeId, UpdatePersonajeRequest request, CancellationToken ct)
            {
                var personaje = await _context.Personajes.FindAsync(new object[] { personajeId }, ct);

                if (personaje == null)
                {
                    throw new ExceptionHandler(HttpStatusCode.NotFound, new { message = "Personaje no encontrado" });
                }

                personaje.Nombre = request.Nombre ?? personaje.Nombre;

                var result = await _context.SaveChangesAsync(ct);

                if (result <= 0)
                {
                    throw new ExceptionHandler(HttpStatusCode.BadRequest, new { message = "No se actualizó el personaje" });
                }
            }
        }

        public static class Endpoint
        {
            public static void Map(IEndpointRouteBuilder app) =>
                app.MapPatch("api/personajes/{personajeId:guid}", async (Guid personajeId, [FromBody] UpdatePersonajeRequest request, Handler handler, CancellationToken ct) =>
                    {
                        await handler.HandleAsync(personajeId, request, ct);
                        return Results.NoContent();
                    })
                    .RequireAuthorization()
                    .WithTags("Personajes");
        }
    }
}
