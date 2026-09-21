using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Application.Handlers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Persistence;
using WebAPI.Infrastructure.Extensions;

namespace WebAPI.Features.Escenas
{
    public static class CreateEscena
    {
        public record CreateEscenaRequest(string Identificador, bool PrimerEscena, bool UltimaEscena, IReadOnlyList<string> Etiquetas = null);

        public sealed class Handler
        {
            private readonly CreanovelDbContext _context;

            public Handler(CreanovelDbContext context)
            {
                _context = context;
            }

            public async Task HandleAsync(Guid novelaVersionId, CreateEscenaRequest request, Guid? actingUsuarioId, CancellationToken ct)
            {
                await OwnershipGuard.EnsureNovelaVersionOwnerAsync(_context, actingUsuarioId, novelaVersionId, ct);

                var etiquetas = EscenaEtiquetas.Normalizar(request.Etiquetas);

                var escena = new Domain.Models.Escena
                {
                    Identificador = request.Identificador,
                    NovelaVersionId = novelaVersionId,
                    PrimerEscena = request.PrimerEscena,
                    UltimaEscena = request.UltimaEscena,
                    Etiquetas = etiquetas
                };

                await _context.Escenas.AddAsync(escena, ct);
                var result = await _context.SaveChangesAsync(ct);

                if (result <= 0)
                {
                    throw new ExceptionHandler(HttpStatusCode.BadRequest, new { message = "No se pudo registrar la escena" });
                }
            }
        }

        public static class Endpoint
        {
            public static void Map(IEndpointRouteBuilder app) =>
                app.MapPost("api/novela-versiones/{novelaVersionId:guid}/escenas", async (Guid novelaVersionId, [FromBody] CreateEscenaRequest request, Handler handler, ClaimsPrincipal user, CancellationToken ct) =>
                    {
                        await handler.HandleAsync(novelaVersionId, request, user.GetActingUsuarioId(), ct);
                        return Results.NoContent();
                    })
                    .RequireAuthorization()
                    .WithTags("Escenas");
        }
    }
}
