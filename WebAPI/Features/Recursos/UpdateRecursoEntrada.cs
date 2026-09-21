using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Application.Handlers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Persistence;
using WebAPI.Infrastructure.Extensions;

namespace WebAPI.Features.Recursos
{
    public static class UpdateRecursoEntrada
    {
        public record UpdateRecursoEntradaRequest(
            string Etiqueta,
            string Clave,
            string Valor,
            string Placeholder,
            Guid? SiguienteRecursoId,
            bool? PrimerRecurso,
            bool? UltimoRecurso,
            List<PersonajeEnEscenaDto> Personajes,
            Guid? BackgroundSpriteId
        );

        public sealed class Handler
        {
            private readonly CreanovelDbContext _context;

            public Handler(CreanovelDbContext context)
            {
                _context = context;
            }

            public async Task HandleAsync(Guid recursoId, UpdateRecursoEntradaRequest request, Guid? actingUsuarioId, CancellationToken ct)
            {
                var recurso = await _context.Recursos
                    .FirstOrDefaultAsync(r => r.RecursoId == recursoId && r.TipoRecurso == RecursoTipos.Entrada, ct);

                if (recurso == null)
                {
                    throw new ExceptionHandler(HttpStatusCode.NotFound, new { message = "Recurso no encontrado" });
                }

                await OwnershipGuard.EnsureRecursoOwnerAsync(_context, actingUsuarioId, recurso.RecursoId, ct);

                var contenido = JsonSerializer.Deserialize<EntradaContenido>(recurso.Contenido);
                recurso.Contenido = JsonSerializer.Serialize(new EntradaContenido(
                    request.Etiqueta ?? contenido.Etiqueta,
                    request.Clave ?? contenido.Clave,
                    request.Valor ?? contenido.Valor,
                    request.Placeholder ?? contenido.Placeholder
                ));
                recurso.PrimerRecurso = request.PrimerRecurso ?? recurso.PrimerRecurso;
                recurso.UltimoRecurso = request.UltimoRecurso ?? recurso.UltimoRecurso;
                // The editor always sends the full state, so these nullable fields are replaced, not merged:
                // an omitted/null value means "clear it" (a merge with ?? could never unset a sprite or link).
                recurso.SiguienteRecursoId = request.SiguienteRecursoId;
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
                app.MapPatch("api/recursos/entrada/{recursoId:guid}", async (Guid recursoId, [FromBody] UpdateRecursoEntradaRequest request, Handler handler, ClaimsPrincipal user, CancellationToken ct) =>
                    {
                        await handler.HandleAsync(recursoId, request, user.GetActingUsuarioId(), ct);
                        return Results.NoContent();
                    })
                    .RequireAuthorization()
                    .WithTags("Recursos");
        }
    }
}
