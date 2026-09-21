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

public class JuegaTests
{
    private const string Definiciones = """
        {"variables":[{"clave":"puntos","tipo":"numero","valores":[],"hud":"oculto"},{"clave":"nombre","tipo":"texto","valores":[],"hud":"oculto"}],
         "objetos":[{"id":"llave","nombre":"Llave","descripcion":"","apilable":false,"inicial":0}],"ubicaciones":[]}
        """;

    private static JsonElement Json(string json) => JsonDocument.Parse(json).RootElement.Clone();

    // ---- MinijuegoValidador

    private static List<string> Validar(string json)
    {
        var errores = new List<string>();
        MinijuegoValidador.Validar(Json(json), "m", errores);
        return errores;
    }

    [Theory]
    [InlineData("""{"tipo":"reflejo","objetivos":5,"aciertosNecesarios":4,"duracionMs":1500}""")]
    [InlineData("""{"tipo":"precision","velocidad":5,"anchoZona":20,"intentos":3}""")]
    [InlineData("""{"tipo":"secuencia","longitud":5,"tiempoMs":6000}""")]
    [InlineData("""{"tipo":"pulsaciones","objetivo":20,"tiempoMs":5000}""")]
    [InlineData("""{"tipo":"reflejo","objetivos":1,"aciertosNecesarios":1,"duracionMs":400}""")]        // límites inferiores
    [InlineData("""{"tipo":"pulsaciones","objetivo":200,"tiempoMs":30000}""")]                          // límites superiores
    public void Minijuego_Valido_NoTieneErrores(string json) => Assert.Empty(Validar(json));

    [Theory]
    [InlineData("""{"tipo":"reflejo","objetivos":0,"aciertosNecesarios":1,"duracionMs":1500}""")]      // pocos objetivos
    [InlineData("""{"tipo":"reflejo","objetivos":21,"aciertosNecesarios":1,"duracionMs":1500}""")]     // demasiados
    [InlineData("""{"tipo":"reflejo","objetivos":3,"aciertosNecesarios":4,"duracionMs":1500}""")]      // más aciertos que objetivos
    [InlineData("""{"tipo":"reflejo","objetivos":3,"aciertosNecesarios":2,"duracionMs":100}""")]       // demasiado rápido
    [InlineData("""{"tipo":"reflejo","objetivos":3,"aciertosNecesarios":2}""")]                        // falta un dato
    [InlineData("""{"tipo":"reflejo","objetivos":3.5,"aciertosNecesarios":2,"duracionMs":1500}""")]    // no entero
    [InlineData("""{"tipo":"reflejo","objetivos":"3","aciertosNecesarios":2,"duracionMs":1500}""")]    // texto
    [InlineData("""{"tipo":"precision","velocidad":11,"anchoZona":20,"intentos":3}""")]
    [InlineData("""{"tipo":"precision","velocidad":5,"anchoZona":4,"intentos":3}""")]
    [InlineData("""{"tipo":"secuencia","longitud":13,"tiempoMs":6000}""")]
    [InlineData("""{"tipo":"secuencia","longitud":5,"tiempoMs":999}""")]
    [InlineData("""{"tipo":"pulsaciones","objetivo":2,"tiempoMs":5000}""")]
    [InlineData("""{"tipo":"pulsaciones","objetivo":20,"tiempoMs":60000}""")]
    [InlineData("""{"tipo":"secuencia","longitud":5,"tiempoMs":6000,"extra":1}""")]                   // propiedad ajena
    [InlineData("""{"tipo":"reflejo","longitud":5,"tiempoMs":6000}""")]                              // propiedades de otro tipo
    [InlineData("""{"tipo":"ajedrez"}""")]                                                            // tipo desconocido
    [InlineData("""{"objetivos":3}""")]                                                               // sin tipo
    [InlineData("""{"tipo":7}""")]
    [InlineData("""[1,2]""")]
    public void Minijuego_Invalido_ReportaError(string json) => Assert.NotEmpty(Validar(json));

