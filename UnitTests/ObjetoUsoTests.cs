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
using WebAPI.Features.Escenas;
using WebAPI.Features.NovelaVersiones;
using WebAPI.Features.Recursos;

namespace UnitTests;

// Los objetos pueden "usarse" desde la mochila: efectos, condición, consumo y un nodo destino.
public class ObjetoUsoTests
{
    private static JsonElement Json(string json) => JsonDocument.Parse(json).RootElement.Clone();

    private static UsoObjetoDef Uso(string efectos = null, string condicion = null, bool consumir = false, Guid? destino = null, string etiqueta = null) =>
        new(etiqueta, condicion == null ? null : Json(condicion), efectos == null ? null : Json(efectos), consumir, destino);

    private static DefinicionesJuego Defs(params ObjetoDef[] objetos) => new(
        new() { new("salud", "Salud", VariableTipos.Numero, null, 0, 100, new(), VariableHud.Oculto) },
        objetos.ToList(),
        new() { new("cueva", "Cueva", null) },
        null,
        new() { new("curioso", "Curioso", "") },
        new());

    private static ObjetoDef Objeto(string id, UsoObjetoDef uso, bool apilable = true) => new(id, id, "", null, apilable, null, 0, uso);

    // ---- Normalización y validación

    [Fact]
    public void Uso_SinEtiqueta_UsaUsar_YSinEfectosVaciosNiCondicionNula()
    {
        var (defs, errores) = MotorJson.NormalizarYValidar(Defs(Objeto("pocion",
            Uso(efectos: "[]", condicion: "null", consumir: true, etiqueta: "  "))));

        Assert.Empty(errores);
        var uso = defs.Objetos![0].Uso!;
        Assert.Equal("Usar", uso.Etiqueta);
        Assert.Null(uso.Efectos);
        Assert.Null(uso.Condicion);
        Assert.True(uso.Consumir);
    }

    [Fact]
    public void Uso_ConEfectosCondicionYEtiqueta_SeConserva()
    {
        var (defs, errores) = MotorJson.NormalizarYValidar(Defs(Objeto("pocion", Uso(
            efectos: """[{"var":"salud","op":"sumar","valor":1},{"logro":"curioso"}]""",
            condicion: """{"en":"cueva"}""",
            etiqueta: "Beber"))));

        Assert.Empty(errores);
        var uso = defs.Objetos![0].Uso!;
        Assert.Equal("Beber", uso.Etiqueta);
        Assert.Equal(2, uso.Efectos!.Value.GetArrayLength());
        Assert.Equal("cueva", uso.Condicion!.Value.GetProperty("en").GetString());
    }

    [Fact]
    public void Uso_QueNoHaceNada_EsError()
    {
        var (_, errores) = MotorJson.NormalizarYValidar(Defs(Objeto("piedra", Uso(efectos: "[]"))));

        var error = Assert.Single(errores);
        Assert.Contains("piedra", error);
        Assert.Contains("no hace nada", error);
    }

    [Fact]
    public void Uso_SoloConsumir_ONavegar_HaceAlgo()
    {
        var (_, errores) = MotorJson.NormalizarYValidar(Defs(
            Objeto("a", Uso(consumir: true)),
            Objeto("b", Uso(destino: Guid.NewGuid()))));

        Assert.Empty(errores);
    }

    [Fact]
    public void Uso_EtiquetaLarga_EsError()
    {
        var (_, errores) = MotorJson.NormalizarYValidar(Defs(Objeto("a", Uso(consumir: true, etiqueta: new string('x', 31)))));

        Assert.Single(errores);
    }

    [Theory]
    [InlineData("""[{"var":"fantasma","op":"sumar","valor":1}]""", null)]
    [InlineData("""[{"ir":"luna"}]""", null)]
    [InlineData("""[{"logro":"nada"}]""", null)]
    [InlineData("""[{"objeto":"nada","op":"dar"}]""", null)]
    [InlineData(null, """{"var":"fantasma","op":">","valor":1}""")]
    [InlineData(null, """{"en":"luna"}""")]
    [InlineData("""[{"var":"salud","op":"explotar","valor":1}]""", null)]
    public void Uso_QueCitaAlgoInexistente_EsErrorYNombraAlObjeto(string efectos, string condicion)
    {
        var (_, errores) = MotorJson.NormalizarYValidar(Defs(Objeto("pocion", Uso(efectos: efectos, condicion: condicion, consumir: true))));

        Assert.NotEmpty(errores);
        Assert.All(errores, e => Assert.Contains("'pocion'", e));
    }

