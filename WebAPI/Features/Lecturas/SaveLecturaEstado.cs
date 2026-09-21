using System;
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

namespace WebAPI.Features.Lecturas
{
    // Guarda la partida: el estado del motor (variables, ...) y el recurso donde quedó el jugador.
    // El servidor no interpreta el estado; solo lo acota. El motor corre en el cliente (juego de un solo jugador).
    public static class SaveLecturaEstado
    {
        public const int MaxBytesEstado = 64 * 1024;

        public record SaveLecturaEstadoRequest(JsonElement Estado, Guid? RecursoActualId, Guid? NovelaVersionId);

        public sealed class Handler
        {
            private readonly CreanovelDbContext _context;

            public Handler(CreanovelDbContext context)
            {
                _context = context;
            }

            public async Task HandleAsync(Guid lecturaId, SaveLecturaEstadoRequest request, Guid? actingUsuarioId, CancellationToken ct)
            {
                var lectura = await _context.Lecturas.FirstOrDefaultAsync(l => l.LecturaId == lecturaId, ct);

                if (lectura == null)
                {
                    throw new ExceptionHandler(HttpStatusCode.NotFound, new { message = "Lectura no encontrada" });
                }

                await OwnershipGuard.EnsureLecturaOwnerAsync(_context, actingUsuarioId, lecturaId, ct);

                if (request.Estado.ValueKind != JsonValueKind.Object)
                {
                    throw new ExceptionHandler(HttpStatusCode.BadRequest, new { message = "El estado debe ser un objeto" });
                }

                var estado = request.Estado.GetRawText();

                if (estado.Length > MaxBytesEstado)
                {
                    throw new ExceptionHandler(HttpStatusCode.BadRequest, new { message = "El estado guardado es demasiado grande" });
                }

                lectura.Estado = estado;
                lectura.RecursoActualId = request.RecursoActualId;
                lectura.NovelaVersionId = request.NovelaVersionId ?? lectura.NovelaVersionId;

                await _context.SaveChangesAsync(ct);
            }
        }

        public static class Endpoint
        {
            public static void Map(IEndpointRouteBuilder app) =>
                app.MapPut("api/lecturas/{lecturaId:guid}/estado", async (Guid lecturaId, [FromBody] SaveLecturaEstadoRequest request, Handler handler, ClaimsPrincipal user, CancellationToken ct) =>
                    {
                        await handler.HandleAsync(lecturaId, request, user.GetActingUsuarioId(), ct);
                        return Results.NoContent();
                    })
                    .RequireAuthorization()
                    .WithTags("Lecturas");
        }
    }
}
