using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace WebAPI.Features.Novelas
{
    public static class GetNovelas
    {
        public sealed class Handler
        {
            private readonly CreanovelDbContext _context;

            public Handler(CreanovelDbContext context)
            {
                _context = context;
            }

            public async Task<List<NovelaListItemDto>> HandleAsync(CancellationToken ct)
            {
                var novelas = await _context.Novelas.ToListAsync(ct);

                return novelas
                    .Select(n => new NovelaListItemDto(n.NovelaId, n.Titulo, n.Descripcion, n.Disponible, n.PortadaImagenUrl, n.UsuarioCreadorId))
                    .ToList();
            }
        }

        public static class Endpoint
        {
            public static void Map(IEndpointRouteBuilder app) =>
                app.MapGet("api/novelas", async (Handler handler, CancellationToken ct) =>
                        Results.Ok(await handler.HandleAsync(ct)))
                    .AllowAnonymous()
                    .WithTags("Novelas");
        }
    }
}
