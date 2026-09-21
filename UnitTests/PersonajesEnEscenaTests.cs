using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Application.Handlers;
using Domain.Models;
using Microsoft.EntityFrameworkCore;
using Persistence;
using WebAPI.Features.NovelaVersiones;
using WebAPI.Features.Personajes;
using WebAPI.Features.Recursos;

namespace UnitTests;

// Varios personajes por nodo, cada uno colocado (x, y, escala, espejo) sobre el escenario 16:9. Los sprites se registran enteros: sin recorte.
public class PersonajesEnEscenaTests
{
    private record Mundo(Guid DuenoId, Novela Novela, NovelaVersion Version, Escena Escena, PersonajeSprite Ana, PersonajeSprite Beto);

    private static async Task<Mundo> NuevoMundoAsync(CreanovelDbContext context, bool borrador = true)
    {
        var duenoId = Guid.NewGuid();
        var novela = new Novela { Titulo = "T", UsuarioCreadorId = duenoId };
        await context.Novelas.AddAsync(novela);
        var version = new NovelaVersion { NovelaId = novela.NovelaId, NumeroVersion = "1.0.0", EsBorrador = borrador, Disponible = !borrador };
        await context.NovelaVersiones.AddAsync(version);
        var escena = new Escena { NovelaVersionId = version.NovelaVersionId, PrimerEscena = true };
        await context.Escenas.AddAsync(escena);
        var personaje = new Personaje { Nombre = "Ana" };
        await context.Personajes.AddAsync(personaje);
        var ana = new PersonajeSprite { Nombre = "feliz", DireccionImagen = "/uploads/personajes/a.png", PersonajeId = personaje.PersonajeId };
        var beto = new PersonajeSprite { Nombre = "serio", DireccionImagen = "/uploads/personajes/b.png", PersonajeId = personaje.PersonajeId };
        await context.PersonajeSprites.AddRangeAsync(ana, beto);
        await context.SaveChangesAsync();
        return new Mundo(duenoId, novela, version, escena, ana, beto);
    }

    private static PersonajeEnEscenaDto En(PersonajeSprite sprite, double x = 50, double y = 50, double escala = 1, bool espejo = false) =>
        new(sprite.PersonajeSpriteId, x, y, escala, espejo);

    private static async Task<Guid> CrearHablaAsync(CreanovelDbContext context, Mundo mundo, List<PersonajeEnEscenaDto> personajes) =>
        await new CreateRecursoConversacion.Handler(context).HandleAsync(
            new CreateRecursoConversacion.CreateRecursoConversacionRequest(
                mundo.Escena.EscenaId, RecursoTipos.Conversacion, true, false, "Hola", "Ana", null, personajes, null),
            mundo.DuenoId, CancellationToken.None);

    private static async Task<IReadOnlyList<PersonajeEnEscenaDto>> GuardadosAsync(CreanovelDbContext context, Guid recursoId) =>
        PersonajesEnEscena.Leer((await context.Recursos.AsNoTracking().SingleAsync(r => r.RecursoId == recursoId)).Personajes);

    // ---- Crear

    [Fact]
    public async Task CrearHabla_GuardaVariosPersonajesConSuColocacionYOrden()
    {
        await using var context = TestDb.CreateContext();
        var mundo = await NuevoMundoAsync(context);

        var id = await CrearHablaAsync(context, mundo, new() { En(mundo.Ana, 25, 60, 0.8, true), En(mundo.Beto, 75, 55, 1.5) });

        var guardados = await GuardadosAsync(context, id);
        Assert.Equal(new[] { mundo.Ana.PersonajeSpriteId, mundo.Beto.PersonajeSpriteId }, guardados.Select(p => p.PersonajeSpriteId));
        Assert.Equal((25d, 60d, 0.8, true), (guardados[0].X, guardados[0].Y, guardados[0].Escala, guardados[0].Espejo));
        Assert.Equal((75d, 55d, 1.5, false), (guardados[1].X, guardados[1].Y, guardados[1].Escala, guardados[1].Espejo));
    }

    [Fact]
    public async Task CrearHabla_SinPersonajes_GuardaListaVacia()
    {
        await using var context = TestDb.CreateContext();
        var mundo = await NuevoMundoAsync(context);

        var id = await CrearHablaAsync(context, mundo, null!);

        Assert.Equal("[]", (await context.Recursos.SingleAsync(r => r.RecursoId == id)).Personajes);
    }

    [Fact]
    public async Task ElDtoDelRecurso_DevuelveLosPersonajes()
    {
        await using var context = TestDb.CreateContext();
        var mundo = await NuevoMundoAsync(context);
        var id = await CrearHablaAsync(context, mundo, new() { En(mundo.Ana, 30, 70, 2) });

        var dto = (RecursoConversacionDto)RecursoMapping.ToDto(await context.Recursos.SingleAsync(r => r.RecursoId == id));

        var personaje = Assert.Single(dto.Personajes);
        Assert.Equal((mundo.Ana.PersonajeSpriteId, 30d, 70d, 2d), (personaje.PersonajeSpriteId, personaje.X, personaje.Y, personaje.Escala));
    }

