using System;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Application.Handlers;
using Application.Motor;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Persistence;
using WebAPI.Infrastructure.Extensions;

namespace WebAPI.Features.NovelaVersiones
{
    // Informe de problemas de una versión (los mismos que bloquean la publicación), para mostrar en el editor.
    public static class ValidateNovelaVersion
    {
        public sealed class Handler
        {
            private readonly CreanovelDbContext _context;

            public Handler(CreanovelDbContext context)
            {
                _context = context;
            }

            public async Task<ValidacionVersionDto> HandleAsync(Guid novelaVersionId, Guid? actingUsuarioId, CancellationToken ct)
            {
                await OwnershipGuard.EnsureNovelaVersionOwnerAsync(_context, actingUsuarioId, novelaVersionId, ct);

                var resultado = await VersionValidator.ValidarAsync(_context, novelaVersionId, ct);

                if (resultado.Errores.Any(e => e.Codigo == "version_no_encontrada"))
                {
                    throw new ExceptionHandler(HttpStatusCode.NotFound, new { message = "NovelaVersion no encontrada" });
                }

                return resultado;
            }
        }

        public static class Endpoint
        {
            public static void Map(IEndpointRouteBuilder app) =>
                app.MapGet("api/novela-versiones/{novelaVersionId:guid}/validacion", async (Guid novelaVersionId, Handler handler, ClaimsPrincipal user, CancellationToken ct) =>
                        Results.Ok(await handler.HandleAsync(novelaVersionId, user.GetActingUsuarioId(), ct)))
                    .RequireAuthorization()
                    .WithTags("NovelaVersiones");
        }
    }
}
