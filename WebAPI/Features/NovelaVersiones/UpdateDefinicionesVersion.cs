using System;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Application.Handlers;
using Application.Motor;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Persistence;
using WebAPI.Infrastructure.Extensions;

namespace WebAPI.Features.NovelaVersiones
{
    // Reemplaza el catálogo del motor (variables, ...) de un borrador. El editor envía siempre el catálogo completo.
    public static class UpdateDefinicionesVersion
    {
        public sealed class Handler
        {
            private readonly CreanovelDbContext _context;

            public Handler(CreanovelDbContext context)
            {
                _context = context;
            }

            public async Task<JsonElement?> HandleAsync(Guid novelaVersionId, DefinicionesJuego request, Guid? actingUsuarioId, CancellationToken ct)
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
                    throw new ExceptionHandler(HttpStatusCode.BadRequest, new { message = "Solo se pueden editar las definiciones de una versión en borrador" });
                }

                var (normalizadas, errores) = MotorJson.NormalizarYValidar(request);

                // El fondo de una ubicación debe ser un sprite de un fondo asociado a esta novela.
                var spritesUsados = normalizadas.Ubicaciones
                    .Where(u => u.BackgroundSpriteId.HasValue)
                    .Select(u => u.BackgroundSpriteId.Value)
                    .Distinct()
                    .ToList();

                if (spritesUsados.Count > 0)
                {
                    var validos = await _context.BackgroundSprites
                        .Where(s => spritesUsados.Contains(s.BackgroundSpriteId)
                            && _context.NovelaBackground.Any(nb => nb.NovelaId == novelaVersion.NovelaId && nb.BackgroundId == s.BackgroundId))
                        .CountAsync(ct);

                    if (validos != spritesUsados.Count)
                    {
                        errores.Add("Una ubicación usa un fondo que no pertenece a esta novela");
                    }
                }

                // Un objeto solo puede llevar a un nodo de esta misma versión.
                var destinos = UsoObjetoDestinos.De(normalizadas).ToList();

                if (destinos.Count > 0)
                {
                    var ids = destinos.Select(d => d.RecursoId).Distinct().ToList();
                    var existentes = await _context.Recursos
                        .Where(r => ids.Contains(r.RecursoId) && r.Escena.NovelaVersionId == novelaVersionId)
                        .Select(r => r.RecursoId)
                        .ToListAsync(ct);

                    errores.AddRange(destinos
                        .Where(d => !existentes.Contains(d.RecursoId))
                        .Select(d => $"Objeto '{d.ObjetoId}' (uso): el destino no es un nodo de esta versión"));
                }

                if (errores.Count > 0)
                {
                    throw new ExceptionHandler(HttpStatusCode.BadRequest, new { message = "Las definiciones no son válidas", errores });
                }

                novelaVersion.Definiciones = MotorJson.Serializar(normalizadas);
                await _context.SaveChangesAsync(ct);

                return MotorJson.ParseNullable(novelaVersion.Definiciones);
            }
        }

        public static class Endpoint
        {
            public static void Map(IEndpointRouteBuilder app) =>
                app.MapPut("api/novela-versiones/{novelaVersionId:guid}/definiciones", async (Guid novelaVersionId, [FromBody] DefinicionesJuego request, Handler handler, ClaimsPrincipal user, CancellationToken ct) =>
                        Results.Ok(await handler.HandleAsync(novelaVersionId, request, user.GetActingUsuarioId(), ct)))
                    .RequireAuthorization()
                    .WithTags("NovelaVersiones");
        }
    }
}
