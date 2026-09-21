using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Application.Handlers;
using Application.Motor;
using Domain.Models;
using Microsoft.EntityFrameworkCore;
using Persistence;
using WebAPI.Features.NovelaVersiones;
using WebAPI.Features.Recursos;

namespace UnitTests;

public class ExploraTests
{
    private const string Definiciones = """
        {"variables":[{"clave":"salud","tipo":"numero","valores":[],"hud":"oculto"}],
         "objetos":[{"id":"llave","nombre":"Llave","descripcion":"","apilable":false,"inicial":0},
                    {"id":"moneda","nombre":"Moneda","descripcion":"","apilable":true,"max":99,"inicial":0}],
         "ubicaciones":[{"id":"biblioteca","nombre":"Biblioteca"},{"id":"patio","nombre":"Patio"}]}
        """;

    private const string RegionValida = """{"x":10,"y":20,"ancho":30,"alto":25}""";

    private static JsonElement Json(string json) => JsonDocument.Parse(json).RootElement.Clone();

    private record Mundo(Guid DuenoId, Novela Novela, NovelaVersion Version, Escena Escena);

    private static async Task<Mundo> NuevoMundoAsync(CreanovelDbContext context, string definiciones = Definiciones)
    {
        var duenoId = Guid.NewGuid();
        var novela = new Novela { Titulo = "T", UsuarioCreadorId = duenoId };
        await context.Novelas.AddAsync(novela);
        var version = new NovelaVersion { NovelaId = novela.NovelaId, NumeroVersion = "1.0.0", EsBorrador = true, Definiciones = definiciones };
        await context.NovelaVersiones.AddAsync(version);
        var escena = new Escena { NovelaVersionId = version.NovelaVersionId, PrimerEscena = true };
        await context.Escenas.AddAsync(escena);
        await context.SaveChangesAsync();
        return new Mundo(duenoId, novela, version, escena);
    }

    private static async Task<Recurso> NuevoExploraAsync(CreanovelDbContext context, Mundo mundo)
    {
        var explora = new Recurso
        {
            EscenaId = mundo.Escena.EscenaId, TipoRecurso = RecursoTipos.Explora, PrimerRecurso = true,
            Contenido = JsonSerializer.Serialize(new ExploraContenido(""))
        };
        await context.Recursos.AddAsync(explora);
        await context.SaveChangesAsync();
        return explora;
    }

    private static AddRecursoDecisionOpcion.AddRecursoDecisionOpcionRequest Zona(
        Guid exploraId, string etiqueta, string? region = RegionValida, string? condicion = null, string? efectos = null, Guid? destino = null) =>
        new(etiqueta, destino, exploraId, null,
            condicion == null ? null : Json(condicion), null,
            efectos == null ? null : Json(efectos),
            region == null ? null : Json(region));

    // ---- Definiciones: objetos y ubicaciones

    private static (DefinicionesJuego, List<string>) Normalizar(List<ObjetoDef>? objetos = null, List<UbicacionDef>? ubicaciones = null, string? inicial = null) =>
        MotorJson.NormalizarYValidar(new DefinicionesJuego(new List<VariableDef>(), objetos, ubicaciones, inicial));

    [Fact]
    public void Definiciones_ObjetosYUbicacionesValidos_SeNormalizan()
    {
        var (defs, errores) = Normalizar(
            new() { new("llave", " ", null!, null!, false, 5, 0), new("moneda", "Moneda", "", "/uploads/objetos/abc.png", true, 99, 3) },
            new() { new("biblioteca", "Biblioteca", null) },
            "biblioteca");

        Assert.Empty(errores);
        Assert.Equal("llave", defs.Objetos![0].Nombre);          // sin nombre → usa el id
        Assert.Null(defs.Objetos[0].Max);                        // un no apilable no tiene máximo
        Assert.Equal("", defs.Objetos[0].Descripcion);
        Assert.Equal("biblioteca", defs.UbicacionInicial);
    }