    [Fact]
    public void Uso_PuedeCitarOtrosObjetosDelMismoCatalogo()
    {
        var (_, errores) = MotorJson.NormalizarYValidar(Defs(
            Objeto("botella", null),
            Objeto("pocion", Uso(efectos: """[{"objeto":"botella","op":"dar"}]""", consumir: true))));

        Assert.Empty(errores);
    }

    [Fact]
    public void Uso_SobreviveAlIdaYVueltaPorElJsonb_YLasVersionesViejasNoLoTienen()
    {
        var destino = Guid.NewGuid();
        var (defs, _) = MotorJson.NormalizarYValidar(Defs(Objeto("pocion", Uso(efectos: """[{"var":"salud","op":"sumar","valor":1}]""", destino: destino))));

        var leidas = MotorJson.LeerDefiniciones(MotorJson.Serializar(defs));

        Assert.Equal(destino, leidas.Objetos![0].Uso!.DestinoRecursoId);
        Assert.Equal("sumar", leidas.Objetos[0].Uso!.Efectos!.Value[0].GetProperty("op").GetString());

        var viejas = MotorJson.LeerDefiniciones("""{"variables":[],"objetos":[{"id":"llave","nombre":"Llave","descripcion":"","apilable":false,"inicial":0}]}""");
        Assert.Null(viejas.Objetos![0].Uso);
    }

    // ---- Persistencia: UpdateDefinicionesVersion

    private record Mundo(Guid DuenoId, Novela Novela, NovelaVersion Version, Escena Escena, Recurso Nodo);

    private static async Task<Mundo> NuevoMundoAsync(CreanovelDbContext context, bool borrador = true, string definiciones = null)
    {
        var duenoId = Guid.NewGuid();
        var novela = new Novela { Titulo = "T", UsuarioCreadorId = duenoId };
        await context.Novelas.AddAsync(novela);
        var version = new NovelaVersion
        {
            NovelaId = novela.NovelaId,
            NumeroVersion = "1.0.0",
            EsBorrador = borrador,
            Disponible = !borrador,
            Definiciones = definiciones ?? """{"variables":[]}"""
        };
        await context.NovelaVersiones.AddAsync(version);
        var escena = new Escena { NovelaVersionId = version.NovelaVersionId, PrimerEscena = true };
        await context.Escenas.AddAsync(escena);
        var nodo = new Recurso { EscenaId = escena.EscenaId, TipoRecurso = RecursoTipos.Conversacion, PrimerRecurso = true, Contenido = "{}" };
        await context.Recursos.AddAsync(nodo);
        await context.SaveChangesAsync();
        return new Mundo(duenoId, novela, version, escena, nodo);
    }

    private static async Task<UsoObjetoDef> UsoGuardadoAsync(CreanovelDbContext context, Guid versionId, string objetoId)
    {
        var version = await context.NovelaVersiones.AsNoTracking().SingleAsync(v => v.NovelaVersionId == versionId);
        return MotorJson.LeerDefiniciones(version.Definiciones).Objetos!.Single(o => o.Id == objetoId).Uso;
    }

    [Fact]
    public async Task UpdateDefiniciones_ObjetoQueLlevaAUnNodoDeLaVersion_Guarda()
    {
        await using var context = TestDb.CreateContext();
        var mundo = await NuevoMundoAsync(context);

        await new UpdateDefinicionesVersion.Handler(context).HandleAsync(mundo.Version.NovelaVersionId,
            Defs(Objeto("mapa", Uso(destino: mundo.Nodo.RecursoId))), mundo.DuenoId, CancellationToken.None);

        var uso = await UsoGuardadoAsync(context, mundo.Version.NovelaVersionId, "mapa");
        Assert.Equal(mundo.Nodo.RecursoId, uso.DestinoRecursoId);
    }

