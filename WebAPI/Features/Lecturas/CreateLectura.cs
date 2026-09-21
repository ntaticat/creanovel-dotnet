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
using Persistence;
using WebAPI.Infrastructure.Extensions;

namespace WebAPI.Features.Lecturas
{
    public static class CreateLectura
    {
        public record CreateLecturaRequest(Guid NovelaRegistrosId, Guid UsuarioPropietarioId, Guid? NovelaVersionId);

        public sealed class Handler
        {
            private readonly CreanovelDbContext _context;

            public Handler(CreanovelDbContext context)
            {
                _context = context;
            }

            public async Task<Guid> HandleAsync(CreateLecturaRequest request, Guid? actingUsuarioId, CancellationToken ct)
            {
                var lectura = new Domain.Models.Lectura
                {
                    NovelaRegistrosId = request.NovelaRegistrosId,
                    // La lectura es siempre del usuario autenticado; el cuerpo solo cuenta sin sesión identificable.
                    UsuarioPropietarioId = actingUsuarioId ?? request.UsuarioPropietarioId,
                    NovelaVersionId = request.NovelaVersionId
                };

                await _context.Lecturas.AddAsync(lectura, ct);
                var result = await _context.SaveChangesAsync(ct);

                if (result > 0)
                {
                    return lectura.LecturaId;
                }

                throw new ExceptionHandler(HttpStatusCode.BadRequest, new { message = "No se pudo registrar la lectura" });
            }
        }

        public static class Endpoint
        {
            public static void Map(IEndpointRouteBuilder app) =>
                app.MapPost("api/lecturas", async ([FromBody] CreateLecturaRequest request, Handler handler, ClaimsPrincipal user, CancellationToken ct) =>
                        Results.Ok(await handler.HandleAsync(request, user.GetActingUsuarioId(), ct)))
                    .RequireAuthorization()
                    .WithTags("Lecturas");
        }
    }
}