    [Fact]
    public void Definiciones_ObjetoInvalido_ReportaErrores()
    {
        var (_, errores) = Normalizar(new()
        {
            new("Mala Clave", "x", "", null!, false, null, 0),          // id inválido
            new("a", "x", "", null!, false, null, 0),
            new("a", "x", "", null!, false, null, 0),                    // repetido
            new("b", "x", "", "https://evil.example/x.png", false, null, 0), // imagen externa
            new("c", "x", "", null!, false, null, 2),                    // no apilable con 2
            new("d", "x", "", null!, true, 5, 9),                        // inicial > max
            new("e", "x", "", null!, true, 0, 0),                        // max < 1
        });

        Assert.Equal(6, errores.Count);
    }

    [Fact]
    public void Definiciones_UbicacionInicialInexistenteORepetida_ReportaError()
    {
        var (_, e1) = Normalizar(ubicaciones: new() { new("a", "A", null), new("a", "A", null) });
        Assert.Single(e1);

        var (_, e2) = Normalizar(ubicaciones: new() { new("a", "A", null) }, inicial: "fantasma");
        Assert.Single(e2);
    }

    [Fact]
    public void LeerDefiniciones_VersionesAnterioresSinObjetos_NoRompen()
    {
        var defs = MotorJson.LeerDefiniciones("""{"variables":[]}""");

        Assert.NotNull(defs.Objetos);
        Assert.NotNull(defs.Ubicaciones);
        Assert.Empty(defs.Objetos);
    }

    // ---- MotorValidador: objetos y ubicaciones

    private static ReglasContexto Ctx() => ReglasContexto.Desde(MotorJson.LeerDefiniciones(Definiciones));

    private static List<string> Condicion(string json)
    {
        var errores = new List<string>();
        MotorValidador.ValidarCondicion(Json(json), Ctx(), "c", errores);
        return errores;
    }

    private static List<string> Efectos(string json)
    {
        var errores = new List<string>();
        MotorValidador.ValidarEfectos(Json(json), Ctx(), "e", errores);
        return errores;
    }

    [Fact]
    public void Condicion_ObjetoYUbicacion_Validas()
    {
        Assert.Empty(Condicion("""{"objeto":"llave","op":">=","valor":1}"""));
        Assert.Empty(Condicion("""{"en":"biblioteca"}"""));
        Assert.Empty(Condicion("""{"y":[{"en":"patio"},{"no":{"objeto":"moneda","op":"<","valor":5}}]}"""));
    }

    [Theory]
    [InlineData("""{"objeto":"fantasma","op":">=","valor":1}""")]
    [InlineData("""{"objeto":"llave","op":">=","valor":-1}""")]
    [InlineData("""{"objeto":"llave","op":">=","valor":1.5}""")]
    [InlineData("""{"objeto":"llave","op":"~","valor":1}""")]
    [InlineData("""{"objeto":"llave","op":">=","valor":"uno"}""")]
    [InlineData("""{"en":"la_luna"}""")]
    [InlineData("""{"en":7}""")]
    [InlineData("""{"en":"patio","extra":1}""")]
    public void Condicion_ObjetoOUbicacionInvalida_ReportaError(string json) => Assert.NotEmpty(Condicion(json));

    [Fact]
    public void Efectos_ObjetoYUbicacion_Validos()
    {
        Assert.Empty(Efectos("""[{"objeto":"llave","op":"dar"},{"objeto":"moneda","op":"dar","cantidad":5},{"objeto":"moneda","op":"quitar","cantidad":2},{"ir":"patio"}]"""));
    }

