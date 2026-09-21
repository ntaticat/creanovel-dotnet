using System;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Handlers
{
    public static class OwnershipGuard
    {
        public static void EnsureNovelaOwner(Guid? actingUsuarioId, Guid? novelaOwnerId)
        {
            if (actingUsuarioId == null)
            {
                return;
            }

            if (novelaOwnerId == null || novelaOwnerId != actingUsuarioId)
            {
                throw new ExceptionHandler(HttpStatusCode.Forbidden, new { message = "No tienes permiso sobre esta novela" });
            }
        }

        public static async Task EnsureLecturaOwnerAsync(CreanovelDbContext context, Guid? actingUsuarioId, Guid lecturaId, CancellationToken cancellationToken = default)
        {
            if (actingUsuarioId == null)
            {
                return;
            }

            var ownerId = await context.Lecturas
                .Where(l => l.LecturaId == lecturaId)
                .Select(l => (Guid?)l.UsuarioPropietarioId)
                .FirstOrDefaultAsync(cancellationToken);

            if (ownerId == null || ownerId != actingUsuarioId)
            {
                throw new ExceptionHandler(HttpStatusCode.Forbidden, new { message = "No tienes permiso sobre esta lectura" });
            }
        }

        public static async Task EnsureNovelaVersionOwnerAsync(CreanovelDbContext context, Guid? actingUsuarioId, Guid novelaVersionId, CancellationToken cancellationToken = default)
        {
            if (actingUsuarioId == null)
            {
                return;
            }

            var ownerId = await context.NovelaVersiones
                .Where(nv => nv.NovelaVersionId == novelaVersionId)
                .Select(nv => nv.Novela.UsuarioCreadorId)
                .FirstOrDefaultAsync(cancellationToken);

            EnsureNovelaOwner(actingUsuarioId, ownerId);
        }

        public static async Task EnsureEscenaOwnerAsync(CreanovelDbContext context, Guid? actingUsuarioId, Guid escenaId, CancellationToken cancellationToken = default)
        {
            if (actingUsuarioId == null)
            {
                return;
            }

            var ownerId = await context.Escenas
                .Where(e => e.EscenaId == escenaId)
                .Select(e => e.NovelaVersion.Novela.UsuarioCreadorId)
                .FirstOrDefaultAsync(cancellationToken);

            EnsureNovelaOwner(actingUsuarioId, ownerId);
        }

        public static async Task EnsureRecursoOwnerAsync(CreanovelDbContext context, Guid? actingUsuarioId, Guid recursoId, CancellationToken cancellationToken = default)
        {
            if (actingUsuarioId == null)
            {
                return;
            }

            var ownerId = await context.Recursos
                .Where(r => r.RecursoId == recursoId)
                .Select(r => r.Escena.NovelaVersion.Novela.UsuarioCreadorId)
                .FirstOrDefaultAsync(cancellationToken);

            EnsureNovelaOwner(actingUsuarioId, ownerId);
        }
    }
}
