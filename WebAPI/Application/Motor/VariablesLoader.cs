using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Motor
{
    // Carga lo que las reglas pueden citar (variables, objetos, ubicaciones) en la versión a la que pertenece una escena o un recurso.
    public static class VariablesLoader
    {
        public static async Task<ReglasContexto> PorEscenaAsync(CreanovelDbContext context, Guid escenaId, CancellationToken ct)
        {
            var json = await context.Escenas
                .Where(e => e.EscenaId == escenaId)
                .Select(e => e.NovelaVersion.Definiciones)
                .FirstOrDefaultAsync(ct);

            return ReglasContexto.Desde(MotorJson.LeerDefiniciones(json));
        }

        public static async Task<ReglasContexto> PorRecursoAsync(CreanovelDbContext context, Guid recursoId, CancellationToken ct)
        {
            var json = await context.Recursos
                .Where(r => r.RecursoId == recursoId)
                .Select(r => r.Escena.NovelaVersion.Definiciones)
                .FirstOrDefaultAsync(ct);

            return ReglasContexto.Desde(MotorJson.LeerDefiniciones(json));
        }
    }
}
