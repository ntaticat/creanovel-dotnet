using System;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Persistence;
using WebAPI.Features.NovelaVersiones;
using WebAPI.Features.Recursos;

namespace UnitTests;

public class CreateDraftFromVersionCommandTests
{
    private static CreanovelDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<CreanovelDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        return new CreanovelDbContext(options);
    }

    [Fact]
    public async Task Handle_ClonesCrossEscenaLinksWithRemappedIds()
    {
        await using var context = CreateContext();

        var novela = new Novela { Titulo = "Test", Disponible = true };
        await context.Novelas.AddAsync(novela);

        var publishedVersion = new NovelaVersion
        {
            NovelaId = novela.NovelaId,
            NumeroVersion = "1.0.0",
            Disponible = true,
            EsBorrador = false
        };
        await context.NovelaVersiones.AddAsync(publishedVersion);

        var escena1 = new Escena { NovelaVersionId = publishedVersion.NovelaVersionId, Identificador = "escena-1", PrimerEscena = true };
        var escena2 = new Escena { NovelaVersionId = publishedVersion.NovelaVersionId, Identificador = "escena-2" };
        await context.Escenas.AddRangeAsync(escena1, escena2);

        var decisionEnEscena2 = new Recurso
        {
            EscenaId = escena2.EscenaId,
            TipoRecurso = RecursoTipos.Decision,
            Contenido = JsonSerializer.Serialize(new DecisionContenido("¿Qué haces?", null))
        };
        await context.Recursos.AddAsync(decisionEnEscena2);

        var conversacionEnEscena1 = new Recurso
        {
            EscenaId = escena1.EscenaId,
            TipoRecurso = RecursoTipos.Conversacion,
            Contenido = JsonSerializer.Serialize(new ConversacionContenido("Hola", null)),
            PrimerRecurso = true,
            SiguienteRecursoId = decisionEnEscena2.RecursoId
        };
        await context.Recursos.AddAsync(conversacionEnEscena1);

        var opcionQueVuelve = new RecursoDecisionOpcion
        {
            RecursoDecisionId = decisionEnEscena2.RecursoId,
            OpcionMensaje = "Volver",
            SiguienteRecursoId = conversacionEnEscena1.RecursoId
        };
        await context.RecursoDecisionOpciones.AddAsync(opcionQueVuelve);

        await context.SaveChangesAsync();

        var handler = new CreateDraftFromVersion.Handler(context);

        var newVersionId = await handler.HandleAsync(novela.NovelaId, actingUsuarioId: null, CancellationToken.None);

        var newVersion = await context.NovelaVersiones.FirstAsync(nv => nv.NovelaVersionId == newVersionId);
        Assert.True(newVersion.EsBorrador);
        Assert.False(newVersion.Disponible);
        Assert.Equal("1.1.0", newVersion.NumeroVersion);

        var newEscenas = await context.Escenas.Where(e => e.NovelaVersionId == newVersionId).ToListAsync();
        Assert.Equal(2, newEscenas.Count);
        Assert.DoesNotContain(newEscenas, e => e.EscenaId == escena1.EscenaId || e.EscenaId == escena2.EscenaId);

        var newEscenaIds = newEscenas.Select(e => e.EscenaId).ToList();
        var newRecursos = await context.Recursos.Where(r => newEscenaIds.Contains(r.EscenaId)).ToListAsync();
        Assert.Equal(2, newRecursos.Count);

        var nuevaConversacion = newRecursos.Single(r => r.TipoRecurso == RecursoTipos.Conversacion);
        var nuevaDecision = newRecursos.Single(r => r.TipoRecurso == RecursoTipos.Decision);

        Assert.NotEqual(conversacionEnEscena1.RecursoId, nuevaConversacion.RecursoId);
        Assert.NotEqual(decisionEnEscena2.RecursoId, nuevaDecision.RecursoId);

        // The clone must point at the NEW decision node, not the original one.
        Assert.Equal(nuevaDecision.RecursoId, nuevaConversacion.SiguienteRecursoId);

        var nuevaOpcion = await context.RecursoDecisionOpciones
            .SingleAsync(o => o.RecursoDecisionId == nuevaDecision.RecursoId);

        // The cross-escena "volver" link must point back at the NEW conversacion node.
        Assert.Equal(nuevaConversacion.RecursoId, nuevaOpcion.SiguienteRecursoId);

        // Original content must be untouched.
        var originalStillThere = await context.Escenas.CountAsync(e => e.NovelaVersionId == publishedVersion.NovelaVersionId);
        Assert.Equal(2, originalStillThere);
    }
}
