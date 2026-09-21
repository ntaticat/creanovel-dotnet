using System;
using System.Collections.Generic;
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

namespace WebAPI.Features.Recursos
{
    public static class UpdateRecurso
    {
        public record UpdateRecursoRequest(
            bool? PrimerRecurso,
            bool? UltimoRecurso,
            Guid? SiguienteRecursoId,
            List<PersonajeEnEscenaDto> Personajes,
            Guid? BackgroundSpriteId,
            JsonElement? Contenido
        );

        public sealed class Handler
        {
            private readonly CreanovelDbContext _context;

            public Handler(CreanovelDbContext context)
            {
                _context = context;
            }

            public async Task HandleAsync(Guid recursoId, UpdateRecursoRequest request, Guid? actingUsuarioId, CancellationToken ct)
            {
                var recurso = await _context.Recursos
                    .Include(r => r.Escena)
                    .FirstOrDefaultAsync(r => r.RecursoId == recursoId && RecursoTipos.Genericos.Contains(r.TipoRecurso), ct);

                if (recurso == null)
                {
                    throw new ExceptionHandler(HttpStatusCode.NotFound, new { message = "Recurso no encontrado" });
                }

                await OwnershipGuard.EnsureRecursoOwnerAsync(_context, actingUsuarioId, recurso.RecursoId, ct);
                await RecursoEnlaces.EnsureMismaVersionAsync(_context, recurso.Escena.NovelaVersionId, request.SiguienteRecursoId, ct);

                if (request.Contenido != null)
                {
                    var ctx = await VariablesLoader.PorRecursoAsync(_context, recurso.RecursoId, ct);
                    var errores = new List<string>();
                    var contenido = RecursoContenidoNormalizador.Normalizar(recurso.TipoRecurso, request.Contenido, ctx, errores);

                    if (errores.Count > 0)
                    {
                        throw new ExceptionHandler(HttpStatusCode.BadRequest, new { message = "El contenido del recurso no es válido", errores });
                    }

                    recurso.Contenido = contenido;
                }

                recurso.PrimerRecurso = request.PrimerRecurso ?? recurso.PrimerRecurso;
                recurso.UltimoRecurso = request.UltimoRecurso ?? recurso.UltimoRecurso;
                // The editor always sends the full state, so these nullable fields are replaced, not merged:
                // an omitted/null value means "clear it" (for Evalua, a null SiguienteRecursoId clears the "sino").
                recurso.SiguienteRecursoId = RecursoTipos.SinSiguiente(recurso.TipoRecurso) ? null : request.SiguienteRecursoId;
                // Sin lista = sin cambios; una lista vacía quita todos los personajes.
                if (request.Personajes != null)
                {
                    recurso.Personajes = await PersonajesEnEscena.NormalizarAsync(_context, request.Personajes, ct);
                }
                recurso.BackgroundSpriteId = request.BackgroundSpriteId;

                // A PATCH that leaves the row unchanged saves 0 rows; that is not an error.
                await _context.SaveChangesAsync(ct);
            }
        }

        public static class Endpoint
        {
            public static void Map(IEndpointRouteBuilder app) =>
                app.MapPatch("api/recursos/{recursoId:guid}", async (Guid recursoId, [FromBody] UpdateRecursoRequest request, Handler handler, ClaimsPrincipal user, CancellationToken ct) =>
                    {
                        await handler.HandleAsync(recursoId, request, user.GetActingUsuarioId(), ct);
                        return Results.NoContent();
                    })
                    .RequireAuthorization()
                    .WithTags("Recursos");
        }
    }
}