    // ---- Creación del nodo y de sus salidas

    private record Mundo(Guid DuenoId, Novela Novela, NovelaVersion Version, Escena Escena);

    private static async Task<Mundo> NuevoMundoAsync(CreanovelDbContext context)
    {
        var duenoId = Guid.NewGuid();
        var novela = new Novela { Titulo = "T", UsuarioCreadorId = duenoId };
        await context.Novelas.AddAsync(novela);
        var version = new NovelaVersion { NovelaId = novela.NovelaId, NumeroVersion = "1.0.0", EsBorrador = true, Definiciones = Definiciones };
        await context.NovelaVersiones.AddAsync(version);
        var escena = new Escena { NovelaVersionId = version.NovelaVersionId, PrimerEscena = true };
        await context.Escenas.AddAsync(escena);
        await context.SaveChangesAsync();
        return new Mundo(duenoId, novela, version, escena);
    }

    private const string Reflejo = """{"tipo":"reflejo","objetivos":5,"aciertosNecesarios":4,"duracionMs":1500}""";

    private static Task<Guid> CrearJuega(CreanovelDbContext context, Mundo mundo, string contenido, Guid? siguiente = null) =>
        new CreateRecurso.Handler(context).HandleAsync(new CreateRecurso.CreateRecursoRequest(
            mundo.Escena.EscenaId, RecursoTipos.Juega, true, false, siguiente, null, null, Json(contenido)), mundo.DuenoId, CancellationToken.None);

    private static string Contenido(string minijuego = Reflejo, string mensaje = "¡Rápido!", string? variable = null) =>
        $$"""{"mensaje":"{{mensaje}}","minijuego":{{minijuego}}{{(variable == null ? "" : $",\"variableResultado\":\"{variable}\"")}}}""";

    private static AddRecursoDecisionOpcion.AddRecursoDecisionOpcionRequest Salida(
        Guid juegaId, string? tipo, Guid? destino = null, string? condicion = null, string? efectos = null) =>
        new("", destino, juegaId, null, condicion == null ? null : Json(condicion), null, efectos == null ? null : Json(efectos), null, tipo);

    [Fact]
    public async Task CreateRecurso_Juega_GuardaElMinijuegoYElMapeoLoDevuelve()
    {
        await using var context = TestDb.CreateContext();
        var mundo = await NuevoMundoAsync(context);

        var id = await CrearJuega(context, mundo, Contenido(variable: "puntos"));

        var dto = (RecursoJuegaDto)RecursoMapping.ToDto(await context.Recursos.FindAsync(id));
        Assert.Equal("¡Rápido!", dto.Mensaje);
        Assert.Equal("puntos", dto.VariableResultado);
        Assert.Equal("reflejo", dto.Minijuego!.Value.GetProperty("tipo").GetString());
        Assert.Equal(5, dto.Minijuego.Value.GetProperty("objetivos").GetInt32());
    }

    [Fact]
    public async Task CreateRecurso_Juega_IgnoraElSiguiente()
    {
        await using var context = TestDb.CreateContext();
        var mundo = await NuevoMundoAsync(context);
        var otro = new Recurso { EscenaId = mundo.Escena.EscenaId, TipoRecurso = RecursoTipos.Conversacion };
        await context.Recursos.AddAsync(otro);
        await context.SaveChangesAsync();

        var id = await CrearJuega(context, mundo, Contenido(), otro.RecursoId);

        Assert.Null((await context.Recursos.FindAsync(id))!.SiguienteRecursoId);
    }

