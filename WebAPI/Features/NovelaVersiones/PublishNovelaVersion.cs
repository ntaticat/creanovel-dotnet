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
using Microsoft.EntityFrameworkCore;
using Persistence;
using WebAPI.Infrastructure.Extensions;

namespace WebAPI.Features.NovelaVersiones
{
    public static class PublishNovelaVersion
    {
        public sealed class Handler
        {
            private readonly CreanovelDbContext _context;

            public Handler(CreanovelDbContext context)
            {
                _context = context;
            }

            public async Task HandleAsync(Guid novelaVersionId, Guid? actingUsuarioId, CancellationToken ct)
            {
                var novelaVersion = await _context.NovelaVersiones
                    .Include(nv => nv.Novela)
                    .FirstOrDefaultAsync(nv => nv.NovelaVersionId == novelaVersionId, ct);

                if (novelaVersion == null)
                {
                    throw new ExceptionHandler(HttpStatusCode.NotFound, new { message = "NovelaVersion no encontrada" });
                }

                OwnershipGuard.EnsureNovelaOwner(actingUsuarioId, novelaVersion.Novela.UsuarioCreadorId);

                if (!novelaVersion.EsBorrador)
                {
                    throw new ExceptionHandler(HttpStatusCode.BadRequest, new { message = "Solo se puede publicar una versión en borrador" });
                }

                var validacion = await VersionValidator.ValidarAsync(_context, novelaVersionId, ct);

                if (!validacion.Valida)
                {
                    throw new ExceptionHandler(HttpStatusCode.BadRequest, new
                    {
                        message = "La versión tiene errores y no se puede publicar",
                        errores = validacion.Errores.Select(e => new { codigo = e.Codigo, mensaje = e.Mensaje, recursoId = e.RecursoId }),
                        advertencias = validacion.Advertencias.Select(e => new { codigo = e.Codigo, mensaje = e.Mensaje, recursoId = e.RecursoId })
                    });
                }

                await using var transaction = await _context.Database.BeginTransactionAsync(ct);

                try
                {
                    var versionesPublicadasActuales = await _context.NovelaVersiones
                        .Where(nv => nv.NovelaId == novelaVersion.NovelaId && nv.Disponible && nv.NovelaVersionId != novelaVersion.NovelaVersionId)
                        .ToListAsync(ct);

                    foreach (var version in versionesPublicadasActuales)
                    {
                        version.Disponible = false;
                    }

                    novelaVersion.Disponible = true;
                    novelaVersion.EsBorrador = false;

                    var result = await _context.SaveChangesAsync(ct);

                    if (result <= 0)
                    {
                        throw new ExceptionHandler(HttpStatusCode.BadRequest, new { message = "No se pudo publicar la versión" });
                    }

                    await transaction.CommitAsync(ct);
                }
                catch (Exception)
                {
                    await transaction.RollbackAsync(ct);
                    throw new ExceptionHandler(HttpStatusCode.BadRequest, new { message = "No se pudo publicar la versión" });
                }
            }
        }

        public static class Endpoint
        {
            public static void Map(IEndpointRouteBuilder app) =>
                app.MapPost("api/novela-versiones/{novelaVersionId:guid}/publicar", async (Guid novelaVersionId, Handler handler, ClaimsPrincipal user, CancellationToken ct) =>
                    {
                        await handler.HandleAsync(novelaVersionId, user.GetActingUsuarioId(), ct);
                        return Results.NoContent();
                    })
                    .RequireAuthorization()
                    .WithTags("NovelaVersiones");
        }
    }
}
