using System;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Application.Handlers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Persistence;
using WebAPI.Infrastructure.Extensions;

namespace WebAPI.Features.Recursos
{
    public static class SetNextRecurso
    {
        public sealed class Handler
        {
            private readonly CreanovelDbContext _context;

            public Handler(CreanovelDbContext context)
            {
                _context = context;
            }

            public async Task HandleAsync(Guid recursoId, Guid siguienteRecursoId, Guid? actingUsuarioId, CancellationToken ct)
            {
                var recurso = await _context.Recursos
                    .FirstOrDefaultAsync(r => r.RecursoId == recursoId && r.TipoRecurso != RecursoTipos.Decision, ct);

                if (recurso == null)
                {
                    throw new ExceptionHandler(HttpStatusCode.NotFound, new { message = "Recurso no encontrado" });
                }

                await OwnershipGuard.EnsureRecursoOwnerAsync(_context, actingUsuarioId, recurso.RecursoId, ct);

                recurso.SiguienteRecursoId = siguienteRecursoId;
                var result = await _context.SaveChangesAsync(ct);

                if (result <= 0)
                {
                    throw new ExceptionHandler(HttpStatusCode.BadRequest, new { message = "No se pudo asignar el siguiente recurso al recurso correspondiente" });
                }
            }
        }

        public static class Endpoint
        {
            public static void Map(IEndpointRouteBuilder app) =>
                app.MapPost("api/recursos/{recursoId:guid}/next/{siguienteRecursoId:guid}", async (Guid recursoId, Guid siguienteRecursoId, Handler handler, ClaimsPrincipal user, CancellationToken ct) =>
                    {
                        await handler.HandleAsync(recursoId, siguienteRecursoId, user.GetActingUsuarioId(), ct);
                        return Results.NoContent();
                    })
                    .RequireAuthorization()
                    .WithTags("Recursos");
        }
    }
}
