using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Application.Handlers;
using Application.Motor;
using Domain.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Persistence;
using WebAPI.Features.Recursos;
using WebAPI.Infrastructure.Extensions;

namespace WebAPI.Features.NovelaVersiones
{
    // Entry-point resolution logic here mirrors NovelaPlayerService.resolverArte on the Angular side:
    // links between recursos can cross escena boundaries freely, so cloning a whole published version
    // into a new draft must remap every id up front (pass 1) before constructing any clone (pass 2),
    // rather than relying on iteration order to resolve cross-escena SiguienteRecursoId references.
    public static class CreateDraftFromVersion
    {
        public sealed class Handler
        {
            private readonly CreanovelDbContext _context;

            public Handler(CreanovelDbContext context)
            {
                _context = context;
            }

            public async Task<Guid> HandleAsync(Guid novelaId, Guid? actingUsuarioId, CancellationToken ct)
            {
                var novela = await _context.Novelas.FindAsync(new object[] { novelaId }, ct);

                if (novela == null)
                {
                    throw new ExceptionHandler(HttpStatusCode.NotFound, new { message = "Novela no encontrada" });
                }

                OwnershipGuard.EnsureNovelaOwner(actingUsuarioId, novela.UsuarioCreadorId);

                var existeBorrador = await _context.NovelaVersiones
                    .AnyAsync(nv => nv.NovelaId == novelaId && nv.EsBorrador, ct);

                if (existeBorrador)
                {
                    throw new ExceptionHandler(HttpStatusCode.Conflict, new { message = "Ya existe un borrador para esta novela" });
                }

                var sourceVersion = await _context.NovelaVersiones
                    .Include(nv => nv.Escenas)
                    .ThenInclude(e => e.Recursos)
                    .FirstOrDefaultAsync(nv => nv.NovelaId == novelaId && nv.Disponible, ct);

                if (sourceVersion == null)
                {
                    throw new ExceptionHandler(HttpStatusCode.NotFound, new { message = "La novela no tiene una versión publicada de la cual crear un borrador" });
                }

                var sourceRecursos = sourceVersion.Escenas.SelectMany(e => e.Recursos).ToList();

                var decisionIds = sourceRecursos.Where(r => RecursoTipos.TieneOpciones(r.TipoRecurso)).Select(r => r.RecursoId).ToList();
                var sourceOpciones = decisionIds.Any()
                    ? await _context.RecursoDecisionOpciones.Where(o => decisionIds.Contains(o.RecursoDecisionId)).ToListAsync(ct)
                    : new List<RecursoDecisionOpcion>();

                await using var transaction = await _context.Database.BeginTransactionAsync(ct);

                try
                {
                    // Pass 1: generate new ids up front so cross-escena links resolve correctly regardless of iteration order.
                    var oldToNewEscenaId = sourceVersion.Escenas.ToDictionary(e => e.EscenaId, _ => Guid.NewGuid());
                    var oldToNewRecursoId = sourceRecursos.ToDictionary(r => r.RecursoId, _ => Guid.NewGuid());

                    var newVersion = new Domain.Models.NovelaVersion
                    {
                        NovelaId = novelaId,
                        NumeroVersion = BumpVersion(sourceVersion.NumeroVersion),
                        // Los objetos pueden llevar a un nodo: ese id (dentro del jsonb) debe seguir al nodo clonado.
                        Definiciones = UsoObjetoDestinos.Remapear(sourceVersion.Definiciones, oldToNewRecursoId),
                        Disponible = false,
                        EsBorrador = true
                    };
                    await _context.NovelaVersiones.AddAsync(newVersion, ct);

                    // Pass 2: construct the clones with remapped ids/links.
                    foreach (var escena in sourceVersion.Escenas)
                    {
                        var nuevaEscena = new Domain.Models.Escena
                        {
                            EscenaId = oldToNewEscenaId[escena.EscenaId],
                            Identificador = escena.Identificador,
                            NovelaVersionId = newVersion.NovelaVersionId,
                            PrimerEscena = escena.PrimerEscena,
                            UltimaEscena = escena.UltimaEscena,
                            Etiquetas = escena.Etiquetas
                        };
                        await _context.Escenas.AddAsync(nuevaEscena, ct);
                    }

                    foreach (var recurso in sourceRecursos)
                    {
                        var nuevoRecurso = new Recurso
                        {
                            RecursoId = oldToNewRecursoId[recurso.RecursoId],
                            EscenaId = oldToNewEscenaId[recurso.EscenaId],
                            TipoRecurso = recurso.TipoRecurso,
                            PrimerRecurso = recurso.PrimerRecurso,
                            UltimoRecurso = recurso.UltimoRecurso,
                            Personajes = recurso.Personajes,
                            BackgroundSpriteId = recurso.BackgroundSpriteId,
                            SiguienteRecursoId = recurso.SiguienteRecursoId.HasValue
                                ? oldToNewRecursoId.GetValueOrDefault(recurso.SiguienteRecursoId.Value)
                                : null,
                            Contenido = recurso.Contenido
                        };
                        await _context.Recursos.AddAsync(nuevoRecurso, ct);
                    }

                    foreach (var opcion in sourceOpciones)
                    {
                        var nuevaOpcion = new RecursoDecisionOpcion
                        {
                            RecursoDecisionOpcionId = Guid.NewGuid(),
                            RecursoDecisionId = oldToNewRecursoId[opcion.RecursoDecisionId],
                            OpcionMensaje = opcion.OpcionMensaje,
                            Orden = opcion.Orden,
                            Tipo = opcion.Tipo,
                            Condicion = opcion.Condicion,
                            CondicionModo = opcion.CondicionModo,
                            Efectos = opcion.Efectos,
                            Region = opcion.Region,
                            SiguienteRecursoId = opcion.SiguienteRecursoId.HasValue
                                ? oldToNewRecursoId.GetValueOrDefault(opcion.SiguienteRecursoId.Value)
                                : null
                        };
                        await _context.RecursoDecisionOpciones.AddAsync(nuevaOpcion, ct);
                    }

                    var result = await _context.SaveChangesAsync(ct);

                    if (result <= 0)
                    {
                        throw new ExceptionHandler(HttpStatusCode.BadRequest, new { message = "No se pudo crear el borrador" });
                    }

                    await transaction.CommitAsync(ct);

                    return newVersion.NovelaVersionId;
                }
                catch (Exception)
                {
                    await transaction.RollbackAsync(ct);
                    throw new ExceptionHandler(HttpStatusCode.BadRequest, new { message = "No se pudo crear el borrador a partir de la versión publicada" });
                }
            }

            public static string BumpVersion(string numeroVersion)
            {
                var partes = numeroVersion?.Split('.') ?? Array.Empty<string>();

                if (partes.Length == 3 && int.TryParse(partes[1], out var minor))
                {
                    return $"{partes[0]}.{minor + 1}.0";
                }

                return $"{numeroVersion}-borrador";
            }
        }

        public static class Endpoint
        {
            public static void Map(IEndpointRouteBuilder app) =>
                app.MapPost("api/novelas/{novelaId:guid}/borrador", async (Guid novelaId, Handler handler, ClaimsPrincipal user, CancellationToken ct) =>
                    {
                        var versionId = await handler.HandleAsync(novelaId, user.GetActingUsuarioId(), ct);
                        return Results.Ok(versionId);
                    })
                    .RequireAuthorization()
                    .WithTags("NovelaVersiones");
        }
    }
}
