using System;
using System.Collections.Generic;
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
using WebAPI.Features.Backgrounds;
using WebAPI.Features.Personajes;

namespace WebAPI.Features.Novelas
{
    public static class GetNovelaById
    {
        public sealed class Handler
        {
            private readonly CreanovelDbContext _context;

            public Handler(CreanovelDbContext context)
            {
                _context = context;
            }

            public async Task<NovelaPopulatedDto> HandleAsync(Guid novelaId, bool versiones, bool personajes, bool backgrounds, CancellationToken ct)
            {
                var novela = await _context.Novelas.FindAsync(new object[] { novelaId }, ct);

                if (novela == null)
                {
                    throw new ExceptionHandler(HttpStatusCode.NotFound, new { message = "Novela no encontrada" });
                }

                await _context.Entry(novela).Reference(n => n.UsuarioCreador).LoadAsync(ct);

                if (versiones)
                {
                    await _context.Entry(novela).Collection(n => n.NovelaVersiones).LoadAsync(ct);
                }

                if (personajes)
                {
                    await _context.Entry(novela)
                        .Collection(n => n.Personajes)
                        .Query()
                        .Include(np => np.Personaje)
                        .ThenInclude(p => p.Sprites)
                        .LoadAsync(ct);
                }

                if (backgrounds)
                {
                    await _context.Entry(novela)
                        .Collection(n => n.Backgrounds)
                        .Query()
                        .Include(nb => nb.Background)
                        .ThenInclude(b => b.Sprites)
                        .LoadAsync(ct);
                }

                return new NovelaPopulatedDto(
                    novela.NovelaId,
                    novela.Titulo,
                    novela.Descripcion,
                    novela.Disponible,
                    novela.PortadaImagenUrl,
                    novela.UsuarioCreadorId,
                    novela.UsuarioCreador?.Nombre,
                    (novela.NovelaVersiones ?? new List<Domain.Models.NovelaVersion>())
                        .Select(v => new NovelaVersionResumenDto(v.NovelaVersionId, v.NumeroVersion, v.Disponible, v.EsBorrador, v.NovelaId))
                        .ToList(),
                    (novela.Personajes ?? new List<Domain.Models.NovelaPersonaje>())
                        .Select(np => np.Personaje)
                        .Select(p => new PersonajeDto(
                            p.PersonajeId, p.Nombre,
                            p.Sprites.Select(s => new PersonajeSpriteDto(s.PersonajeSpriteId, s.Nombre, s.DireccionImagen)).ToList()
                        ))
                        .ToList(),
                    (novela.Backgrounds ?? new List<Domain.Models.NovelaBackground>())
                        .Select(nb => nb.Background)
                        .Select(b => new BackgroundDto(
                            b.BackgroundId, b.Descripcion,
                            b.Sprites.Select(s => new BackgroundSpriteDto(s.BackgroundSpriteId, s.Nombre, s.DireccionImagen)).ToList()
                        ))
                        .ToList()
                );
            }
        }

        public static class Endpoint
        {
            public static void Map(IEndpointRouteBuilder app) =>
                app.MapGet("api/novelas/{novelaId:guid}", async (Guid novelaId, Handler handler, CancellationToken ct, bool versiones = false, bool personajes = false, bool backgrounds = false) =>
                        Results.Ok(await handler.HandleAsync(novelaId, versiones, personajes, backgrounds, ct)))
                    .AllowAnonymous()
                    .WithTags("Novelas");
        }
    }
}
