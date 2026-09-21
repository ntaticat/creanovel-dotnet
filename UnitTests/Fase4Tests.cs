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
using Persistence;
using WebAPI.Features.Recursos;

namespace UnitTests;

public class Fase4Tests
{
    private const string Definiciones = """
        {"variables":[{"clave":"tiempo","tipo":"texto","valores":["mañana","tarde","noche"],"hud":"oculto"},
                      {"clave":"salud","tipo":"numero","valores":[],"hud":"oculto"},
                      {"clave":"solo","tipo":"texto","valores":["uno"],"hud":"oculto"},
                      {"clave":"libre","tipo":"texto","valores":[],"hud":"oculto"}],
         "objetos":[{"id":"llave","nombre":"Llave","descripcion":"","apilable":false,"inicial":0}],
         "ubicaciones":[{"id":"patio","nombre":"Patio"}],
         "logros":[{"id":"valiente","nombre":"Valiente","descripcion":""}],
         "finales":[{"id":"bueno","nombre":"Final bueno","descripcion":""},{"id":"malo","nombre":"Final malo","descripcion":""}]}
        """;

    private static JsonElement Json(string json) => JsonDocument.Parse(json).RootElement.Clone();
    private static ReglasContexto Ctx() => ReglasContexto.Desde(MotorJson.LeerDefiniciones(Definiciones));

    // ---- Catálogo: logros y finales, palabras reservadas

    private static (DefinicionesJuego, List<string>) Normalizar(DefinicionesJuego defs) => MotorJson.NormalizarYValidar(defs);

    [Fact]
    public void Definiciones_LogrosYFinales_SeNormalizan()
    {
        var (defs, errores) = Normalizar(new DefinicionesJuego(new(), null, null, null,
            new() { new("valiente", null!, "  Sobreviviste  ") }, new() { new("bueno", "Final bueno", null!) }));

        Assert.Empty(errores);
        Assert.Equal("valiente", defs.Logros![0].Nombre);        // sin nombre → usa el id
        Assert.Equal("Sobreviviste", defs.Logros[0].Descripcion);
        Assert.Equal("", defs.Finales![0].Descripcion);
    }

    [Fact]
    public void Definiciones_LogrosYFinalesInvalidos_ReportanError()
    {
        var (_, errores) = Normalizar(new DefinicionesJuego(new(), null, null, null,
            new() { new("Mal Id", "x", ""), new("a", "x", ""), new("a", "x", "") },
            new() { new("final", "x", "") }));

        Assert.Equal(3, errores.Count);   // id inválido, repetido, palabra reservada
    }

    [Theory]
    [InlineData("y")] [InlineData("o")] [InlineData("no")] [InlineData("en")] [InlineData("tengo")] [InlineData("objeto")]
    [InlineData("verdadero")] [InlineData("falso")] [InlineData("alternar")] [InlineData("avanzar")] [InlineData("dar")]
    [InlineData("quitar")] [InlineData("ir")] [InlineData("logro")] [InlineData("final")] [InlineData("si")] [InlineData("ubicacion")]
    public void PalabraReservada_NoSePuedeUsarComoId(string palabra)
    {
        var variable = new VariableDef(palabra, null!, VariableTipos.Numero, null, null, null, null!, null!);

        Assert.Single(Normalizar(new DefinicionesJuego(new() { variable })).Item2);
        Assert.Single(Normalizar(new DefinicionesJuego(new(), new() { new(palabra, "x", "", null!, false, null, 0) })).Item2);
        Assert.Single(Normalizar(new DefinicionesJuego(new(), null, new() { new(palabra, "x", null) })).Item2);
    }

    [Fact]
    public void PalabraA_NoEstaReservada_ParaPoderNombrarUnaUbicacionAsi()
    {
        var (_, errores) = Normalizar(new DefinicionesJuego(new(), null, new() { new("a", "A", null) }));

        Assert.Empty(errores);
    }

    [Fact]
    public void LeerDefiniciones_VersionesSinLogrosNiFinales_NoRompen()
    {
        var defs = MotorJson.LeerDefiniciones("""{"variables":[],"objetos":[]}""");

        Assert.NotNull(defs.Logros);
        Assert.NotNull(defs.Finales);
    }

    // ---- avanzar, logro y final en condiciones y efectos

    private static List<string> Efectos(string json)
    {
        var errores = new List<string>();
        MotorValidador.ValidarEfectos(Json(json), Ctx(), "e", errores);
        return errores;
    }

    private static List<string> Condicion(string json)
    {
        var errores = new List<string>();
        MotorValidador.ValidarCondicion(Json(json), Ctx(), "c", errores);
        return errores;
    }

