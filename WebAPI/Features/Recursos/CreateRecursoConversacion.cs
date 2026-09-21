using System;
using System.Collections.Generic;
using System.Net;
using System.Security.Claims;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Application.Handlers;
using Domain.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Persistence;
using WebAPI.Infrastructure.Extensions;

namespace WebAPI.Features.Recursos
{
    public static class CreateRecursoConversacion
    {
        public record CreateRecursoConversacionRequest(
            Guid EscenaId,
            string TipoRecurso,
            bool PrimerRecurso,
            bool UltimoRecurso,
            string Mensaje,
            string AutorMensaje,
            Guid? SiguienteRecursoId,
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

            public async Task<Guid> HandleAsync(CreateRecursoConversacionRequest request, Guid? actingUsuarioId, CancellationToken ct)
            {
                await OwnershipGuard.EnsureEscenaOwnerAsync(_context, actingUsuarioId, request.EscenaId, ct);

                var personajes = await PersonajesEnEscena.NormalizarAsync(_context, request.Personajes, ct);


                var recurso = new Recurso
                {
                    EscenaId = request.EscenaId,
                    PrimerRecurso = request.PrimerRecurso,
                    UltimoRecurso = request.UltimoRecurso,
                    TipoRecurso = request.TipoRecurso,
                    SiguienteRecursoId = request.SiguienteRecursoId,
                    Personajes = personajes,
                    BackgroundSpriteId = request.BackgroundSpriteId,
                    Contenido = JsonSerializer.Serialize(new ConversacionContenido(request.Mensaje, request.AutorMensaje))
                };

                await _context.Recursos.AddAsync(recurso, ct);
                var result = await _context.SaveChangesAsync(ct);

                if (result > 0)
                {
                    return recurso.RecursoId;
                }

                throw new ExceptionHandler(HttpStatusCode.BadRequest, new { message = "No se pudo registrar el recurso conversacion" });
            }
        }

        public static class Endpoint
        {
            public static void Map(IEndpointRouteBuilder app) =>
                app.MapPost("api/recursos/conversacion", async ([FromBody] CreateRecursoConversacionRequest request, Handler handler, ClaimsPrincipal user, CancellationToken ct) =>
                        Results.Ok(await handler.HandleAsync(request, user.GetActingUsuarioId(), ct)))
                    .RequireAuthorization()
                    .WithTags("Recursos");
        }
    }
}