    [Theory]
    [InlineData("""{"mensaje":"x"}""")]                                                                       // sin minijuego
    [InlineData("""{"minijuego":{"tipo":"reflejo","objetivos":99,"aciertosNecesarios":1,"duracionMs":1500}}""")] // fuera de límites
    [InlineData("""{"minijuego":{"tipo":"secuencia","longitud":5,"tiempoMs":6000},"variableResultado":"nombre"}""")] // variable de texto
    [InlineData("""{"minijuego":{"tipo":"secuencia","longitud":5,"tiempoMs":6000},"variableResultado":"fantasma"}""")] // inexistente
    public async Task CreateRecurso_Juega_Invalido_Falla400(string contenido)
    {
        await using var context = TestDb.CreateContext();
        var mundo = await NuevoMundoAsync(context);

        var ex = await Assert.ThrowsAsync<ExceptionHandler>(() => CrearJuega(context, mundo, contenido));

        Assert.Equal(HttpStatusCode.BadRequest, ex.Code);
        Assert.Empty(context.Recursos);
    }

    [Fact]
    public async Task UpdateRecurso_Juega_CambiaElMinijuego()
    {
        await using var context = TestDb.CreateContext();
        var mundo = await NuevoMundoAsync(context);
        var id = await CrearJuega(context, mundo, Contenido());

        await new UpdateRecurso.Handler(context).HandleAsync(id, new UpdateRecurso.UpdateRecursoRequest(
            null, null, null, null, null, Json(Contenido("""{"tipo":"pulsaciones","objetivo":30,"tiempoMs":4000}""", "Machaca"))), mundo.DuenoId, CancellationToken.None);

        var dto = (RecursoJuegaDto)RecursoMapping.ToDto(await context.Recursos.FindAsync(id));
        Assert.Equal("pulsaciones", dto.Minijuego!.Value.GetProperty("tipo").GetString());
        Assert.Equal("Machaca", dto.Mensaje);
    }

    [Fact]
    public async Task AddSalida_ExitoYFallo_SeGuardanConSuTipo_YNoSePuedenRepetir()
    {
        await using var context = TestDb.CreateContext();
        var mundo = await NuevoMundoAsync(context);
        var id = await CrearJuega(context, mundo, Contenido());
        var handler = new AddRecursoDecisionOpcion.Handler(context);

        await handler.HandleAsync(Salida(id, "exito", efectos: """[{"objeto":"llave","op":"dar"}]"""), mundo.DuenoId, CancellationToken.None);
        await handler.HandleAsync(Salida(id, "fallo"), mundo.DuenoId, CancellationToken.None);

        var tipos = await context.RecursoDecisionOpciones.Select(o => o.Tipo).OrderBy(t => t).ToListAsync();
        Assert.Equal(new[] { "exito", "fallo" }, tipos);

        var repetida = await Assert.ThrowsAsync<ExceptionHandler>(() => handler.HandleAsync(Salida(id, "exito"), mundo.DuenoId, CancellationToken.None));
        Assert.Equal(HttpStatusCode.BadRequest, repetida.Code);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("zona")]
    [InlineData("opcion")]
    [InlineData("victoria")]
    public async Task AddSalida_ConTipoQueNoEsExitoNiFallo_Falla400(string? tipo)
    {
        await using var context = TestDb.CreateContext();
        var mundo = await NuevoMundoAsync(context);
        var id = await CrearJuega(context, mundo, Contenido());

        var ex = await Assert.ThrowsAsync<ExceptionHandler>(() =>
            new AddRecursoDecisionOpcion.Handler(context).HandleAsync(Salida(id, tipo), mundo.DuenoId, CancellationToken.None));

        Assert.Equal(HttpStatusCode.BadRequest, ex.Code);
    }

    [Fact]
    public async Task AddSalida_ConCondicion_Falla400()
    {
        await using var context = TestDb.CreateContext();
        var mundo = await NuevoMundoAsync(context);
        var id = await CrearJuega(context, mundo, Contenido());

        var ex = await Assert.ThrowsAsync<ExceptionHandler>(() => new AddRecursoDecisionOpcion.Handler(context).HandleAsync(
            Salida(id, "exito", condicion: """{"var":"puntos","op":">","valor":1}"""), mundo.DuenoId, CancellationToken.None));

        Assert.Equal(HttpStatusCode.BadRequest, ex.Code);
    }

