using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Application.Handlers;
using Domain.Models;
using Microsoft.EntityFrameworkCore;
using Persistence;
using WebAPI.Features.Escenas;
using WebAPI.Features.NovelaVersiones;

namespace UnitTests;

// Las etiquetas de una escena sirven al autor para agruparlas y filtrarlas en el editor.
public class EscenaEtiquetasTests
{
    private record Mundo(Guid DuenoId, Novela Novela, NovelaVersion Version);

    private static async Task<Mundo> NuevoMundoAsync(CreanovelDbContext context)
    {
        var duenoId = Guid.NewGuid();
        var novela = new Novela { Titulo = "T", UsuarioCreadorId = duenoId };
        await context.Novelas.AddAsync(novela);
        var version = new NovelaVersion { NovelaId = novela.NovelaId, NumeroVersion = "1.0.0", EsBorrador = true };
        await context.NovelaVersiones.AddAsync(version);
        await context.SaveChangesAsync();
        return new Mundo(duenoId, novela, version);
    }

    private static Task CrearAsync(CreanovelDbContext context, Mundo mundo, string identificador, params string[] etiquetas) =>
        new CreateEscena.Handler(context).HandleAsync(
            mundo.Version.NovelaVersionId,
            new CreateEscena.CreateEscenaRequest(identificador, false, false, etiquetas),
            mundo.DuenoId, CancellationToken.None);

    private static async Task<IReadOnlyList<string>> GuardadasAsync(CreanovelDbContext context, string identificador) =>
        EscenaEtiquetas.Leer((await context.Escenas.AsNoTracking().SingleAsync(e => e.Identificador == identificador)).Etiquetas);

    private static async Task<ExceptionHandler> Falla(Func<Task> accion) => await Assert.ThrowsAsync<ExceptionHandler>(accion);

    // ---- Normalizar

    [Fact]
    public void Normalizar_RecortaJuntaEspaciosYDescartaVaciasYRepetidas()
    {
        var json = EscenaEtiquetas.Normalizar(new[] { "  Capítulo   1 ", "", "   ", "capítulo 1", "Flashback", "FLASHBACK" });

        Assert.Equal(new[] { "Capítulo 1", "Flashback" }, EscenaEtiquetas.Leer(json));
    }

    [Fact]
    public void Normalizar_NuloOVacioEsSinEtiquetas()
    {
        Assert.Equal("[]", EscenaEtiquetas.Normalizar(null));
        Assert.Equal("[]", EscenaEtiquetas.Normalizar(Array.Empty<string>()));
    }

    [Fact]
    public void Normalizar_RechazaEtiquetasDemasiadoLargas()
    {
        var error = Assert.Throws<ExceptionHandler>(() => EscenaEtiquetas.Normalizar(new[] { new string('a', EscenaEtiquetas.LongitudMaxima + 1) }));

        Assert.Equal(HttpStatusCode.BadRequest, error.Code);
        // En el límite exacto sí se acepta.
        Assert.Single(EscenaEtiquetas.Leer(EscenaEtiquetas.Normalizar(new[] { new string('a', EscenaEtiquetas.LongitudMaxima) })));
    }

    [Fact]
    public void Normalizar_RechazaMasDelMaximo()
    {
        var demasiadas = Enumerable.Range(1, EscenaEtiquetas.Maximo + 1).Select(i => $"e{i}").ToList();

        var error = Assert.Throws<ExceptionHandler>(() => EscenaEtiquetas.Normalizar(demasiadas));

        Assert.Equal(HttpStatusCode.BadRequest, error.Code);
        Assert.Equal(EscenaEtiquetas.Maximo, EscenaEtiquetas.Leer(EscenaEtiquetas.Normalizar(demasiadas.Take(EscenaEtiquetas.Maximo).ToList())).Count);
    }

    [Fact]
    public void Leer_DatosCorruptosDanListaVacia()
    {
        Assert.Empty(EscenaEtiquetas.Leer("no es json"));
        Assert.Empty(EscenaEtiquetas.Leer(""));
        Assert.Empty(EscenaEtiquetas.Leer(null));
    }

    // ---- Crear / actualizar

    [Fact]
    public async Task Crear_GuardaLasEtiquetasNormalizadas()
    {
        await using var context = TestDb.CreateContext();
        var mundo = await NuevoMundoAsync(context);

        await CrearAsync(context, mundo, "intro", " Acto 1 ", "acto 1", "Flashback");

        Assert.Equal(new[] { "Acto 1", "Flashback" }, await GuardadasAsync(context, "intro"));
    }