    [Theory]
    [InlineData("""[{"objeto":"fantasma","op":"dar"}]""")]
    [InlineData("""[{"objeto":"llave","op":"sumar"}]""")]
    [InlineData("""[{"objeto":"llave","op":"dar","cantidad":0}]""")]
    [InlineData("""[{"objeto":"llave","op":"dar","cantidad":"dos"}]""")]
    [InlineData("""[{"objeto":"llave","op":"dar","valor":1}]""")]
    [InlineData("""[{"ir":"la_luna"}]""")]
    [InlineData("""[{"ir":"patio","op":"dar"}]""")]
    public void Efectos_ObjetoOUbicacionInvalidos_ReportanError(string json) => Assert.NotEmpty(Efectos(json));

    [Theory]
    [InlineData("""{"x":0,"y":0,"ancho":100,"alto":100}""", true)]
    [InlineData("""{"x":10.5,"y":20,"ancho":30,"alto":25}""", true)]
    [InlineData("""{"x":-1,"y":0,"ancho":10,"alto":10}""", false)]
    [InlineData("""{"x":0,"y":0,"ancho":0,"alto":10}""", false)]
    [InlineData("""{"x":95,"y":0,"ancho":10,"alto":10}""", false)]
    [InlineData("""{"x":0,"y":0,"ancho":10}""", false)]
    [InlineData("""{"x":"0","y":0,"ancho":10,"alto":10}""", false)]
    [InlineData("""{"x":0,"y":0,"ancho":10,"alto":10,"z":1}""", false)]
    [InlineData("""[0,0,10,10]""", false)]
    public void Region_SeValida(string json, bool valida)
    {
        var errores = new List<string>();
        MotorValidador.ValidarRegion(Json(json), "r", errores);
        Assert.Equal(valida, errores.Count == 0);
    }

    // ---- Handlers: Explora y zonas

    [Fact]
    public async Task CreateRecurso_Explora_GuardaMensajeEIgnoraSiguiente()
    {
        await using var context = TestDb.CreateContext();
        var mundo = await NuevoMundoAsync(context);
        var otro = new Recurso { EscenaId = mundo.Escena.EscenaId, TipoRecurso = RecursoTipos.Conversacion };
        await context.Recursos.AddAsync(otro);
        await context.SaveChangesAsync();

        var id = await new CreateRecurso.Handler(context).HandleAsync(new CreateRecurso.CreateRecursoRequest(
            mundo.Escena.EscenaId, RecursoTipos.Explora, true, false, otro.RecursoId, null, null, Json("""{"mensaje":"Un pasillo largo"}""")),
            mundo.DuenoId, CancellationToken.None);

        var recurso = await context.Recursos.FindAsync(id);
        Assert.Null(recurso!.SiguienteRecursoId);
        var dto = (RecursoExploraDto)RecursoMapping.ToDto(recurso);
        Assert.Equal("Un pasillo largo", dto.Mensaje);
    }

    [Fact]
    public async Task AddZona_GuardaRegionTipoZonaYCondicionConObjeto()
    {
        await using var context = TestDb.CreateContext();
        var mundo = await NuevoMundoAsync(context);
        var explora = await NuevoExploraAsync(context, mundo);

        await new AddRecursoDecisionOpcion.Handler(context).HandleAsync(
            Zona(explora.RecursoId, "Puerta cerrada", condicion: """{"objeto":"llave","op":">=","valor":1}""",
                efectos: """[{"ir":"patio"}]"""), mundo.DuenoId, CancellationToken.None);

        var zona = await context.RecursoDecisionOpciones.SingleAsync();
        Assert.Equal(RecursoSalidaTipos.Zona, zona.Tipo);
        Assert.Contains("\"ancho\":30", zona.Region!.Replace(" ", ""));

        var dto = ((RecursoExploraDto)RecursoMapping.ToDto(await context.Recursos.Include(r => r.Opciones).SingleAsync())).Opciones.Single();
        Assert.Equal(30, dto.Region!.Value.GetProperty("ancho").GetDouble());
        Assert.Equal("llave", dto.Condicion!.Value.GetProperty("objeto").GetString());
    }

