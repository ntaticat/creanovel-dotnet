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
    public static class UpdateRecursoConversacion
    {
        public record UpdateRecursoConversacionRequest(
            string Mensaje,
            string AutorMensaje,
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

            public async Task HandleAsync(Guid recursoId, UpdateRecursoConversacionRequest request, Guid? actingUsuarioId, CancellationToken ct)
            {
                var recurso = await _context.Recursos
                    .FirstOrDefaultAsync(r => r.RecursoId == recursoId && r.TipoRecurso == RecursoTipos.Conversacion, ct);

                if (recurso == null)
                {
                    throw new ExceptionHandler(HttpStatusCode.NotFound, new { message = "Recurso no encontrado" });
                }

                await OwnershipGuard.EnsureRecursoOwnerAsync(_context, actingUsuarioId, recurso.RecursoId, ct);

                var contenido = JsonSerializer.Deserialize<ConversacionContenido>(recurso.Contenido);
                recurso.Contenido = JsonSerializer.Serialize(new ConversacionContenido(
                    request.Mensaje ?? contenido.Mensaje,
                    request.AutorMensaje
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
                app.MapPatch("api/recursos/conversacion/{recursoId:guid}", async (Guid recursoId, [FromBody] UpdateRecursoConversacionRequest request, Handler handler, ClaimsPrincipal user, CancellationToken ct) =>
                    {
                        await handler.HandleAsync(recursoId, request, user.GetActingUsuarioId(), ct);
                        return Results.NoContent();
                    })
                    .RequireAuthorization()
                    .WithTags("Recursos");
        }
    }
}