    [Fact]
    public async Task Tipo_EnOtrosRecursos_SeIgnora()
    {
        await using var context = TestDb.CreateContext();
        var mundo = await NuevoMundoAsync(context);
        var decision = new Recurso { EscenaId = mundo.Escena.EscenaId, TipoRecurso = RecursoTipos.Decision };
        await context.Recursos.AddAsync(decision);
        await context.SaveChangesAsync();

        await new AddRecursoDecisionOpcion.Handler(context).HandleAsync(
            new AddRecursoDecisionOpcion.AddRecursoDecisionOpcionRequest("A", null, decision.RecursoId, null, null, null, null, null, "exito"),
            mundo.DuenoId, CancellationToken.None);

        Assert.Equal(RecursoSalidaTipos.Opcion, (await context.RecursoDecisionOpciones.SingleAsync()).Tipo);
    }

    // ---- Validación de la versión

    [Fact]
    public async Task ValidarVersion_JuegaSinSalidas_EsError_Completo_NoLoEs()
    {
        await using var context = TestDb.CreateContext();
        var mundo = await NuevoMundoAsync(context);
        var id = await CrearJuega(context, mundo, Contenido());

        var incompleta = await VersionValidator.ValidarAsync(context, mundo.Version.NovelaVersionId, CancellationToken.None);
        Assert.Contains(incompleta.Errores, e => e.Codigo == "juega_sin_exito");
        Assert.Contains(incompleta.Errores, e => e.Codigo == "juega_sin_fallo");

        var handler = new AddRecursoDecisionOpcion.Handler(context);
        await handler.HandleAsync(Salida(id, "exito", efectos: """[{"objeto":"llave","op":"dar"}]"""), mundo.DuenoId, CancellationToken.None);
        await handler.HandleAsync(Salida(id, "fallo", destino: id), mundo.DuenoId, CancellationToken.None);   // reintentar: vuelve al mismo nodo

        var completa = await VersionValidator.ValidarAsync(context, mundo.Version.NovelaVersionId, CancellationToken.None);
        Assert.True(completa.Valida);
        Assert.DoesNotContain(completa.Advertencias, a => a.Codigo == "salida_sin_destino");
    }

    [Fact]
    public async Task ValidarVersion_SalidaSinDestinoNiEfectos_EsAdvertencia_YSalidaRepetidaEsError()
    {
        await using var context = TestDb.CreateContext();
        var mundo = await NuevoMundoAsync(context);
        var id = await CrearJuega(context, mundo, Contenido());
        // Datos que el handler nunca dejaría pasar: dos salidas de éxito y ninguna de fallo.
        await context.RecursoDecisionOpciones.AddRangeAsync(
            new RecursoDecisionOpcion { RecursoDecisionId = id, Tipo = RecursoSalidaTipos.Exito },
            new RecursoDecisionOpcion { RecursoDecisionId = id, Tipo = RecursoSalidaTipos.Exito });
        await context.SaveChangesAsync();

        var resultado = await VersionValidator.ValidarAsync(context, mundo.Version.NovelaVersionId, CancellationToken.None);

        Assert.Contains(resultado.Errores, e => e.Codigo == "juega_sin_exito");   // "exactamente una"
        Assert.Contains(resultado.Errores, e => e.Codigo == "juega_sin_fallo");

        // Salida única sin destino ni efectos → solo aviso
        await using var otro = TestDb.CreateContext();
        var m2 = await NuevoMundoAsync(otro);
        var id2 = await CrearJuega(otro, m2, Contenido());
        var h = new AddRecursoDecisionOpcion.Handler(otro);
        await h.HandleAsync(Salida(id2, "exito"), m2.DuenoId, CancellationToken.None);
        await h.HandleAsync(Salida(id2, "fallo", destino: id2), m2.DuenoId, CancellationToken.None);
        var avisos = await VersionValidator.ValidarAsync(otro, m2.Version.NovelaVersionId, CancellationToken.None);
        Assert.True(avisos.Valida);
        Assert.Contains(avisos.Advertencias, a => a.Codigo == "salida_sin_destino");
    }

