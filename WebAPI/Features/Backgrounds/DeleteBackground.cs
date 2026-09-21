using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Application.Handlers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Persistence;

namespace WebAPI.Features.Backgrounds
{
    public static class DeleteBackground
    {
        public sealed class Handler
        {
            private readonly CreanovelDbContext _context;

            public Handler(CreanovelDbContext context)
            {
                _context = context;
            }

            public async Task HandleAsync(Guid backgroundId, CancellationToken ct)
            {
                var background = await _context.Backgrounds.FindAsync(new object[] { backgroundId }, ct);

                if (background == null)
                {
                    throw new ExceptionHandler(HttpStatusCode.NotFound, new { message = "Background no encontrado" });
                }

                _context.Backgrounds.Remove(background);
                var result = await _context.SaveChangesAsync(ct);

                if (result <= 0)
                {
                    throw new ExceptionHandler(HttpStatusCode.BadRequest, new { message = "No se eliminó el background" });
                }
            }
        }

        public static class Endpoint
        {
            public static void Map(IEndpointRouteBuilder app) =>
                app.MapDelete("api/backgrounds/{backgroundId:guid}", async (Guid backgroundId, Handler handler, CancellationToken ct) =>
                    {
                        await handler.HandleAsync(backgroundId, ct);
                        return Results.NoContent();
                    })
                    .RequireAuthorization()
                    .WithTags("Backgrounds");
        }
    }
}