    [Fact]
    public async Task CrearExplora_ConElEndpointGenerico_TambienGuardaPersonajes()
    {
        await using var context = TestDb.CreateContext();
        var mundo = await NuevoMundoAsync(context);

        var id = await new CreateRecurso.Handler(context).HandleAsync(new CreateRecurso.CreateRecursoRequest(
            mundo.Escena.EscenaId, RecursoTipos.Explora, false, false, null, new() { En(mundo.Beto, 10, 90, 0.5) }, null,
            JsonDocument.Parse("""{"mensaje":"Un pasillo"}""").RootElement.Clone()), mundo.DuenoId, CancellationToken.None);

        Assert.Equal(mundo.Beto.PersonajeSpriteId, Assert.Single(await GuardadosAsync(context, id)).PersonajeSpriteId);
    }

    // ---- Validación

    public static IEnumerable<object[]> Invalidos()
    {
        yield return new object[] { 100.5, 50d, 1d };                 // x fuera del escenario
        yield return new object[] { 50d, -0.1, 1d };                  // y fuera del escenario
        yield return new object[] { 50d, 50d, 0.05 };                 // demasiado pequeño
        yield return new object[] { 50d, 50d, 4.5 };                  // demasiado grande
        yield return new object[] { double.NaN, 50d, 1d };
        yield return new object[] { 50d, 50d, double.PositiveInfinity };
    }

    [Theory]
    [MemberData(nameof(Invalidos))]
    public async Task CrearHabla_ConColocacionFueraDeRango_Falla400(double x, double y, double escala)
    {
        await using var context = TestDb.CreateContext();
        var mundo = await NuevoMundoAsync(context);

        var ex = await Assert.ThrowsAsync<ExceptionHandler>(() => CrearHablaAsync(context, mundo, new() { En(mundo.Ana, x, y, escala) }));

        Assert.Equal(HttpStatusCode.BadRequest, ex.Code);
        Assert.Empty(context.Recursos);
    }

    [Fact]
    public async Task CrearHabla_ConUnSpriteInexistente_Falla400()
    {
        await using var context = TestDb.CreateContext();
        var mundo = await NuevoMundoAsync(context);

        var ex = await Assert.ThrowsAsync<ExceptionHandler>(() =>
            CrearHablaAsync(context, mundo, new() { new PersonajeEnEscenaDto(Guid.NewGuid(), 50, 50, 1, false) }));

        Assert.Equal(HttpStatusCode.BadRequest, ex.Code);
        Assert.Contains("no existe", JsonSerializer.Serialize(ex.Errors));
    }

    [Fact]
    public async Task CrearHabla_ConDemasiadosPersonajes_Falla400_YElMaximoSiSeAcepta()
    {
        await using var context = TestDb.CreateContext();
        var mundo = await NuevoMundoAsync(context);

        var maximo = Enumerable.Range(0, PersonajesEnEscena.Maximo).Select(i => En(mundo.Ana, 10 + i * 10)).ToList();
        await CrearHablaAsync(context, mundo, maximo);   // el mismo sprite puede aparecer varias veces

        var ex = await Assert.ThrowsAsync<ExceptionHandler>(() => CrearHablaAsync(context, mundo, maximo.Append(En(mundo.Beto)).ToList()));
        Assert.Equal(HttpStatusCode.BadRequest, ex.Code);
    }

    // ---- Actualizar

    [Fact]
    public async Task Actualizar_ReemplazaLaLista_SinListaNoLaToca_YVaciaLaQuita()
    {
        await using var context = TestDb.CreateContext();
        var mundo = await NuevoMundoAsync(context);
        var id = await CrearHablaAsync(context, mundo, new() { En(mundo.Ana) });
        var handler = new UpdateRecursoConversacion.Handler(context);
        Task Actualizar(List<PersonajeEnEscenaDto> personajes) => handler.HandleAsync(id,
            new UpdateRecursoConversacion.UpdateRecursoConversacionRequest("Hola", "Ana", null, null, null, personajes, null),
            mundo.DuenoId, CancellationToken.None);

        await Actualizar(new() { En(mundo.Beto, 80, 40, 1.2, true), En(mundo.Ana, 20) });
        Assert.Equal(new[] { mundo.Beto.PersonajeSpriteId, mundo.Ana.PersonajeSpriteId }, (await GuardadosAsync(context, id)).Select(p => p.PersonajeSpriteId));

        await Actualizar(null!);
        Assert.Equal(2, (await GuardadosAsync(context, id)).Count);   // omitirla no borra nada

        await Actualizar(new());
        Assert.Empty(await GuardadosAsync(context, id));
    }