    [Fact]
    public async Task ValidarVersion_MinijuegoOVariableRotos_SonError()
    {
        await using var context = TestDb.CreateContext();
        var mundo = await NuevoMundoAsync(context);
        await context.Recursos.AddAsync(new Recurso
        {
            EscenaId = mundo.Escena.EscenaId, TipoRecurso = RecursoTipos.Juega, PrimerRecurso = true,
            Contenido = """{"Mensaje":"x","VariableResultado":"nombre","Minijuego":{"tipo":"reflejo","objetivos":500}}"""
        });
        await context.Recursos.AddAsync(new Recurso { EscenaId = mundo.Escena.EscenaId, TipoRecurso = RecursoTipos.Juega, Contenido = "{}" });
        await context.SaveChangesAsync();

        var resultado = await VersionValidator.ValidarAsync(context, mundo.Version.NovelaVersionId, CancellationToken.None);

        Assert.True(resultado.Errores.Count(e => e.Codigo == "juega_invalido") >= 3);
    }

    [Fact]
    public async Task Mapeo_JuegaConContenidoVacio_NoLanza()
    {
        await using var context = TestDb.CreateContext();
        var mundo = await NuevoMundoAsync(context);
        var recurso = new Recurso { EscenaId = mundo.Escena.EscenaId, TipoRecurso = RecursoTipos.Juega, Contenido = "{}" };
        await context.Recursos.AddAsync(recurso);
        await context.SaveChangesAsync();

        var dto = (RecursoJuegaDto)RecursoMapping.ToDto(recurso);

        Assert.Null(dto.Minijuego);
        JsonSerializer.Serialize(dto);   // serializar la versión entera no debe fallar
    }

    // ---- Borrador

    [Fact]
    public async Task CreateDraft_ClonaElJuegaConSusSalidas()
    {
        await using var context = TestDb.CreateContext();
        var mundo = await NuevoMundoAsync(context);
        var destino = new Recurso { EscenaId = mundo.Escena.EscenaId, TipoRecurso = RecursoTipos.Conversacion, Contenido = "{}" };
        await context.Recursos.AddAsync(destino);
        await context.SaveChangesAsync();
        var id = await CrearJuega(context, mundo, Contenido(variable: "puntos"));
        var handler = new AddRecursoDecisionOpcion.Handler(context);
        await handler.HandleAsync(Salida(id, "exito", destino.RecursoId), mundo.DuenoId, CancellationToken.None);
        await handler.HandleAsync(Salida(id, "fallo", id), mundo.DuenoId, CancellationToken.None);
        mundo.Version.EsBorrador = false;
        mundo.Version.Disponible = true;
        await context.SaveChangesAsync();

        var draftId = await new CreateDraftFromVersion.Handler(context).HandleAsync(mundo.Novela.NovelaId, mundo.DuenoId, CancellationToken.None);

        var nuevo = await context.Recursos.SingleAsync(r => r.Escena.NovelaVersionId == draftId && r.TipoRecurso == RecursoTipos.Juega);
        var nuevoDestino = await context.Recursos.SingleAsync(r => r.Escena.NovelaVersionId == draftId && r.TipoRecurso == RecursoTipos.Conversacion);
        var salidas = await context.RecursoDecisionOpciones.Where(o => o.RecursoDecisionId == nuevo.RecursoId).ToListAsync();
        Assert.Equal(nuevoDestino.RecursoId, salidas.Single(s => s.Tipo == "exito").SiguienteRecursoId);
        Assert.Equal(nuevo.RecursoId, salidas.Single(s => s.Tipo == "fallo").SiguienteRecursoId);   // el reintento apunta al NUEVO nodo
        Assert.Contains("puntos", nuevo.Contenido);
    }
}
