using System;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Application.Handlers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace WebAPI.Features.Usuarios
{
    public static class GetUsuarioById
    {
        public sealed class Handler
        {
            private readonly CreanovelDbContext _context;

            public Handler(CreanovelDbContext context)
            {
                _context = context;
            }

            public async Task<UsuarioDto> HandleAsync(Guid usuarioId, CancellationToken ct)
            {
                var usuario = await _context.Usuarios
                    .Include(u => u.Lecturas)
                    .ThenInclude(l => l.Recursos)
                    .AsSplitQuery()
                    .Include(u => u.NovelasCreadas)
                    .ThenInclude(n => n.NovelaVersiones)
                    .AsSplitQuery()
                    .FirstOrDefaultAsync(u => u.Id == usuarioId, ct);

                if (usuario == null)
                {
                    throw new ExceptionHandler(HttpStatusCode.NotFound, new { message = "Usuario no encontrado" });
                }

                return new UsuarioDto(
                    usuario.Id,
                    usuario.Nombre,
                    usuario.Email,
                    usuario.UserName,
                    usuario.Lecturas.Select(l => new LecturaDto(
                        l.LecturaId,
                        l.NovelaRegistrosId,
                        l.UsuarioPropietarioId,
                        l.Recursos.Select(r => new LecturaRecursoDto(r.LecturaId, r.RecursoId, r.RecursoOrder)).ToList(),
                        Application.Motor.MotorJson.ParseNullable(l.Estado),
                        l.RecursoActualId,
                        l.NovelaVersionId
                    )).ToList(),
                    usuario.NovelasCreadas.Select(n => new NovelaResumenDto(
                        n.NovelaId, n.Titulo, n.Descripcion, n.Disponible, n.PortadaImagenUrl, n.UsuarioCreadorId, n.NovelaVersiones.Count
                    )).ToList()
                );
            }
        }

        public static class Endpoint
        {
            public static void Map(IEndpointRouteBuilder app) =>
                app.MapGet("api/usuarios/{usuarioId:guid}", async (Guid usuarioId, Handler handler, CancellationToken ct) =>
                        Results.Ok(await handler.HandleAsync(usuarioId, ct)))
                    .RequireAuthorization()
                    .WithTags("Usuarios");
        }
    }
}