    [Theory]
    [InlineData(null)]                                                // sin región
    [InlineData("""{"x":90,"y":0,"ancho":20,"alto":10}""")]           // se sale del escenario
    public async Task AddZona_ConRegionInvalida_Falla400(string? region)
    {
        await using var context = TestDb.CreateContext();
        var mundo = await NuevoMundoAsync(context);
        var explora = await NuevoExploraAsync(context, mundo);

        var ex = await Assert.ThrowsAsync<ExceptionHandler>(() => new AddRecursoDecisionOpcion.Handler(context).HandleAsync(
            Zona(explora.RecursoId, "Zona", region), mundo.DuenoId, CancellationToken.None));

        Assert.Equal(HttpStatusCode.BadRequest, ex.Code);
        Assert.Empty(context.RecursoDecisionOpciones);
    }

    [Fact]
    public async Task AddZona_SinNombre_Falla400()
    {
        await using var context = TestDb.CreateContext();
        var mundo = await NuevoMundoAsync(context);
        var explora = await NuevoExploraAsync(context, mundo);

        var ex = await Assert.ThrowsAsync<ExceptionHandler>(() => new AddRecursoDecisionOpcion.Handler(context).HandleAsync(
            Zona(explora.RecursoId, "  "), mundo.DuenoId, CancellationToken.None));

        Assert.Equal(HttpStatusCode.BadRequest, ex.Code);
    }

    [Fact]
    public async Task AddOpcion_EnSelecciona_DescartaLaRegion()
    {
        await using var context = TestDb.CreateContext();
        var mundo = await NuevoMundoAsync(context);
        var decision = new Recurso { EscenaId = mundo.Escena.EscenaId, TipoRecurso = RecursoTipos.Decision };
        await context.Recursos.AddAsync(decision);
        await context.SaveChangesAsync();

        await new AddRecursoDecisionOpcion.Handler(context).HandleAsync(
            Zona(decision.RecursoId, "Opción"), mundo.DuenoId, CancellationToken.None);

        var opcion = await context.RecursoDecisionOpciones.SingleAsync();
        Assert.Equal(RecursoSalidaTipos.Opcion, opcion.Tipo);
        Assert.Null(opcion.Region);
    }

    [Fact]
    public async Task UpdateZona_ReemplazaLaRegion_YNoDejaQuitarla()
    {
        await using var context = TestDb.CreateContext();
        var mundo = await NuevoMundoAsync(context);
        var explora = await NuevoExploraAsync(context, mundo);
        await new AddRecursoDecisionOpcion.Handler(context).HandleAsync(Zona(explora.RecursoId, "Puerta"), mundo.DuenoId, CancellationToken.None);
        var zona = await context.RecursoDecisionOpciones.SingleAsync();
        var handler = new UpdateRecursoDecisionOpcion.Handler(context);

        await handler.HandleAsync(zona.RecursoDecisionOpcionId,
            new UpdateRecursoDecisionOpcion.UpdateRecursoDecisionOpcionRequest(null!, null, null, null, null, null,
                Json("""{"x":50,"y":50,"ancho":10,"alto":10}""")), mundo.DuenoId, CancellationToken.None);
        Assert.Contains("50", (await context.RecursoDecisionOpciones.SingleAsync()).Region);

        var ex = await Assert.ThrowsAsync<ExceptionHandler>(() => handler.HandleAsync(zona.RecursoDecisionOpcionId,
            new UpdateRecursoDecisionOpcion.UpdateRecursoDecisionOpcionRequest(null!, null, null, null, null, null, null),
            mundo.DuenoId, CancellationToken.None));
        Assert.Equal(HttpStatusCode.BadRequest, ex.Code);
    }