    [Fact]
    public void Efectos_AvanzarYLogro_Validos() =>
        Assert.Empty(Efectos("""[{"var":"tiempo","op":"avanzar"},{"logro":"valiente"}]"""));

    [Theory]
    [InlineData("""[{"var":"salud","op":"avanzar"}]""")]                        // numérica
    [InlineData("""[{"var":"solo","op":"avanzar"}]""")]                         // un solo valor: no hay a dónde avanzar
    [InlineData("""[{"var":"libre","op":"avanzar"}]""")]                        // texto libre, sin lista de valores
    [InlineData("""[{"var":"tiempo","op":"avanzar","valor":"tarde"}]""")]        // no lleva valor
    [InlineData("""[{"logro":"fantasma"}]""")]
    [InlineData("""[{"logro":"valiente","op":"dar"}]""")]
    [InlineData("""[{"logro":7}]""")]
    public void Efectos_AvanzarOLogroInvalidos_ReportanError(string json) => Assert.NotEmpty(Efectos(json));

    [Fact]
    public void Condicion_LogroYFinal_Validos()
    {
        Assert.Empty(Condicion("""{"logro":"valiente"}"""));
        Assert.Empty(Condicion("""{"y":[{"final":"bueno"},{"no":{"final":"malo"}}]}"""));
    }

    [Theory]
    [InlineData("""{"logro":"fantasma"}""")]
    [InlineData("""{"final":"fantasma"}""")]
    [InlineData("""{"final":"valiente"}""")]           // es un logro, no un final
    [InlineData("""{"logro":"bueno"}""")]              // es un final, no un logro
    [InlineData("""{"final":"bueno","extra":1}""")]
    public void Condicion_LogroOFinalInvalidos_ReportanError(string json) => Assert.NotEmpty(Condicion(json));

    // ---- Termina

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

    private static Task<Guid> CrearTermina(CreanovelDbContext context, Mundo mundo, string contenido, Guid? siguiente = null) =>
        new CreateRecurso.Handler(context).HandleAsync(new CreateRecurso.CreateRecursoRequest(
            mundo.Escena.EscenaId, RecursoTipos.Termina, true, true, siguiente, null, null, Json(contenido)), mundo.DuenoId, CancellationToken.None);

    [Fact]
    public async Task CreateRecurso_Termina_GuardaElFinalYElMapeoLoDevuelve()
    {
        await using var context = TestDb.CreateContext();
        var mundo = await NuevoMundoAsync(context);

        var id = await CrearTermina(context, mundo, """{"final":"bueno","mensaje":"Fin, {salud} de salud."}""");

        var recurso = (await context.Recursos.FindAsync(id))!;
        var dto = (RecursoTerminaDto)RecursoMapping.ToDto(recurso);
        Assert.Equal("bueno", dto.Final);
        Assert.Equal("Fin, {salud} de salud.", dto.Mensaje);
        Assert.Null(recurso.SiguienteRecursoId);
    }

    [Fact]
    public async Task CreateRecurso_Termina_IgnoraElSiguiente()
    {
        await using var context = TestDb.CreateContext();
        var mundo = await NuevoMundoAsync(context);
        var otro = new Recurso { EscenaId = mundo.Escena.EscenaId, TipoRecurso = RecursoTipos.Conversacion };
        await context.Recursos.AddAsync(otro);
        await context.SaveChangesAsync();

        var id = await CrearTermina(context, mundo, """{"final":"malo"}""", otro.RecursoId);

        Assert.Null((await context.Recursos.FindAsync(id))!.SiguienteRecursoId);
    }

    [Theory]
    [InlineData("""{"mensaje":"x"}""")]                    // sin final
    [InlineData("""{"final":"fantasma"}""")]                // no está en el catálogo
    [InlineData("""{"final":"valiente"}""")]                // es un logro
    public async Task CreateRecurso_Termina_Invalido_Falla400(string contenido)
    {
        await using var context = TestDb.CreateContext();
        var mundo = await NuevoMundoAsync(context);

        var ex = await Assert.ThrowsAsync<ExceptionHandler>(() => CrearTermina(context, mundo, contenido));

        Assert.Equal(HttpStatusCode.BadRequest, ex.Code);
        Assert.Empty(context.Recursos);
    }

    [Fact]
    public async Task CreateRecurso_Termina_ConMensajeLargo_Falla400()
    {
        await using var context = TestDb.CreateContext();
        var mundo = await NuevoMundoAsync(context);

        var ex = await Assert.ThrowsAsync<ExceptionHandler>(() =>
            CrearTermina(context, mundo, $$"""{"final":"bueno","mensaje":"{{new string('a', 501)}}"}"""));

        Assert.Equal(HttpStatusCode.BadRequest, ex.Code);
    }

