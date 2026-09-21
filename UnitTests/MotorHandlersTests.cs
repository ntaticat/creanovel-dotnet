using System;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Application.Handlers;
using Domain.Models;
using Microsoft.EntityFrameworkCore;
using Persistence;
using WebAPI.Features.Lecturas;
using WebAPI.Features.NovelaVersiones;
using WebAPI.Features.Recursos;

namespace UnitTests;

public class MotorHandlersTests
{
    private const string DefinicionesConSalud =
        """{"variables":[{"clave":"salud","etiqueta":"Salud","tipo":"numero","inicial":50,"min":0,"max":100,"valores":[],"hud":"barra"}]}""";

    private static JsonElement Json(string json) => JsonDocument.Parse(json).RootElement.Clone();

    private record Mundo(Guid DuenoId, Novela Novela, NovelaVersion Version, Escena Escena);

    private static async Task<Mundo> NuevoMundoAsync(CreanovelDbContext context, bool borrador = true)
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
            Definiciones = DefinicionesConSalud
        };
        await context.NovelaVersiones.AddAsync(version);
        var escena = new Escena { NovelaVersionId = version.NovelaVersionId, PrimerEscena = true };
        await context.Escenas.AddAsync(escena);
        await context.SaveChangesAsync();
        return new Mundo(duenoId, novela, version, escena);
    }

    private static async Task<ExceptionHandler> Falla(Func<Task> accion)
    {
        return await Assert.ThrowsAsync<ExceptionHandler>(accion);
    }

    // ---- CreateRecurso / UpdateRecurso (nodos Asigna y Evalua)

    [Fact]
    public async Task CreateRecurso_Asigna_GuardaEfectosValidados()
    {
        await using var context = TestDb.CreateContext();
        var mundo = await NuevoMundoAsync(context);
        var handler = new CreateRecurso.Handler(context);

        var id = await handler.HandleAsync(new CreateRecurso.CreateRecursoRequest(
            mundo.Escena.EscenaId, RecursoTipos.Asigna, false, false, null, null, null,
            Json("""{"efectos":[{"var":"salud","op":"restar","valor":10}]}""")), mundo.DuenoId, CancellationToken.None);

        var recurso = await context.Recursos.SingleAsync(r => r.RecursoId == id);
        var dto = (RecursoAsignaDto)RecursoMapping.ToDto(recurso);
        Assert.Equal(1, dto.Efectos.GetArrayLength());
        Assert.Equal("restar", dto.Efectos[0].GetProperty("op").GetString());
    }

    [Fact]
    public async Task CreateRecurso_AsignaConVariableInexistente_Falla400()
    {
        await using var context = TestDb.CreateContext();
        var mundo = await NuevoMundoAsync(context);
        var handler = new CreateRecurso.Handler(context);

        var ex = await Falla(() => handler.HandleAsync(new CreateRecurso.CreateRecursoRequest(
            mundo.Escena.EscenaId, RecursoTipos.Asigna, false, false, null, null, null,
            Json("""{"efectos":[{"var":"fantasma","op":"sumar","valor":1}]}""")), mundo.DuenoId, CancellationToken.None));

        Assert.Equal(HttpStatusCode.BadRequest, ex.Code);
        Assert.Empty(context.Recursos);
    }

    [Fact]
    public async Task CreateRecurso_TipoNoGenerico_Falla400()
    {
        await using var context = TestDb.CreateContext();
        var mundo = await NuevoMundoAsync(context);
        var handler = new CreateRecurso.Handler(context);

        var ex = await Falla(() => handler.HandleAsync(new CreateRecurso.CreateRecursoRequest(
            mundo.Escena.EscenaId, RecursoTipos.Conversacion, false, false, null, null, null, null), mundo.DuenoId, CancellationToken.None));

        Assert.Equal(HttpStatusCode.BadRequest, ex.Code);
    }

    [Fact]
    public async Task CreateRecurso_DeOtroUsuario_Falla403()
    {
        await using var context = TestDb.CreateContext();
        var mundo = await NuevoMundoAsync(context);
        var handler = new CreateRecurso.Handler(context);

        var ex = await Falla(() => handler.HandleAsync(new CreateRecurso.CreateRecursoRequest(
            mundo.Escena.EscenaId, RecursoTipos.Evalua, false, false, null, null, null, null), Guid.NewGuid(), CancellationToken.None));

        Assert.Equal(HttpStatusCode.Forbidden, ex.Code);
    }

    [Fact]
    public async Task CreateRecurso_SiguienteDeOtraVersion_Falla400()
    {
        await using var context = TestDb.CreateContext();
        var mundo = await NuevoMundoAsync(context);
        var otro = await NuevoMundoAsync(context);
        var ajeno = new Recurso { EscenaId = otro.Escena.EscenaId, TipoRecurso = RecursoTipos.Conversacion };
        await context.Recursos.AddAsync(ajeno);
        await context.SaveChangesAsync();
        var handler = new CreateRecurso.Handler(context);

        var ex = await Falla(() => handler.HandleAsync(new CreateRecurso.CreateRecursoRequest(
            mundo.Escena.EscenaId, RecursoTipos.Evalua, false, false, ajeno.RecursoId, null, null, null), mundo.DuenoId, CancellationToken.None));

        Assert.Equal(HttpStatusCode.BadRequest, ex.Code);
    }

    [Fact]
    public async Task UpdateRecurso_Evalua_ReemplazaElSino()
    {
        await using var context = TestDb.CreateContext();
        var mundo = await NuevoMundoAsync(context);
        var destino = new Recurso { EscenaId = mundo.Escena.EscenaId, TipoRecurso = RecursoTipos.Conversacion };
        var evalua = new Recurso { EscenaId = mundo.Escena.EscenaId, TipoRecurso = RecursoTipos.Evalua };
        await context.Recursos.AddRangeAsync(destino, evalua);
        await context.SaveChangesAsync();
        var handler = new UpdateRecurso.Handler(context);

        await handler.HandleAsync(evalua.RecursoId, new UpdateRecurso.UpdateRecursoRequest(null, null, destino.RecursoId, null, null, null), mundo.DuenoId, CancellationToken.None);
        Assert.Equal(destino.RecursoId, (await context.Recursos.FindAsync(evalua.RecursoId))!.SiguienteRecursoId);

        // El editor manda el estado completo: null limpia el "sino".
        await handler.HandleAsync(evalua.RecursoId, new UpdateRecurso.UpdateRecursoRequest(null, null, null, null, null, null), mundo.DuenoId, CancellationToken.None);
        Assert.Null((await context.Recursos.FindAsync(evalua.RecursoId))!.SiguienteRecursoId);
    }

    [Fact]
    public async Task UpdateRecursoEntrada_ReemplazaSiguienteYSprites_NullLosLimpia()
    {
        await using var context = TestDb.CreateContext();
        var mundo = await NuevoMundoAsync(context);
        var destino = new Recurso { EscenaId = mundo.Escena.EscenaId, TipoRecurso = RecursoTipos.Conversacion };
        await context.Recursos.AddAsync(destino);
        var entrada = new Recurso
        {
            EscenaId = mundo.Escena.EscenaId,
            TipoRecurso = RecursoTipos.Entrada,
            SiguienteRecursoId = destino.RecursoId,
            Contenido = JsonSerializer.Serialize(new EntradaContenido("¿Nombre?", "nombre", "", ""))
        };
        await context.Recursos.AddAsync(entrada);
        await context.SaveChangesAsync();

        await new UpdateRecursoEntrada.Handler(context).HandleAsync(entrada.RecursoId,
            new UpdateRecursoEntrada.UpdateRecursoEntradaRequest("¿Cómo te llamas?", null!, null!, null!, null, null, null, null, null),
            mundo.DuenoId, CancellationToken.None);

        var guardada = await context.Recursos.FindAsync(entrada.RecursoId);
        Assert.Null(guardada!.SiguienteRecursoId);
        var dto = (RecursoEntradaDto)RecursoMapping.ToDto(guardada);
        Assert.Equal("¿Cómo te llamas?", dto.Etiqueta);
        Assert.Equal("nombre", dto.Clave);   // los campos de texto omitidos se conservan
    }

    // ---- Opciones con condición y efectos

    [Fact]
    public async Task AddOpcion_EnEvalua_CreaRamasOrdenadasConTipoRama()
    {
        await using var context = TestDb.CreateContext();
        var mundo = await NuevoMundoAsync(context);
        var evalua = new Recurso { EscenaId = mundo.Escena.EscenaId, TipoRecurso = RecursoTipos.Evalua };
        await context.Recursos.AddAsync(evalua);
        await context.SaveChangesAsync();
        var handler = new AddRecursoDecisionOpcion.Handler(context);

        await handler.HandleAsync(new AddRecursoDecisionOpcion.AddRecursoDecisionOpcionRequest(
            "", null, evalua.RecursoId, null, Json("""{"var":"salud","op":">=","valor":50}"""), null, null), mundo.DuenoId, CancellationToken.None);
        await handler.HandleAsync(new AddRecursoDecisionOpcion.AddRecursoDecisionOpcionRequest(
            "", null, evalua.RecursoId, null, Json("""{"var":"salud","op":"<","valor":50}"""), null, null), mundo.DuenoId, CancellationToken.None);

        var ramas = await context.RecursoDecisionOpciones.Where(o => o.RecursoDecisionId == evalua.RecursoId).OrderBy(o => o.Orden).ToListAsync();
        Assert.Equal(new[] { 0, 1 }, ramas.Select(r => r.Orden));
        Assert.All(ramas, r => Assert.Equal(RecursoSalidaTipos.Rama, r.Tipo));
        Assert.All(ramas, r => Assert.Equal(CondicionModos.Ocultar, r.CondicionModo));
    }

    [Fact]
    public async Task AddOpcion_EnSeleccionaConCondicionYEfectos_LosPersiste()
    {
        await using var context = TestDb.CreateContext();
        var mundo = await NuevoMundoAsync(context);
        var decision = new Recurso { EscenaId = mundo.Escena.EscenaId, TipoRecurso = RecursoTipos.Decision };
        await context.Recursos.AddAsync(decision);
        await context.SaveChangesAsync();
        var handler = new AddRecursoDecisionOpcion.Handler(context);

        await handler.HandleAsync(new AddRecursoDecisionOpcion.AddRecursoDecisionOpcionRequest(
            "Descansar", null, decision.RecursoId, 3,
            Json("""{"var":"salud","op":"<","valor":100}"""), CondicionModos.Deshabilitar,
            Json("""[{"var":"salud","op":"sumar","valor":20}]""")), mundo.DuenoId, CancellationToken.None);

        var opcion = await context.RecursoDecisionOpciones.SingleAsync();
        Assert.Equal(RecursoSalidaTipos.Opcion, opcion.Tipo);
        Assert.Equal(3, opcion.Orden);
        Assert.Equal(CondicionModos.Deshabilitar, opcion.CondicionModo);
        Assert.Contains("\"salud\"", opcion.Condicion);
        Assert.Contains("\"sumar\"", opcion.Efectos);

        var dto = ((RecursoDecisionDto)RecursoMapping.ToDto(await context.Recursos.Include(r => r.Opciones).SingleAsync())).Opciones.Single();
        Assert.Equal("<", dto.Condicion!.Value.GetProperty("op").GetString());
    }

    [Fact]
    public async Task AddOpcion_ConCondicionInvalida_Falla400()
    {
        await using var context = TestDb.CreateContext();
        var mundo = await NuevoMundoAsync(context);
        var decision = new Recurso { EscenaId = mundo.Escena.EscenaId, TipoRecurso = RecursoTipos.Decision };
        await context.Recursos.AddAsync(decision);
        await context.SaveChangesAsync();
        var handler = new AddRecursoDecisionOpcion.Handler(context);

        var ex = await Falla(() => handler.HandleAsync(new AddRecursoDecisionOpcion.AddRecursoDecisionOpcionRequest(
            "x", null, decision.RecursoId, null, Json("""{"eval":"alert(1)"}"""), null, null), mundo.DuenoId, CancellationToken.None));

        Assert.Equal(HttpStatusCode.BadRequest, ex.Code);
        Assert.Empty(context.RecursoDecisionOpciones);
    }

    [Fact]
    public async Task UpdateOpcion_ReemplazaCondicionYEfectos_ConNullLosLimpia()
    {
        await using var context = TestDb.CreateContext();
        var mundo = await NuevoMundoAsync(context);
        var decision = new Recurso { EscenaId = mundo.Escena.EscenaId, TipoRecurso = RecursoTipos.Decision };
        await context.Recursos.AddAsync(decision);
        var opcion = new RecursoDecisionOpcion
        {
            RecursoDecisionId = decision.RecursoId, OpcionMensaje = "A", Condicion = """{"var":"salud","op":">","valor":1}""", Orden = 2
        };
        await context.RecursoDecisionOpciones.AddAsync(opcion);
        await context.SaveChangesAsync();
        var handler = new UpdateRecursoDecisionOpcion.Handler(context);

        await handler.HandleAsync(opcion.RecursoDecisionOpcionId,
            new UpdateRecursoDecisionOpcion.UpdateRecursoDecisionOpcionRequest("B", null, null, null, null, null), mundo.DuenoId, CancellationToken.None);

        var guardada = await context.RecursoDecisionOpciones.SingleAsync();
        Assert.Equal("B", guardada.OpcionMensaje);
        Assert.Equal(2, guardada.Orden);      // Orden se conserva si no se envía
        Assert.Null(guardada.Condicion);      // la condición se reemplaza (null = sin condición)
    }

    // ---- Definiciones

    [Fact]
    public async Task UpdateDefiniciones_GuardaNormalizadas()
    {
        await using var context = TestDb.CreateContext();
        var mundo = await NuevoMundoAsync(context);
        var handler = new UpdateDefinicionesVersion.Handler(context);

        var resultado = await handler.HandleAsync(mundo.Version.NovelaVersionId,
            new Application.Motor.DefinicionesJuego(new()
            {
                new("afecto", null!, Application.Motor.VariableTipos.Numero, null, 0, 100, null!, Application.Motor.VariableHud.Barra)
            }), mundo.DuenoId, CancellationToken.None);

        var variable = resultado!.Value.GetProperty("variables")[0];
        Assert.Equal("afecto", variable.GetProperty("etiqueta").GetString());
        Assert.Equal(0, variable.GetProperty("inicial").GetDouble());
        Assert.Contains("afecto", (await context.NovelaVersiones.FindAsync(mundo.Version.NovelaVersionId))!.Definiciones);
    }

    [Fact]
    public async Task UpdateDefiniciones_VersionPublicada_Falla400()
    {
        await using var context = TestDb.CreateContext();
        var mundo = await NuevoMundoAsync(context, borrador: false);
        var handler = new UpdateDefinicionesVersion.Handler(context);

        var ex = await Falla(() => handler.HandleAsync(mundo.Version.NovelaVersionId,
            new Application.Motor.DefinicionesJuego(new()), mundo.DuenoId, CancellationToken.None));

        Assert.Equal(HttpStatusCode.BadRequest, ex.Code);
    }

    [Fact]
    public async Task UpdateDefiniciones_Invalidas_Falla400SinGuardar()
    {
        await using var context = TestDb.CreateContext();
        var mundo = await NuevoMundoAsync(context);
        var handler = new UpdateDefinicionesVersion.Handler(context);

        var ex = await Falla(() => handler.HandleAsync(mundo.Version.NovelaVersionId,
            new Application.Motor.DefinicionesJuego(new()
            {
                new("Mala Clave", null!, Application.Motor.VariableTipos.Numero, null, null, null, null!, null!)
            }), mundo.DuenoId, CancellationToken.None));

        Assert.Equal(HttpStatusCode.BadRequest, ex.Code);
        Assert.Equal(DefinicionesConSalud, (await context.NovelaVersiones.FindAsync(mundo.Version.NovelaVersionId))!.Definiciones);
    }

    [Fact]
    public async Task UpdateDefiniciones_DeOtroUsuario_Falla403()
    {
        await using var context = TestDb.CreateContext();
        var mundo = await NuevoMundoAsync(context);
        var handler = new UpdateDefinicionesVersion.Handler(context);

        var ex = await Falla(() => handler.HandleAsync(mundo.Version.NovelaVersionId,
            new Application.Motor.DefinicionesJuego(new()), Guid.NewGuid(), CancellationToken.None));

        Assert.Equal(HttpStatusCode.Forbidden, ex.Code);
    }

    // ---- Publicación

    [Fact]
    public async Task Publish_ConErroresDeValidacion_NoPublica()
    {
        await using var context = TestDb.CreateContext();
        var mundo = await NuevoMundoAsync(context);
        await context.Recursos.AddAsync(new Recurso { EscenaId = mundo.Escena.EscenaId, TipoRecurso = RecursoTipos.Evalua, PrimerRecurso = true });
        await context.SaveChangesAsync();
        var handler = new PublishNovelaVersion.Handler(context);

        var ex = await Falla(() => handler.HandleAsync(mundo.Version.NovelaVersionId, mundo.DuenoId, CancellationToken.None));

        Assert.Equal(HttpStatusCode.BadRequest, ex.Code);
        var version = await context.NovelaVersiones.FindAsync(mundo.Version.NovelaVersionId);
        Assert.True(version!.EsBorrador);
        Assert.False(version.Disponible);
    }

    [Fact]
    public async Task Publish_VersionLegacySinNodosNuevos_SigueSiendoPublicable()
    {
        await using var context = TestDb.CreateContext();
        var mundo = await NuevoMundoAsync(context);
        await context.Recursos.AddAsync(new Recurso
        {
            EscenaId = mundo.Escena.EscenaId, TipoRecurso = RecursoTipos.Conversacion, PrimerRecurso = true,
            Contenido = JsonSerializer.Serialize(new ConversacionContenido("Hola", null!))
        });
        await context.SaveChangesAsync();
        var handler = new PublishNovelaVersion.Handler(context);

        await handler.HandleAsync(mundo.Version.NovelaVersionId, mundo.DuenoId, CancellationToken.None);

        Assert.True((await context.NovelaVersiones.FindAsync(mundo.Version.NovelaVersionId))!.Disponible);
    }

    // ---- Borrador clona el motor

    [Fact]
    public async Task CreateDraft_ClonaDefinicionesRamasCondicionesYEfectos()
    {
        await using var context = TestDb.CreateContext();
        var mundo = await NuevoMundoAsync(context, borrador: false);
        var destino = new Recurso { EscenaId = mundo.Escena.EscenaId, TipoRecurso = RecursoTipos.Conversacion, Contenido = "{}" };
        await context.Recursos.AddAsync(destino);
        var evalua = new Recurso { EscenaId = mundo.Escena.EscenaId, TipoRecurso = RecursoTipos.Evalua, SiguienteRecursoId = destino.RecursoId };
        await context.Recursos.AddAsync(evalua);
        await context.RecursoDecisionOpciones.AddAsync(new RecursoDecisionOpcion
        {
            RecursoDecisionId = evalua.RecursoId,
            SiguienteRecursoId = destino.RecursoId,
            Tipo = RecursoSalidaTipos.Rama,
            Orden = 4,
            Condicion = """{"var":"salud","op":">=","valor":50}""",
            CondicionModo = CondicionModos.Deshabilitar,
            Efectos = """[{"var":"salud","op":"sumar","valor":1}]"""
        });
        await context.SaveChangesAsync();

        var draftId = await new CreateDraftFromVersion.Handler(context).HandleAsync(mundo.Novela.NovelaId, mundo.DuenoId, CancellationToken.None);

        var draft = await context.NovelaVersiones.FindAsync(draftId);
        Assert.Equal(DefinicionesConSalud, draft!.Definiciones);

        var nuevaEvalua = await context.Recursos.SingleAsync(r => r.Escena.NovelaVersionId == draftId && r.TipoRecurso == RecursoTipos.Evalua);
        var nuevoDestino = await context.Recursos.SingleAsync(r => r.Escena.NovelaVersionId == draftId && r.TipoRecurso == RecursoTipos.Conversacion);
        Assert.Equal(nuevoDestino.RecursoId, nuevaEvalua.SiguienteRecursoId);

        var rama = await context.RecursoDecisionOpciones.SingleAsync(o => o.RecursoDecisionId == nuevaEvalua.RecursoId);
        Assert.Equal(nuevoDestino.RecursoId, rama.SiguienteRecursoId);
        Assert.Equal(RecursoSalidaTipos.Rama, rama.Tipo);
        Assert.Equal(4, rama.Orden);
        Assert.Equal(CondicionModos.Deshabilitar, rama.CondicionModo);
        Assert.Contains("\"salud\"", rama.Condicion);
        Assert.Contains("\"sumar\"", rama.Efectos);
    }

    // ---- Lecturas

    private static async Task<(Guid UsuarioId, Lectura Lectura, Recurso Recurso)> NuevaLecturaAsync(CreanovelDbContext context)
    {
        var mundo = await NuevoMundoAsync(context, borrador: false);
        var recurso = new Recurso { EscenaId = mundo.Escena.EscenaId, TipoRecurso = RecursoTipos.Conversacion };
        var jugadorId = Guid.NewGuid();
        var lectura = new Lectura { NovelaRegistrosId = mundo.Novela.NovelaId, UsuarioPropietarioId = jugadorId };
        await context.Recursos.AddAsync(recurso);
        await context.Lecturas.AddAsync(lectura);
        await context.SaveChangesAsync();
        return (jugadorId, lectura, recurso);
    }

    [Fact]
    public async Task AddRecursoToLectura_PermiteVolverAPasarPorElMismoRecurso()
    {
        await using var context = TestDb.CreateContext();
        var (jugadorId, lectura, recurso) = await NuevaLecturaAsync(context);
        var handler = new AddRecursoToLectura.Handler(context);

        await handler.HandleAsync(new AddRecursoToLectura.AddRecursoToLecturaRequest(lectura.LecturaId, recurso.RecursoId, 1), jugadorId, CancellationToken.None);
        await handler.HandleAsync(new AddRecursoToLectura.AddRecursoToLecturaRequest(lectura.LecturaId, recurso.RecursoId, 2), jugadorId, CancellationToken.None);

        Assert.Equal(2, await context.LecturaRecurso.CountAsync(lr => lr.LecturaId == lectura.LecturaId && lr.RecursoId == recurso.RecursoId));
    }

    [Fact]
    public async Task AddRecursoToLectura_MismoOrdenDosVeces_EsIdempotente()
    {
        await using var context = TestDb.CreateContext();
        var (jugadorId, lectura, recurso) = await NuevaLecturaAsync(context);
        var handler = new AddRecursoToLectura.Handler(context);
        var request = new AddRecursoToLectura.AddRecursoToLecturaRequest(lectura.LecturaId, recurso.RecursoId, 1);

        await handler.HandleAsync(request, jugadorId, CancellationToken.None);
        await handler.HandleAsync(request, jugadorId, CancellationToken.None);

        Assert.Equal(1, await context.LecturaRecurso.CountAsync());
    }

    [Fact]
    public async Task AddRecursoToLectura_LecturaAjena_Falla403()
    {
        await using var context = TestDb.CreateContext();
        var (_, lectura, recurso) = await NuevaLecturaAsync(context);
        var handler = new AddRecursoToLectura.Handler(context);

        var ex = await Falla(() => handler.HandleAsync(new AddRecursoToLectura.AddRecursoToLecturaRequest(lectura.LecturaId, recurso.RecursoId, 1), Guid.NewGuid(), CancellationToken.None));

        Assert.Equal(HttpStatusCode.Forbidden, ex.Code);
    }

    [Fact]
    public async Task RemoveRecursoFromLectura_QuitaTodosLosPasosDelRecurso()
    {
        await using var context = TestDb.CreateContext();
        var (jugadorId, lectura, recurso) = await NuevaLecturaAsync(context);
        await context.LecturaRecurso.AddRangeAsync(
            new LecturaRecursos { LecturaId = lectura.LecturaId, RecursoId = recurso.RecursoId, RecursoOrder = 1 },
            new LecturaRecursos { LecturaId = lectura.LecturaId, RecursoId = recurso.RecursoId, RecursoOrder = 2 });
        await context.SaveChangesAsync();

        await new RemoveRecursoFromLectura.Handler(context).HandleAsync(new RemoveRecursoFromLectura.RemoveRecursoFromLecturaRequest(lectura.LecturaId, recurso.RecursoId), jugadorId, CancellationToken.None);

        Assert.Empty(context.LecturaRecurso);
    }

    [Fact]
    public async Task CreateLectura_UsaElUsuarioAutenticadoNoElDelCuerpo()
    {
        await using var context = TestDb.CreateContext();
        var mundo = await NuevoMundoAsync(context, borrador: false);
        var autenticado = Guid.NewGuid();

        var id = await new CreateLectura.Handler(context).HandleAsync(
            new CreateLectura.CreateLecturaRequest(mundo.Novela.NovelaId, Guid.NewGuid(), mundo.Version.NovelaVersionId), autenticado, CancellationToken.None);

        var lectura = await context.Lecturas.FindAsync(id);
        Assert.Equal(autenticado, lectura!.UsuarioPropietarioId);
        Assert.Equal(mundo.Version.NovelaVersionId, lectura.NovelaVersionId);
    }

    [Fact]
    public async Task SaveLecturaEstado_GuardaEstadoYRecursoActual()
    {
        await using var context = TestDb.CreateContext();
        var (jugadorId, lectura, recurso) = await NuevaLecturaAsync(context);
        var handler = new SaveLecturaEstado.Handler(context);

        await handler.HandleAsync(lectura.LecturaId,
            new SaveLecturaEstado.SaveLecturaEstadoRequest(Json("""{"vars":{"salud":80}}"""), recurso.RecursoId, null), jugadorId, CancellationToken.None);

        var guardada = await context.Lecturas.FindAsync(lectura.LecturaId);
        Assert.Contains("\"salud\":80", guardada!.Estado);
        Assert.Equal(recurso.RecursoId, guardada.RecursoActualId);
    }

    [Fact]
    public async Task SaveLecturaEstado_EstadoNoObjetoOAjeno_Falla()
    {
        await using var context = TestDb.CreateContext();
        var (jugadorId, lectura, _) = await NuevaLecturaAsync(context);
        var handler = new SaveLecturaEstado.Handler(context);

        var noObjeto = await Falla(() => handler.HandleAsync(lectura.LecturaId,
            new SaveLecturaEstado.SaveLecturaEstadoRequest(Json("[1]"), null, null), jugadorId, CancellationToken.None));
        Assert.Equal(HttpStatusCode.BadRequest, noObjeto.Code);

        var ajeno = await Falla(() => handler.HandleAsync(lectura.LecturaId,
            new SaveLecturaEstado.SaveLecturaEstadoRequest(Json("{}"), null, null), Guid.NewGuid(), CancellationToken.None));
        Assert.Equal(HttpStatusCode.Forbidden, ajeno.Code);
    }

    [Fact]
    public async Task SaveLecturaEstado_DemasiadoGrande_Falla400()
    {
        await using var context = TestDb.CreateContext();
        var (jugadorId, lectura, _) = await NuevaLecturaAsync(context);
        var grande = "{\"x\":\"" + new string('a', SaveLecturaEstado.MaxBytesEstado) + "\"}";

        var ex = await Falla(() => new SaveLecturaEstado.Handler(context).HandleAsync(lectura.LecturaId,
            new SaveLecturaEstado.SaveLecturaEstadoRequest(Json(grande), null, null), jugadorId, CancellationToken.None));

        Assert.Equal(HttpStatusCode.BadRequest, ex.Code);
    }
}