    [Fact]
    public async Task AddOpcion_ConObjetoInexistente_Falla400()
    {
        await using var context = TestDb.CreateContext();
        var mundo = await NuevoMundoAsync(context);
        var explora = await NuevoExploraAsync(context, mundo);

        var ex = await Assert.ThrowsAsync<ExceptionHandler>(() => new AddRecursoDecisionOpcion.Handler(context).HandleAsync(
            Zona(explora.RecursoId, "Baúl", efectos: """[{"objeto":"tesoro","op":"dar"}]"""), mundo.DuenoId, CancellationToken.None));

        Assert.Equal(HttpStatusCode.BadRequest, ex.Code);
    }

    // ---- Validación de la versión

    [Fact]
    public async Task ValidarVersion_ExploraSinZonas_EsError_ConZonaValida_NoLoEs()
    {
        await using var context = TestDb.CreateContext();
        var mundo = await NuevoMundoAsync(context);
        var explora = await NuevoExploraAsync(context, mundo);

        var sinZonas = await VersionValidator.ValidarAsync(context, mundo.Version.NovelaVersionId, CancellationToken.None);
        Assert.Contains(sinZonas.Errores, e => e.Codigo == "explora_sin_zonas");

        await new AddRecursoDecisionOpcion.Handler(context).HandleAsync(
            Zona(explora.RecursoId, "Pared", efectos: """[{"objeto":"llave","op":"dar"}]"""), mundo.DuenoId, CancellationToken.None);

        var conZonas = await VersionValidator.ValidarAsync(context, mundo.Version.NovelaVersionId, CancellationToken.None);
        Assert.True(conZonas.Valida);
        Assert.DoesNotContain(conZonas.Advertencias, a => a.Codigo == "zona_sin_efecto");
    }

    [Fact]
    public async Task ValidarVersion_ZonaSinDestinoNiEfectos_EsAdvertencia_YRegionRotaEsError()
    {
        await using var context = TestDb.CreateContext();
        var mundo = await NuevoMundoAsync(context);
        var explora = await NuevoExploraAsync(context, mundo);
        // Una zona que llegó por otra vía con datos rotos (el handler nunca la dejaría pasar).
        await context.RecursoDecisionOpciones.AddRangeAsync(
            new RecursoDecisionOpcion { RecursoDecisionId = explora.RecursoId, OpcionMensaje = "Decorado", Tipo = RecursoSalidaTipos.Zona, Region = RegionValida },
            new RecursoDecisionOpcion { RecursoDecisionId = explora.RecursoId, OpcionMensaje = "Rota", Tipo = RecursoSalidaTipos.Zona, Region = """{"x":99,"y":0,"ancho":50,"alto":5}""" });
        await context.SaveChangesAsync();

        var resultado = await VersionValidator.ValidarAsync(context, mundo.Version.NovelaVersionId, CancellationToken.None);

        Assert.Contains(resultado.Advertencias, a => a.Codigo == "zona_sin_efecto");
        Assert.Contains(resultado.Errores, e => e.Codigo == "zona_invalida");
    }

    [Fact]
    public async Task ValidarVersion_CondicionQueCitaObjetoInexistente_EsError()
    {
        await using var context = TestDb.CreateContext();
        var mundo = await NuevoMundoAsync(context);
        var explora = await NuevoExploraAsync(context, mundo);
        await context.RecursoDecisionOpciones.AddAsync(new RecursoDecisionOpcion
        {
            RecursoDecisionId = explora.RecursoId, OpcionMensaje = "Puerta", Tipo = RecursoSalidaTipos.Zona, Region = RegionValida,
            Condicion = """{"objeto":"fantasma","op":">=","valor":1}""", Efectos = """[{"ir":"patio"}]"""
        });
        await context.SaveChangesAsync();

        var resultado = await VersionValidator.ValidarAsync(context, mundo.Version.NovelaVersionId, CancellationToken.None);

        Assert.Contains(resultado.Errores, e => e.Codigo == "condicion_o_efecto_invalido");
    }

    // ---- Definiciones: fondo de ubicación

