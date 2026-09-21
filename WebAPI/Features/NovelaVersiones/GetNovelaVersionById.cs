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
using WebAPI.Features.Escenas;
using WebAPI.Features.Recursos;

namespace WebAPI.Features.NovelaVersiones
{
    public static class GetNovelaVersionById
    {
        public sealed class Handler
        {
            private readonly CreanovelDbContext _context;

            public Handler(CreanovelDbContext context)
            {
                _context = context;
            }

            public async Task<NovelaVersionPopulatedDto> HandleAsync(Guid novelaVersionId, CancellationToken ct)
            {
                var novelaVersion = await _context.NovelaVersiones
                    .Include(nv => nv.Escenas)
                    .ThenInclude(e => e.Recursos)
                    .FirstOrDefaultAsync(nv => nv.NovelaVersionId == novelaVersionId, ct);

                if (novelaVersion == null)
                {
                    throw new ExceptionHandler(HttpStatusCode.NotFound, new { message = "NovelaVersion no encontrada" });
                }

                var decisionIds = novelaVersion.Escenas
                    .SelectMany(e => e.Recursos)
                    .Where(r => RecursoTipos.TieneOpciones(r.TipoRecurso))
                    .Select(r => r.RecursoId)
                    .ToList();

                if (decisionIds.Any())
                {
                    await _context.RecursoDecisionOpciones
                        .Where(o => decisionIds.Contains(o.RecursoDecisionId))
                        .LoadAsync(ct);
                }

                return new NovelaVersionPopulatedDto(
                    novelaVersion.NovelaVersionId,
                    novelaVersion.NumeroVersion,
                    novelaVersion.Disponible,
                    novelaVersion.EsBorrador,
                    novelaVersion.NovelaId,
                    novelaVersion.Escenas.Select(EscenaMapping.ToDto).ToList(),
                    Application.Motor.MotorJson.ParseNullable(novelaVersion.Definiciones)
                );
            }
        }

        public static class Endpoint
        {
            public static void Map(IEndpointRouteBuilder app) =>
                app.MapGet("api/novela-versiones/{novelaVersionId:guid}", async (Guid novelaVersionId, Handler handler, CancellationToken ct) =>
                        Results.Ok(await handler.HandleAsync(novelaVersionId, ct)))
                    .AllowAnonymous()
                    .WithTags("NovelaVersiones");
        }
    }
}
