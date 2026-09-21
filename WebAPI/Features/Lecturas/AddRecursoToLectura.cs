using System;
using System.Net;
using System.Security.Claims;
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
    public static class AddRecursoToLectura
    {
        public record AddRecursoToLecturaRequest(Guid LecturaId, Guid RecursoId, int RecursoOrder);

        public sealed class Handler
        {
            private readonly CreanovelDbContext _context;

            public Handler(CreanovelDbContext context)
            {
                _context = context;
            }

            public async Task HandleAsync(AddRecursoToLecturaRequest request, Guid? actingUsuarioId, CancellationToken ct)
            {
                await OwnershipGuard.EnsureLecturaOwnerAsync(_context, actingUsuarioId, request.LecturaId, ct);

                // Reintentos del cliente (los pasos se registran sin esperar respuesta) no deben fallar.
                var yaRegistrado = await _context.LecturaRecurso
                    .AnyAsync(lr => lr.LecturaId == request.LecturaId && lr.RecursoOrder == request.RecursoOrder, ct);

                if (yaRegistrado)
                {
                    return;
                }

                var lecturaRecurso = new Domain.Models.LecturaRecursos
                {
                    LecturaId = request.LecturaId,
                    RecursoId = request.RecursoId,
                    RecursoOrder = request.RecursoOrder
                };

                await _context.LecturaRecurso.AddAsync(lecturaRecurso, ct);
                var result = await _context.SaveChangesAsync(ct);

                if (result <= 0)
                {
                    throw new ExceptionHandler(HttpStatusCode.BadRequest, new { message = "No se pudo registrar la relación de Lectura y Recurso" });
                }
            }
        }

        public static class Endpoint
        {
            public static void Map(IEndpointRouteBuilder app) =>
                app.MapPost("api/lecturas/recursos", async ([FromBody] AddRecursoToLecturaRequest request, Handler handler, ClaimsPrincipal user, CancellationToken ct) =>
                    {
                        await handler.HandleAsync(request, user.GetActingUsuarioId(), ct);
                        return Results.NoContent();
                    })
                    .RequireAuthorization()
                    .WithTags("Lecturas");
        }
    }
}
