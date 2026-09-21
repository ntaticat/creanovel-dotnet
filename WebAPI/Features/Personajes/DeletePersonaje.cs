using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Application.Handlers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Persistence;

namespace WebAPI.Features.Personajes
{
    public static class DeletePersonaje
    {
        public sealed class Handler
        {
            private readonly CreanovelDbContext _context;

            public Handler(CreanovelDbContext context)
            {
                _context = context;
            }

            public async Task HandleAsync(Guid personajeId, CancellationToken ct)
            {
                var personaje = await _context.Personajes.FindAsync(new object[] { personajeId }, ct);

                if (personaje == null)
                {
                    throw new ExceptionHandler(HttpStatusCode.NotFound, new { message = "Personaje no encontrado" });
                }

                _context.Personajes.Remove(personaje);
                var result = await _context.SaveChangesAsync(ct);

                if (result <= 0)
                {
                    throw new ExceptionHandler(HttpStatusCode.BadRequest, new { message = "No se eliminó el personaje" });
                }
            }
        }

        public static class Endpoint
        {
            public static void Map(IEndpointRouteBuilder app) =>
                app.MapDelete("api/personajes/{personajeId:guid}", async (Guid personajeId, Handler handler, CancellationToken ct) =>
                    {
                        await handler.HandleAsync(personajeId, ct);
                        return Results.NoContent();
                    })
                    .RequireAuthorization()
                    .WithTags("Personajes");
        }
    }
}
