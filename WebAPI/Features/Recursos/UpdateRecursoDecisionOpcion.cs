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

namespace WebAPI.Features.Recursos
{
    public static class UpdateRecursoDecisionOpcion
    {
        public record UpdateRecursoDecisionOpcionRequest(
            string OpcionMensaje,
            Guid? SiguienteRecursoId,
            int? Orden,
            JsonElement? Condicion,
            string CondicionModo,
            JsonElement? Efectos,
            JsonElement? Region = null
        );

        public sealed class Handler
        {
            private readonly CreanovelDbContext _context;

            public Handler(CreanovelDbContext context)
            {
                _context = context;
            }

            public async Task HandleAsync(Guid opcionId, UpdateRecursoDecisionOpcionRequest request, Guid? actingUsuarioId, CancellationToken ct)
            {
                var opcion = await _context.RecursoDecisionOpciones.FindAsync(new object[] { opcionId }, ct);

                if (opcion == null)
                {
                    throw new ExceptionHandler(HttpStatusCode.NotFound, new { message = "Opción no encontrada" });
                }

                await OwnershipGuard.EnsureRecursoOwnerAsync(_context, actingUsuarioId, opcion.RecursoDecisionId, ct);

                var novelaVersionId = await _context.Recursos
                    .Where(r => r.RecursoId == opcion.RecursoDecisionId)
                    .Select(r => r.Escena.NovelaVersionId)
                    .FirstAsync(ct);

                await RecursoEnlaces.EnsureMismaVersionAsync(_context, novelaVersionId, request.SiguienteRecursoId, ct);

                var ctx = await VariablesLoader.PorRecursoAsync(_context, opcion.RecursoDecisionId, ct);
                var mensaje = request.OpcionMensaje ?? opcion.OpcionMensaje;
                var (condicion, efectos, region) = OpcionMotor.Validar(opcion.Tipo, mensaje, request.Condicion, request.Efectos, request.Region, request.CondicionModo, ctx);

                opcion.OpcionMensaje = mensaje;
                opcion.Orden = request.Orden ?? opcion.Orden;
                opcion.CondicionModo = request.CondicionModo ?? opcion.CondicionModo;
                // Replaced, not merged: null means "Ninguno (fin de rama)" / "sin condición" / "sin efectos".
                // See UpdateRecursoDecision.
                opcion.SiguienteRecursoId = request.SiguienteRecursoId;
                opcion.Condicion = condicion;
                opcion.Efectos = efectos;
                opcion.Region = region;

                // A PATCH that leaves the row unchanged saves 0 rows; that is not an error.
                await _context.SaveChangesAsync(ct);
            }
        }

        public static class Endpoint
        {
            public static void Map(IEndpointRouteBuilder app) =>
                app.MapPatch("api/recursos/opciones/{opcionId:guid}", async (Guid opcionId, [FromBody] UpdateRecursoDecisionOpcionRequest request, Handler handler, ClaimsPrincipal user, CancellationToken ct) =>
                    {
                        await handler.HandleAsync(opcionId, request, user.GetActingUsuarioId(), ct);
                        return Results.NoContent();
                    })
                    .RequireAuthorization()
                    .WithTags("Recursos");
        }
    }
}
