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

namespace WebAPI.Features.NovelaVersiones
{
    public static class CreateNovelaVersion
    {
        public record CreateNovelaVersionRequest(string NumeroVersion, Guid NovelaId, bool? Disponible);

        public sealed class Handler
        {
            private readonly CreanovelDbContext _context;

            public Handler(CreanovelDbContext context)
            {
                _context = context;
            }

            public async Task HandleAsync(CreateNovelaVersionRequest request, CancellationToken ct)
            {
                var novelaVersion = new Domain.Models.NovelaVersion
                {
                    NovelaId = request.NovelaId,
                    NumeroVersion = request.NumeroVersion,
                    Disponible = request.Disponible ?? false
                };

                await _context.NovelaVersiones.AddAsync(novelaVersion, ct);
                var result = await _context.SaveChangesAsync(ct);

                if (result <= 0)
                {
                    throw new ExceptionHandler(HttpStatusCode.BadRequest, new { message = "No se pudo registrar la version de la novela" });
                }
            }
        }

        public static class Endpoint
        {
            public static void Map(IEndpointRouteBuilder app) =>
                app.MapPost("api/novela-versiones", async ([FromBody] CreateNovelaVersionRequest request, Handler handler, CancellationToken ct) =>
                    {
                        await handler.HandleAsync(request, ct);
                        return Results.NoContent();
                    })
                    .RequireAuthorization()
                    .WithTags("NovelaVersiones");
        }
    }
}