    [Fact]
    public async Task Actualizar_ConColocacionInvalida_NoCambiaNada()
    {
        await using var context = TestDb.CreateContext();
        var mundo = await NuevoMundoAsync(context);
        var id = await CrearHablaAsync(context, mundo, new() { En(mundo.Ana, 30) });

        await Assert.ThrowsAsync<ExceptionHandler>(() => new UpdateRecursoConversacion.Handler(context).HandleAsync(id,
            new UpdateRecursoConversacion.UpdateRecursoConversacionRequest("x", "x", null, null, null, new() { En(mundo.Ana, 500) }, null),
            mundo.DuenoId, CancellationToken.None));

        Assert.Equal(30d, Assert.Single(await GuardadosAsync(context, id)).X);
    }

    // ---- Datos guardados

    [Theory]
    [InlineData("")]
    [InlineData("no es json")]
    [InlineData("{\"a\":1}")]
    [InlineData("null")]
    public void Leer_DatosCorruptosOVacios_DevuelveListaVacia(string json)
    {
        Assert.Empty(PersonajesEnEscena.Leer(json));
    }

    // ---- Clonar la versión

    [Fact]
    public async Task CreateDraft_ConservaLosPersonajesDeCadaNodo()
    {
        await using var context = TestDb.CreateContext();
        var mundo = await NuevoMundoAsync(context, borrador: false);
        await CrearHablaAsync(context, mundo, new() { En(mundo.Ana, 15, 65, 0.9), En(mundo.Beto, 85, 60, 1.1, true) });

        var draftId = await new CreateDraftFromVersion.Handler(context).HandleAsync(mundo.Novela.NovelaId, mundo.DuenoId, CancellationToken.None);

        var clon = await context.Recursos.SingleAsync(r => r.Escena.NovelaVersionId == draftId);
        var personajes = PersonajesEnEscena.Leer(clon.Personajes);
        Assert.Equal(new[] { 15d, 85d }, personajes.Select(p => p.X));
        Assert.Equal(new[] { mundo.Ana.PersonajeSpriteId, mundo.Beto.PersonajeSpriteId }, personajes.Select(p => p.PersonajeSpriteId));
    }

    // ---- Borrar un sprite

    [Fact]
    public async Task BorrarSprite_LoQuitaDeTodosLosNodos_ConservandoLosDemasEnOrden()
    {
        await using var context = TestDb.CreateContext();
        var mundo = await NuevoMundoAsync(context);
        var conAmbos = await CrearHablaAsync(context, mundo, new() { En(mundo.Beto, 10), En(mundo.Ana, 50), En(mundo.Beto, 90) });
        var soloAna = await CrearHablaAsync(context, mundo, new() { En(mundo.Ana) });
        var otroMundo = await NuevoMundoAsync(context);   // otra novela cuyos nodos también lo usan
        var ajeno = await CrearHablaAsync(context, otroMundo, new() { En(mundo.Beto, 33) });

        await new DeletePersonajeSprite.Handler(context).HandleAsync(mundo.Beto.PersonajeSpriteId, CancellationToken.None);

        Assert.Equal(new[] { mundo.Ana.PersonajeSpriteId }, (await GuardadosAsync(context, conAmbos)).Select(p => p.PersonajeSpriteId));
        Assert.Equal(mundo.Ana.PersonajeSpriteId, Assert.Single(await GuardadosAsync(context, soloAna)).PersonajeSpriteId);
        Assert.Empty(await GuardadosAsync(context, ajeno));
        Assert.Null(await context.PersonajeSprites.FindAsync(mundo.Beto.PersonajeSpriteId));
        Assert.NotNull(await context.PersonajeSprites.FindAsync(mundo.Ana.PersonajeSpriteId));
    }

    // ---- Los sprites se registran sin recorte

    [Fact]
    public async Task RegistrarSprite_GuardaLaImagenTalCual_YElDtoNoTieneCamposDeRecorte()
    {
        await using var context = TestDb.CreateContext();
        var personaje = new Personaje { Nombre = "Luz" };
        await context.Personajes.AddAsync(personaje);
        await context.SaveChangesAsync();

        var id = await new CreatePersonajeSprite.Handler(context).HandleAsync(
            new CreatePersonajeSprite.CreatePersonajeSpriteRequest("normal", "/uploads/personajes/luz.png", personaje.PersonajeId), CancellationToken.None);

        Assert.Equal("/uploads/personajes/luz.png", (await context.PersonajeSprites.SingleAsync(s => s.PersonajeSpriteId == id)).DireccionImagen);
        Assert.Equal(new[] { "PersonajeSpriteId", "Nombre", "DireccionImagen" },
            typeof(PersonajeSpriteDto).GetProperties().Select(p => p.Name));
    }
}
