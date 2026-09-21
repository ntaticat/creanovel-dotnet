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
    public static class UpdateBackground
    {
        public record UpdateBackgroundRequest(string Descripcion);

        public sealed class Handler
        {
            private readonly CreanovelDbContext _context;

            public Handler(CreanovelDbContext context)
            {
                _context = context;
            }

            public async Task HandleAsync(Guid backgroundId, UpdateBackgroundRequest request, CancellationToken ct)
            {
                var background = await _context.Backgrounds.FindAsync(new object[] { backgroundId }, ct);

                if (background == null)
                {
                    throw new ExceptionHandler(HttpStatusCode.NotFound, new { message = "Background no encontrado" });
                }

                background.Descripcion = request.Descripcion ?? background.Descripcion;

                var result = await _context.SaveChangesAsync(ct);

                if (result <= 0)
                {
                    throw new ExceptionHandler(HttpStatusCode.BadRequest, new { message = "No se actualizó el background" });
                }
            }
        }

        public static class Endpoint
        {
            public static void Map(IEndpointRouteBuilder app) =>
                app.MapPatch("api/backgrounds/{backgroundId:guid}", async (Guid backgroundId, [FromBody] UpdateBackgroundRequest request, Handler handler, CancellationToken ct) =>
                    {
                        await handler.HandleAsync(backgroundId, request, ct);
                        return Results.NoContent();
                    })
                    .RequireAuthorization()
                    .WithTags("Backgrounds");
        }
    }
}