    [Fact]
    public async Task Crear_SinEtiquetasQuedaVacia()
    {
        await using var context = TestDb.CreateContext();
        var mundo = await NuevoMundoAsync(context);

        await new CreateEscena.Handler(context).HandleAsync(
            mundo.Version.NovelaVersionId, new CreateEscena.CreateEscenaRequest("intro", false, false), mundo.DuenoId, CancellationToken.None);

        Assert.Empty(await GuardadasAsync(context, "intro"));
    }

    [Fact]
    public async Task Actualizar_NuloDejaLasEtiquetasYListaVaciaLasQuita()
    {
        await using var context = TestDb.CreateContext();
        var mundo = await NuevoMundoAsync(context);
        await CrearAsync(context, mundo, "intro", "Acto 1");
        var escenaId = (await context.Escenas.SingleAsync()).EscenaId;
        var handler = new UpdateEscena.Handler(context);

        await handler.HandleAsync(escenaId, new UpdateEscena.UpdateEscenaRequest("intro-2", null, null, null), mundo.DuenoId, CancellationToken.None);
        Assert.Equal(new[] { "Acto 1" }, await GuardadasAsync(context, "intro-2"));

        await handler.HandleAsync(escenaId, new UpdateEscena.UpdateEscenaRequest(null, null, null, null, new[] { "Acto 2", "Extra" }), mundo.DuenoId, CancellationToken.None);
        Assert.Equal(new[] { "Acto 2", "Extra" }, await GuardadasAsync(context, "intro-2"));

        await handler.HandleAsync(escenaId, new UpdateEscena.UpdateEscenaRequest(null, null, null, null, Array.Empty<string>()), mundo.DuenoId, CancellationToken.None);
        Assert.Empty(await GuardadasAsync(context, "intro-2"));
    }

    [Fact]
    public async Task Actualizar_EtiquetasInvalidasNoCambianNada()
    {
        await using var context = TestDb.CreateContext();
        var mundo = await NuevoMundoAsync(context);
        await CrearAsync(context, mundo, "intro", "Acto 1");
        var escenaId = (await context.Escenas.SingleAsync()).EscenaId;

        var error = await Falla(() => new UpdateEscena.Handler(context).HandleAsync(
            escenaId, new UpdateEscena.UpdateEscenaRequest(null, null, null, null, new[] { new string('x', 31) }), mundo.DuenoId, CancellationToken.None));

        Assert.Equal(HttpStatusCode.BadRequest, error.Code);
        Assert.Equal(new[] { "Acto 1" }, await GuardadasAsync(context, "intro"));
    }

    [Fact]
    public async Task Actualizar_OtroUsuarioNoPuedeEtiquetar()
    {
        await using var context = TestDb.CreateContext();
        var mundo = await NuevoMundoAsync(context);
        await CrearAsync(context, mundo, "intro", "Acto 1");
        var escenaId = (await context.Escenas.SingleAsync()).EscenaId;

        await Assert.ThrowsAsync<ExceptionHandler>(() => new UpdateEscena.Handler(context).HandleAsync(
            escenaId, new UpdateEscena.UpdateEscenaRequest(null, null, null, null, new[] { "Otra" }), Guid.NewGuid(), CancellationToken.None));

        Assert.Equal(new[] { "Acto 1" }, await GuardadasAsync(context, "intro"));
    }

    // ---- DTO y clonado

    [Fact]
    public async Task ElDtoExponeLasEtiquetas()
    {
        await using var context = TestDb.CreateContext();
        var mundo = await NuevoMundoAsync(context);
        await CrearAsync(context, mundo, "intro", "Acto 1", "Flashback");
        var escena = await context.Escenas.Include(e => e.Recursos).SingleAsync();

        Assert.Equal(new[] { "Acto 1", "Flashback" }, EscenaMapping.ToDto(escena).Etiquetas);
    }

    [Fact]
    public async Task ClonarUnaVersionConservaLasEtiquetas()
    {
        await using var context = TestDb.CreateContext();
        var mundo = await NuevoMundoAsync(context);
        mundo.Version.EsBorrador = false;
        mundo.Version.Disponible = true;
        await context.SaveChangesAsync();
        await CrearAsync(context, mundo, "intro", "Acto 1", "Flashback");
        await CrearAsync(context, mundo, "final");

        var nuevaVersionId = await new CreateDraftFromVersion.Handler(context).HandleAsync(mundo.Novela.NovelaId, mundo.DuenoId, CancellationToken.None);

        var clonadas = await context.Escenas.Where(e => e.NovelaVersionId == nuevaVersionId).ToListAsync();
        Assert.Equal(new[] { "Acto 1", "Flashback" }, EscenaEtiquetas.Leer(clonadas.Single(e => e.Identificador == "intro").Etiquetas));
        Assert.Empty(EscenaEtiquetas.Leer(clonadas.Single(e => e.Identificador == "final").Etiquetas));
    }
}