    [Fact]
    public async Task UpdateDefiniciones_ObjetoQueLlevaAUnNodoDeOtraVersion_Falla400SinGuardar()
    {
        await using var context = TestDb.CreateContext();
        var mundo = await NuevoMundoAsync(context);
        var ajeno = await NuevoMundoAsync(context);

        var ex = await Assert.ThrowsAsync<ExceptionHandler>(() => new UpdateDefinicionesVersion.Handler(context).HandleAsync(
            mundo.Version.NovelaVersionId, Defs(Objeto("mapa", Uso(destino: ajeno.Nodo.RecursoId))), mundo.DuenoId, CancellationToken.None));

        Assert.Equal(HttpStatusCode.BadRequest, ex.Code);
        Assert.Contains("mapa", JsonSerializer.Serialize(ex.Errors));
        Assert.DoesNotContain("mapa", (await context.NovelaVersiones.FindAsync(mundo.Version.NovelaVersionId))!.Definiciones);
    }

    [Fact]
    public async Task UpdateDefiniciones_ObjetoQueLlevaAUnNodoInexistente_Falla400()
    {
        await using var context = TestDb.CreateContext();
        var mundo = await NuevoMundoAsync(context);

        var ex = await Assert.ThrowsAsync<ExceptionHandler>(() => new UpdateDefinicionesVersion.Handler(context).HandleAsync(
            mundo.Version.NovelaVersionId, Defs(Objeto("mapa", Uso(destino: Guid.NewGuid()))), mundo.DuenoId, CancellationToken.None));

        Assert.Equal(HttpStatusCode.BadRequest, ex.Code);
    }

    // ---- Clonar la versión: el destino sigue al nodo clonado

    [Fact]
    public async Task CreateDraft_RemapeaElDestinoDelObjetoAlNodoClonado()
    {
        await using var context = TestDb.CreateContext();
        var mundo = await NuevoMundoAsync(context, borrador: false);
        mundo.Version.Definiciones = MotorJson.Serializar(Defs(
            Objeto("mapa", Uso(destino: mundo.Nodo.RecursoId)),
            Objeto("pocion", Uso(efectos: """[{"var":"salud","op":"sumar","valor":1}]"""))));
        await context.SaveChangesAsync();

        var draftId = await new CreateDraftFromVersion.Handler(context).HandleAsync(mundo.Novela.NovelaId, mundo.DuenoId, CancellationToken.None);

        var nuevoNodo = await context.Recursos.SingleAsync(r => r.Escena.NovelaVersionId == draftId);
        Assert.NotEqual(mundo.Nodo.RecursoId, nuevoNodo.RecursoId);
        Assert.Equal(nuevoNodo.RecursoId, (await UsoGuardadoAsync(context, draftId, "mapa")).DestinoRecursoId);
        Assert.Null((await UsoGuardadoAsync(context, draftId, "pocion")).DestinoRecursoId);
        // La versión publicada no se toca.
        Assert.Equal(mundo.Nodo.RecursoId, (await UsoGuardadoAsync(context, mundo.Version.NovelaVersionId, "mapa")).DestinoRecursoId);
    }

    [Fact]
    public async Task CreateDraft_SinDestinosEnObjetos_CopiaElTextoTalCual()
    {
        const string definiciones = """{"variables":[],"objetos":[{"id":"llave","nombre":"Llave","descripcion":"","apilable":false,"inicial":0}]}""";
        await using var context = TestDb.CreateContext();
        var mundo = await NuevoMundoAsync(context, borrador: false, definiciones);

        var draftId = await new CreateDraftFromVersion.Handler(context).HandleAsync(mundo.Novela.NovelaId, mundo.DuenoId, CancellationToken.None);

        Assert.Equal(definiciones, (await context.NovelaVersiones.FindAsync(draftId))!.Definiciones);
    }

    // ---- Borrar nodos y escenas: el destino se limpia

    [Fact]
    public async Task DeleteRecurso_QuitaElDestinoDeLosObjetosQueLlevabanAEl()
    {
        await using var context = TestDb.CreateContext();
        var mundo = await NuevoMundoAsync(context);
        var otro = new Recurso { EscenaId = mundo.Escena.EscenaId, TipoRecurso = RecursoTipos.Conversacion, Contenido = "{}" };
        await context.Recursos.AddAsync(otro);
        mundo.Version.Definiciones = MotorJson.Serializar(Defs(
            Objeto("mapa", Uso(destino: mundo.Nodo.RecursoId, consumir: true)),
            Objeto("carta", Uso(destino: otro.RecursoId))));
        await context.SaveChangesAsync();

        await new DeleteRecurso.Handler(context).HandleAsync(mundo.Nodo.RecursoId, mundo.DuenoId, CancellationToken.None);

        var mapa = await UsoGuardadoAsync(context, mundo.Version.NovelaVersionId, "mapa");
        Assert.Null(mapa.DestinoRecursoId);
        Assert.True(mapa.Consumir);   // el resto del uso se conserva
        Assert.Equal(otro.RecursoId, (await UsoGuardadoAsync(context, mundo.Version.NovelaVersionId, "carta")).DestinoRecursoId);
    }

