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
    // Endpoint genérico para los tipos con contenido validado por el motor (Evalua, Asigna).
    // Los tipos anteriores (conversación, decisión, entrada) mantienen sus endpoints propios.
    public static class CreateRecurso
    {
        public record CreateRecursoRequest(
            Guid EscenaId,
            string TipoRecurso,
            bool PrimerRecurso,
            bool UltimoRecurso,
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

            public async Task<Guid> HandleAsync(CreateRecursoRequest request, Guid? actingUsuarioId, CancellationToken ct)
            {
                if (!RecursoTipos.Genericos.Contains(request.TipoRecurso))
                {
                    throw new ExceptionHandler(HttpStatusCode.BadRequest, new { message = $"Tipo de recurso no soportado por este endpoint: {request.TipoRecurso}" });
                }

                await OwnershipGuard.EnsureEscenaOwnerAsync(_context, actingUsuarioId, request.EscenaId, ct);

                var versionId = await _context.Escenas
                    .Where(e => e.EscenaId == request.EscenaId)
                    .Select(e => (Guid?)e.NovelaVersionId)
                    .FirstOrDefaultAsync(ct);

                if (versionId == null)
                {
                    throw new ExceptionHandler(HttpStatusCode.NotFound, new { message = "Escena no encontrada" });
                }

                await RecursoEnlaces.EnsureMismaVersionAsync(_context, versionId.Value, request.SiguienteRecursoId, ct);

                var ctx = await VariablesLoader.PorEscenaAsync(_context, request.EscenaId, ct);
                var errores = new List<string>();
                var contenido = RecursoContenidoNormalizador.Normalizar(request.TipoRecurso, request.Contenido, ctx, errores);

                if (errores.Count > 0)
                {
                    throw new ExceptionHandler(HttpStatusCode.BadRequest, new { message = "El contenido del recurso no es válido", errores });
                }

                var personajes = await PersonajesEnEscena.NormalizarAsync(_context, request.Personajes, ct);


                var recurso = new Recurso
                {
                    EscenaId = request.EscenaId,
                    PrimerRecurso = request.PrimerRecurso,
                    UltimoRecurso = request.UltimoRecurso,
                    TipoRecurso = request.TipoRecurso,
                    // Explora y Juega no tienen "siguiente": sus salidas son las zonas y los resultados.
                    SiguienteRecursoId = RecursoTipos.SinSiguiente(request.TipoRecurso) ? null : request.SiguienteRecursoId,
                    Personajes = personajes,
                    BackgroundSpriteId = request.BackgroundSpriteId,
                    Contenido = contenido
                };

                await _context.Recursos.AddAsync(recurso, ct);
                var result = await _context.SaveChangesAsync(ct);

                if (result > 0)
                {
                    return recurso.RecursoId;
                }

                throw new ExceptionHandler(HttpStatusCode.BadRequest, new { message = "No se pudo registrar el recurso" });
            }
        }

        public static class Endpoint
        {
            public static void Map(IEndpointRouteBuilder app) =>
                app.MapPost("api/recursos", async ([FromBody] CreateRecursoRequest request, Handler handler, ClaimsPrincipal user, CancellationToken ct) =>
                        Results.Ok(await handler.HandleAsync(request, user.GetActingUsuarioId(), ct)))
                    .RequireAuthorization()
                    .WithTags("Recursos");
        }
    }
}
