using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace WebAPI.Features.NovelaVersiones
{
    public static class GetNovelaVersiones
    {
        public sealed class Handler
        {
            private readonly CreanovelDbContext _context;

            public Handler(CreanovelDbContext context)
            {
                _context = context;
            }

            public async Task<List<NovelaVersionDto>> HandleAsync(Guid? novelaId, CancellationToken ct)
            {
                var query = _context.NovelaVersiones.AsQueryable();

                if (novelaId != null)
                {
                    query = query.Where(nv => nv.NovelaId == novelaId);
                }

                var versiones = await query.ToListAsync(ct);

                return versiones
                    .Select(v => new NovelaVersionDto(v.NovelaVersionId, v.NumeroVersion, v.Disponible, v.EsBorrador, v.NovelaId))
                    .ToList();
            }
        }

        public static class Endpoint
        {
            public static void Map(IEndpointRouteBuilder app) =>
                app.MapGet("api/novela-versiones", async (Guid? novelaId, Handler handler, CancellationToken ct) =>
                        Results.Ok(await handler.HandleAsync(novelaId, ct)))
                    .AllowAnonymous()
                    .WithTags("NovelaVersiones");
        }
    }
}