    [Fact]
    public async Task DeleteEscena_QuitaElDestinoDeLosObjetosQueLlevabanASusNodos()
    {
        await using var context = TestDb.CreateContext();
        var mundo = await NuevoMundoAsync(context);
        mundo.Version.Definiciones = MotorJson.Serializar(Defs(Objeto("mapa", Uso(destino: mundo.Nodo.RecursoId, consumir: true))));
        await context.SaveChangesAsync();

        await new DeleteEscena.Handler(context).HandleAsync(mundo.Escena.EscenaId, mundo.DuenoId, CancellationToken.None);

        Assert.Null((await UsoGuardadoAsync(context, mundo.Version.NovelaVersionId, "mapa")).DestinoRecursoId);
    }

    // ---- Publicar: la revisión de la versión

    [Fact]
    public async Task Validar_DestinoQueYaNoExiste_EsError()
    {
        await using var context = TestDb.CreateContext();
        var mundo = await NuevoMundoAsync(context);
        mundo.Version.Definiciones = MotorJson.Serializar(Defs(Objeto("mapa", Uso(destino: Guid.NewGuid()))));
        await context.SaveChangesAsync();

        var resultado = await VersionValidator.ValidarAsync(context, mundo.Version.NovelaVersionId, CancellationToken.None);

        Assert.False(resultado.Valida);
        Assert.Contains(resultado.Errores, e => e.Codigo == "uso_objeto_destino_invalido" && e.Mensaje.Contains("mapa"));
    }

    [Fact]
    public async Task Validar_NodoAlQueSoloLlegaUnObjeto_NoSeAvisaComoInalcanzable()
    {
        await using var context = TestDb.CreateContext();
        var mundo = await NuevoMundoAsync(context);
        var secreto = new Recurso { EscenaId = mundo.Escena.EscenaId, TipoRecurso = RecursoTipos.Conversacion, UltimoRecurso = true, Contenido = "{}" };
        await context.Recursos.AddAsync(secreto);
        mundo.Nodo.UltimoRecurso = true;
        await context.SaveChangesAsync();

        var antes = await VersionValidator.ValidarAsync(context, mundo.Version.NovelaVersionId, CancellationToken.None);
        Assert.Contains(antes.Advertencias, a => a.Codigo == "inalcanzable" && a.RecursoId == secreto.RecursoId);

        mundo.Version.Definiciones = MotorJson.Serializar(Defs(Objeto("mapa", Uso(destino: secreto.RecursoId))));
        await context.SaveChangesAsync();

        var despues = await VersionValidator.ValidarAsync(context, mundo.Version.NovelaVersionId, CancellationToken.None);
        Assert.True(despues.Valida);
        Assert.DoesNotContain(despues.Advertencias, a => a.Codigo == "inalcanzable");
    }

    [Fact]
    public async Task Validar_UsoQueCitaUnaVariableBorrada_BloqueaLaPublicacion()
    {
        await using var context = TestDb.CreateContext();
        var mundo = await NuevoMundoAsync(context);
        // Definiciones guardadas a mano: el objeto cita una variable que ya no existe.
        mundo.Version.Definiciones = """
            {"variables":[],"objetos":[{"id":"pocion","nombre":"Poción","descripcion":"","apilable":true,"inicial":0,
              "uso":{"etiqueta":"Usar","efectos":[{"var":"salud","op":"sumar","valor":1}],"consumir":true}}]}
            """;
        await context.SaveChangesAsync();

        var resultado = await VersionValidator.ValidarAsync(context, mundo.Version.NovelaVersionId, CancellationToken.None);

        Assert.False(resultado.Valida);
        Assert.Contains(resultado.Errores, e => e.Codigo == "definiciones_invalidas" && e.Mensaje.Contains("pocion"));
    }
}