    // ---- Validación de la versión

    private static NovelaVersion Version(params (string tipo, string contenido)[] nodos)
    {
        var version = new NovelaVersion { NovelaVersionId = Guid.NewGuid(), Definiciones = Definiciones, Escenas = new List<Escena>() };
        var escena = new Escena { EscenaId = Guid.NewGuid(), PrimerEscena = true, Recursos = new List<Recurso>() };
        version.Escenas.Add(escena);
        var primero = true;
        foreach (var (tipo, contenido) in nodos)
        {
            escena.Recursos.Add(new Recurso { RecursoId = Guid.NewGuid(), EscenaId = escena.EscenaId, TipoRecurso = tipo, Contenido = contenido, PrimerRecurso = primero, Opciones = new List<RecursoDecisionOpcion>() });
            primero = false;
        }
        return version;
    }

    [Fact]
    public void Validar_TerminaConFinalDefinido_Ok_YSinDefinir_EsError()
    {
        var bueno = VersionValidator.Validar(Version((RecursoTipos.Termina, """{"Final":"bueno","Mensaje":""}""")));
        Assert.DoesNotContain(bueno.Errores, e => e.Codigo == "termina_invalido");

        var malo = VersionValidator.Validar(Version((RecursoTipos.Termina, """{"Final":"fantasma","Mensaje":""}""")));
        Assert.Contains(malo.Errores, e => e.Codigo == "termina_invalido");

        var vacio = VersionValidator.Validar(Version((RecursoTipos.Termina, "{}")));
        Assert.Contains(vacio.Errores, e => e.Codigo == "termina_invalido");
    }

    [Fact]
    public void Validar_FinalesQueNingunNodoUsa_SonAdvertencia()
    {
        var resultado = VersionValidator.Validar(Version((RecursoTipos.Termina, """{"Final":"bueno","Mensaje":""}""")));

        var avisos = resultado.Advertencias.Where(a => a.Codigo == "final_sin_nodo").ToList();
        Assert.Single(avisos);
        Assert.Contains("Final malo", avisos[0].Mensaje);
        Assert.True(resultado.Valida || resultado.Errores.All(e => e.Codigo != "final_sin_nodo"));
    }

    private static List<ValidacionIssue> AvisosDeTexto(string mensaje) =>
        VersionValidator.Validar(Version((RecursoTipos.Conversacion, JsonSerializer.Serialize(new ConversacionContenido(mensaje, "")))))
            .Advertencias.Where(a => a.Codigo == "texto_variable_desconocida").ToList();

    [Fact]
    public void Textos_CitasConocidas_NoAvisan()
    {
        Assert.Empty(AvisosDeTexto("Tienes {salud} de salud, {objeto:llave} llaves, estás en {ubicacion} y es {tiempo}."));
    }

    [Fact]
    public void Textos_CitasDesconocidas_AvisanUnaVezCadaUna()
    {
        var avisos = AvisosDeTexto("Hola {nombre}, {nombre}, {objeto:espada} y {salud}.");

        Assert.Equal(2, avisos.Count);
        Assert.Contains(avisos, a => a.Mensaje.Contains("{nombre}"));
        Assert.Contains(avisos, a => a.Mensaje.Contains("{objeto:espada}"));
    }

    [Fact]
    public void Textos_LlavesEscapadasOTextoNormal_NoAvisan()
    {
        Assert.Empty(AvisosDeTexto("Escribe {{nombre}} tal cual"));
        Assert.Empty(AvisosDeTexto("Un texto sin llaves"));
        Assert.Empty(AvisosDeTexto("{si salud < 3: mal | bien}"));   // los condicionales los valida el editor
    }

    [Fact]
    public void Textos_SeRevisanEnOpcionesYEnLosDemasTipos()
    {
        var version = Version((RecursoTipos.Decision, JsonSerializer.Serialize(new DecisionContenido("¿Qué, {a1}?", ""))));
        version.Escenas.First().Recursos.First().Opciones.Add(new RecursoDecisionOpcion { OpcionMensaje = "Ir con {b1}", Tipo = RecursoSalidaTipos.Opcion });
        var explora = Version((RecursoTipos.Explora, JsonSerializer.Serialize(new ExploraContenido("Ves a {c1}"))));

        var a = VersionValidator.Validar(version).Advertencias.Count(x => x.Codigo == "texto_variable_desconocida");
        var b = VersionValidator.Validar(explora).Advertencias.Count(x => x.Codigo == "texto_variable_desconocida");

        Assert.Equal(2, a);   // el mensaje y la opción
        Assert.Equal(1, b);
    }
}
