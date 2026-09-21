using System;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Application.Handlers;
using Application.Motor;
using Domain.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Persistence;
using WebAPI.Infrastructure.Extensions;

namespace WebAPI.Features.Recursos
{
    public static class AddRecursoDecisionOpcion
    {
        public record AddRecursoDecisionOpcionRequest(
            string OpcionMensaje,
            Guid? SiguienteRecursoId,
            Guid RecursoDecisionId,
            int? Orden,
            JsonElement? Condicion,
            string CondicionModo,
            JsonElement? Efectos,
            JsonElement? Region = null,
            // Solo para Juega: "exito" o "fallo". En los demás tipos lo determina el recurso dueño.
            string Tipo = null
        );

        public sealed class Handler
        {
            private readonly CreanovelDbContext _context;

            public Handler(CreanovelDbContext context)
            {
                _context = context;
            }

            public async Task HandleAsync(AddRecursoDecisionOpcionRequest request, Guid? actingUsuarioId, CancellationToken ct)
            {
                var recurso = await _context.Recursos
                    .Include(r => r.Escena)
                    // Comparación inline (no RecursoTipos.TieneOpciones): EF no traduce llamadas a métodos a SQL.
                    .FirstOrDefaultAsync(r => r.RecursoId == request.RecursoDecisionId
                        && (r.TipoRecurso == RecursoTipos.Decision || r.TipoRecurso == RecursoTipos.Evalua || r.TipoRecurso == RecursoTipos.Explora || r.TipoRecurso == RecursoTipos.Juega), ct);

                if (recurso == null)
                {
                    throw new ExceptionHandler(HttpStatusCode.NotFound, new { message = "El recurso de la opción no fue encontrado" });
                }

                await OwnershipGuard.EnsureRecursoOwnerAsync(_context, actingUsuarioId, recurso.RecursoId, ct);
                await RecursoEnlaces.EnsureMismaVersionAsync(_context, recurso.Escena.NovelaVersionId, request.SiguienteRecursoId, ct);

                var ctx = await VariablesLoader.PorRecursoAsync(_context, recurso.RecursoId, ct);
                // El tipo lo determina el recurso dueño: Selecciona tiene opciones, Evalúa ramas y Explora zonas.
                var tipo = recurso.TipoRecurso == RecursoTipos.Evalua ? RecursoSalidaTipos.Rama
                    : recurso.TipoRecurso == RecursoTipos.Explora ? RecursoSalidaTipos.Zona
                    : recurso.TipoRecurso == RecursoTipos.Juega ? request.Tipo
                    : RecursoSalidaTipos.Opcion;

                if (recurso.TipoRecurso == RecursoTipos.Juega)
                {
                    if (tipo != RecursoSalidaTipos.Exito && tipo != RecursoSalidaTipos.Fallo)
                    {
                        throw new ExceptionHandler(HttpStatusCode.BadRequest, new { message = "La salida de un minijuego debe ser 'exito' o 'fallo'" });
                    }

                    if (await _context.RecursoDecisionOpciones.AnyAsync(o => o.RecursoDecisionId == recurso.RecursoId && o.Tipo == tipo, ct))
                    {
                        throw new ExceptionHandler(HttpStatusCode.BadRequest, new { message = $"El minijuego ya tiene una salida '{tipo}'" });
                    }
                }
                var (condicion, efectos, region) = OpcionMotor.Validar(tipo, request.OpcionMensaje, request.Condicion, request.Efectos, request.Region, request.CondicionModo, ctx);

                var orden = request.Orden
                    ?? (await _context.RecursoDecisionOpciones
                        .Where(o => o.RecursoDecisionId == recurso.RecursoId)
                        .Select(o => (int?)o.Orden)
                        .MaxAsync(ct) + 1)
                    ?? 0;

                var opcion = new RecursoDecisionOpcion
                {
                    RecursoDecisionId = request.RecursoDecisionId,
                    SiguienteRecursoId = request.SiguienteRecursoId,
                    OpcionMensaje = request.OpcionMensaje,
                    Orden = orden,
                    Tipo = tipo,
                    Condicion = condicion,
                    CondicionModo = request.CondicionModo ?? CondicionModos.Ocultar,
                    Efectos = efectos,
                    Region = region
                };

                await _context.RecursoDecisionOpciones.AddAsync(opcion, ct);
                var result = await _context.SaveChangesAsync(ct);

                if (result <= 0)
                {
                    throw new ExceptionHandler(HttpStatusCode.BadRequest, new { message = "No se pudo agregar la opción al recurso correspondiente" });
                }
            }
        }

        public static class Endpoint
        {
            public static void Map(IEndpointRouteBuilder app) =>
                app.MapPost("api/recursos/opciones", async ([FromBody] AddRecursoDecisionOpcionRequest request, Handler handler, ClaimsPrincipal user, CancellationToken ct) =>
                    {
                        await handler.HandleAsync(request, user.GetActingUsuarioId(), ct);
                        return Results.NoContent();
                    })
                    .RequireAuthorization()
                    .WithTags("Recursos");
        }
    }
}
