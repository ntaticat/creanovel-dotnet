using System;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Application.Handlers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Persistence;
using WebAPI.Features.Recursos;

namespace WebAPI.Features.Escenas
{
    public static class GetEscenaById
    {
        public sealed class Handler
        {
            private readonly CreanovelDbContext _context;

            public Handler(CreanovelDbContext context)
            {
                _context = context;
            }

            public async Task<EscenaRecursosDto> HandleAsync(Guid escenaId, CancellationToken ct)
            {
                var escena = await _context.Escenas.Include(e => e.Recursos)
                    .FirstOrDefaultAsync(e => e.EscenaId == escenaId, ct);

                if (escena == null)
                {
                    throw new ExceptionHandler(HttpStatusCode.NotFound, new { message = "Escena no encontrada" });
                }

                var decisionIds = escena.Recursos.Where(r => RecursoTipos.TieneOpciones(r.TipoRecurso)).Select(r => r.RecursoId).ToList();

                if (decisionIds.Any())
                {
                    await _context.RecursoDecisionOpciones
                        .Where(o => decisionIds.Contains(o.RecursoDecisionId))
                        .LoadAsync(ct);
                }

                return EscenaMapping.ToDto(escena);
            }
        }

        public static class Endpoint
        {
            public static void Map(IEndpointRouteBuilder app) =>
                app.MapGet("api/escenas/{escenaId:guid}", async (Guid escenaId, Handler handler, CancellationToken ct) =>
                        Results.Ok(await handler.HandleAsync(escenaId, ct)))
                    .RequireAuthorization()
                    .WithTags("Escenas");
        }
    }
}
