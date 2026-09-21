using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Application.Handlers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace WebAPI.Features.Recursos
{
    public static class GetRecursoById
    {
        public sealed class Handler
        {
            private readonly CreanovelDbContext _context;

            public Handler(CreanovelDbContext context)
            {
                _context = context;
            }

            public async Task<object> HandleAsync(Guid recursoId, CancellationToken ct)
            {
                var recurso = await _context.Recursos.FindAsync(new object[] { recursoId }, ct);

                if (recurso == null)
                {
                    throw new ExceptionHandler(HttpStatusCode.NotFound, new { message = "Recurso no encontrado" });
                }

                if (RecursoTipos.TieneOpciones(recurso.TipoRecurso))
                {
                    await _context.Entry(recurso).Collection(r => r.Opciones).LoadAsync(ct);
                }

                return RecursoMapping.ToDto(recurso);
            }
        }

        public static class Endpoint
        {
            public static void Map(IEndpointRouteBuilder app) =>
                app.MapGet("api/recursos/{recursoId:guid}", async (Guid recursoId, Handler handler, CancellationToken ct) =>
                        Results.Ok(await handler.HandleAsync(recursoId, ct)))
                    .RequireAuthorization()
                    .WithTags("Recursos");
        }
    }
}
