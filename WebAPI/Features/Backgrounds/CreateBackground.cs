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
    public static class CreateBackground
    {
        public record CreateBackgroundRequest(string Descripcion);

        public sealed class Handler
        {
            private readonly CreanovelDbContext _context;

            public Handler(CreanovelDbContext context)
            {
                _context = context;
            }

            public async Task<Guid> HandleAsync(CreateBackgroundRequest request, CancellationToken ct)
            {
                var background = new Domain.Models.Background { Descripcion = request.Descripcion };
                await _context.Backgrounds.AddAsync(background, ct);
                var result = await _context.SaveChangesAsync(ct);

                if (result > 0)
                {
                    return background.BackgroundId;
                }

                throw new ExceptionHandler(HttpStatusCode.BadRequest, new { message = "No se pudo registrar el background" });
            }
        }

        public static class Endpoint
        {
            public static void Map(IEndpointRouteBuilder app) =>
                app.MapPost("api/backgrounds", async ([FromBody] CreateBackgroundRequest request, Handler handler, CancellationToken ct) =>
                        Results.Ok(await handler.HandleAsync(request, ct)))
                    .RequireAuthorization()
                    .WithTags("Backgrounds");
        }
    }
}