    [Fact]
    public async Task UpdateDefiniciones_FondoDeUbicacion_DebePertenecerALaNovela()
    {
        await using var context = TestDb.CreateContext();
        var mundo = await NuevoMundoAsync(context);
        var propio = new Background { Descripcion = "Propio" };
        var ajeno = new Background { Descripcion = "Ajeno" };
        await context.Backgrounds.AddRangeAsync(propio, ajeno);
        var spritePropio = new BackgroundSprite { BackgroundId = propio.BackgroundId, Nombre = "s", DireccionImagen = "/x" };
        var spriteAjeno = new BackgroundSprite { BackgroundId = ajeno.BackgroundId, Nombre = "s", DireccionImagen = "/x" };
        await context.BackgroundSprites.AddRangeAsync(spritePropio, spriteAjeno);
        await context.NovelaBackground.AddAsync(new NovelaBackground { NovelaId = mundo.Novela.NovelaId, BackgroundId = propio.BackgroundId });
        await context.SaveChangesAsync();
        var handler = new UpdateDefinicionesVersion.Handler(context);

        var ok = await handler.HandleAsync(mundo.Version.NovelaVersionId,
            new DefinicionesJuego(new(), new(), new() { new("sala", "Sala", spritePropio.BackgroundSpriteId) }, "sala"), mundo.DuenoId, CancellationToken.None);
        Assert.Equal("sala", ok!.Value.GetProperty("ubicacionInicial").GetString());

        var ex = await Assert.ThrowsAsync<ExceptionHandler>(() => handler.HandleAsync(mundo.Version.NovelaVersionId,
            new DefinicionesJuego(new(), new(), new() { new("sala", "Sala", spriteAjeno.BackgroundSpriteId) }, null), mundo.DuenoId, CancellationToken.None));
        Assert.Equal(HttpStatusCode.BadRequest, ex.Code);
    }

    // ---- Borrador clona zonas y definiciones

    [Fact]
    public async Task CreateDraft_ClonaLasZonasConSuRegion()
    {
        await using var context = TestDb.CreateContext();
        var mundo = await NuevoMundoAsync(context);
        var destino = new Recurso { EscenaId = mundo.Escena.EscenaId, TipoRecurso = RecursoTipos.Conversacion, Contenido = "{}" };
        await context.Recursos.AddAsync(destino);
        var explora = await NuevoExploraAsync(context, mundo);
        await context.RecursoDecisionOpciones.AddAsync(new RecursoDecisionOpcion
        {
            RecursoDecisionId = explora.RecursoId, OpcionMensaje = "Puerta", Tipo = RecursoSalidaTipos.Zona, Region = RegionValida,
            SiguienteRecursoId = destino.RecursoId
        });
        mundo.Version.EsBorrador = false;
        mundo.Version.Disponible = true;
        await context.SaveChangesAsync();

        var draftId = await new CreateDraftFromVersion.Handler(context).HandleAsync(mundo.Novela.NovelaId, mundo.DuenoId, CancellationToken.None);

        var nuevoExplora = await context.Recursos.SingleAsync(r => r.Escena.NovelaVersionId == draftId && r.TipoRecurso == RecursoTipos.Explora);
        var nuevoDestino = await context.Recursos.SingleAsync(r => r.Escena.NovelaVersionId == draftId && r.TipoRecurso == RecursoTipos.Conversacion);
        var zona = await context.RecursoDecisionOpciones.SingleAsync(o => o.RecursoDecisionId == nuevoExplora.RecursoId);
        Assert.Equal(RecursoSalidaTipos.Zona, zona.Tipo);
        Assert.Equal(RegionValida, zona.Region);
        Assert.Equal(nuevoDestino.RecursoId, zona.SiguienteRecursoId);
        Assert.Equal(Definiciones, (await context.NovelaVersiones.FindAsync(draftId))!.Definiciones);
    }
}
