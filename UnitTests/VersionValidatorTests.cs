using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Application.Motor;
using Domain.Models;
using WebAPI.Features.Recursos;

namespace UnitTests;

public class VersionValidatorTests
{
    private const string DefinicionesConSalud =
        """{"variables":[{"clave":"salud","etiqueta":"Salud","tipo":"numero","inicial":50,"min":0,"max":100,"valores":[],"hud":"barra"}]}""";

    private static (NovelaVersion Version, Escena Escena) NuevaVersion(string definiciones = DefinicionesConSalud)
    {
        var version = new NovelaVersion { NovelaVersionId = Guid.NewGuid(), Definiciones = definiciones, Escenas = new List<Escena>() };
        var escena = new Escena { EscenaId = Guid.NewGuid(), NovelaVersionId = version.NovelaVersionId, PrimerEscena = true, Recursos = new List<Recurso>() };
        version.Escenas.Add(escena);
        return (version, escena);
    }

    private static Recurso Nodo(Escena escena, string tipo, string contenido = "{}", Guid? siguiente = null, bool primero = false)
    {
        var recurso = new Recurso
        {
            RecursoId = Guid.NewGuid(),
            EscenaId = escena.EscenaId,
            TipoRecurso = tipo,
            Contenido = contenido,
            SiguienteRecursoId = siguiente,
            PrimerRecurso = primero,
            Opciones = new List<RecursoDecisionOpcion>()
        };
        escena.Recursos.Add(recurso);
        return recurso;
    }

    private static void Rama(Recurso evalua, string? condicion, Guid? destino, string tipo = RecursoSalidaTipos.Rama) =>
        evalua.Opciones.Add(new RecursoDecisionOpcion
        {
            RecursoDecisionOpcionId = Guid.NewGuid(),
            RecursoDecisionId = evalua.RecursoId,
            SiguienteRecursoId = destino,
            Condicion = condicion,
            Tipo = tipo
        });

    private static string Conv(string mensaje) => JsonSerializer.Serialize(new ConversacionContenido(mensaje, ""));

    [Fact]
    public void Validar_VersionCorrecta_EsValidaSinAdvertencias()
    {
        var (version, escena) = NuevaVersion();
        var fin = Nodo(escena, RecursoTipos.Conversacion, Conv("fin"));
        var otro = Nodo(escena, RecursoTipos.Conversacion, Conv("otro"));
        var asigna = Nodo(escena, RecursoTipos.Asigna, """{"Efectos":[{"var":"salud","op":"sumar","valor":10}]}""", siguiente: null);
        var evalua = Nodo(escena, RecursoTipos.Evalua, siguiente: otro.RecursoId);
        Rama(evalua, """{"var":"salud","op":">=","valor":50}""", asigna.RecursoId);
        asigna.SiguienteRecursoId = fin.RecursoId;
        var inicio = Nodo(escena, RecursoTipos.Conversacion, Conv("hola"), siguiente: evalua.RecursoId, primero: true);

        var resultado = VersionValidator.Validar(version);

        Assert.True(resultado.Valida);
        Assert.Empty(resultado.Errores);
        Assert.Empty(resultado.Advertencias);
        Assert.NotEqual(inicio.RecursoId, fin.RecursoId);
    }

    [Fact]
    public void Validar_EvaluaSinRamasNiSino_ReportaAmbosErrores()
    {
        var (version, escena) = NuevaVersion();
        Nodo(escena, RecursoTipos.Evalua, primero: true);

        var resultado = VersionValidator.Validar(version);

        Assert.False(resultado.Valida);
        Assert.Contains(resultado.Errores, e => e.Codigo == "evalua_sin_ramas");
        Assert.Contains(resultado.Errores, e => e.Codigo == "evalua_sin_sino");
    }

    [Fact]
    public void Validar_RamaSinCondicionOConVariableInexistente_ReportaError()
    {
        var (version, escena) = NuevaVersion();
        var destino = Nodo(escena, RecursoTipos.Conversacion, Conv("x"));
        var evalua = Nodo(escena, RecursoTipos.Evalua, siguiente: destino.RecursoId, primero: true);
        Rama(evalua, null, destino.RecursoId);
        Rama(evalua, """{"var":"fantasma","op":"==","valor":1}""", destino.RecursoId);

        var resultado = VersionValidator.Validar(version);

        Assert.Contains(resultado.Errores, e => e.Codigo == "rama_sin_condicion");
        Assert.Contains(resultado.Errores, e => e.Codigo == "condicion_o_efecto_invalido");
    }

    [Fact]
    public void Validar_AsignaConEfectoInvalido_ReportaError()
    {
        var (version, escena) = NuevaVersion();
        Nodo(escena, RecursoTipos.Asigna, """{"Efectos":[{"var":"salud","op":"alternar"}]}""", primero: true);
        Nodo(escena, RecursoTipos.Asigna, "{}");

        var resultado = VersionValidator.Validar(version);

        Assert.Equal(2, resultado.Errores.Count(e => e.Codigo == "asigna_invalido"));
    }

    [Fact]
    public void Validar_OpcionDeSeleccionaConCondicionInvalida_ReportaError()
    {
        var (version, escena) = NuevaVersion();
        var decision = Nodo(escena, RecursoTipos.Decision, primero: true);
        Rama(decision, """{"var":"salud","op":">=","valor":"alto"}""", null, RecursoSalidaTipos.Opcion);

        var resultado = VersionValidator.Validar(version);

        Assert.Contains(resultado.Errores, e => e.Codigo == "condicion_o_efecto_invalido");
    }

    [Fact]
    public void Validar_EnlaceAOtraVersion_ReportaError()
    {
        var (version, escena) = NuevaVersion();
        Nodo(escena, RecursoTipos.Conversacion, Conv("x"), siguiente: Guid.NewGuid(), primero: true);

        var resultado = VersionValidator.Validar(version);

        Assert.Contains(resultado.Errores, e => e.Codigo == "enlace_fuera_de_version");
    }

    [Fact]
    public void Validar_DefinicionesInvalidas_BloqueaPublicacion()
    {
        var (version, escena) = NuevaVersion("""{"variables":[{"clave":"Mala Clave","tipo":"numero"}]}""");
        Nodo(escena, RecursoTipos.Conversacion, Conv("x"), primero: true);

        Assert.Contains(VersionValidator.Validar(version).Errores, e => e.Codigo == "definiciones_invalidas");
    }

    [Fact]
    public void Validar_EntradaSinVariableDefinida_EsAdvertencia_ConVariableDeTexto_No()
    {
        var (version, escena) = NuevaVersion("""{"variables":[{"clave":"nombre","tipo":"texto","valores":[],"hud":"oculto"}]}""");
        var sinVariable = Nodo(escena, RecursoTipos.Entrada,
            JsonSerializer.Serialize(new EntradaContenido("¿Cómo te llamas?", "otra", "", "")), primero: true);
        var conVariable = Nodo(escena, RecursoTipos.Entrada,
            JsonSerializer.Serialize(new EntradaContenido("¿Cómo te llamas?", "nombre", "", "")), siguiente: null);
        sinVariable.SiguienteRecursoId = conVariable.RecursoId;

        var resultado = VersionValidator.Validar(version);

        Assert.True(resultado.Valida);
        var avisos = resultado.Advertencias.Where(a => a.Codigo == "entrada_sin_variable").ToList();
        Assert.Single(avisos);
        Assert.Equal(sinVariable.RecursoId, avisos[0].RecursoId);
    }

    [Fact]
    public void Validar_NodoInalcanzable_EsAdvertenciaNoError()
    {
        var (version, escena) = NuevaVersion();
        Nodo(escena, RecursoTipos.Conversacion, Conv("inicio"), primero: true);
        var huerfano = Nodo(escena, RecursoTipos.Conversacion, Conv("huérfano"));

        var resultado = VersionValidator.Validar(version);

        Assert.True(resultado.Valida);
        Assert.Contains(resultado.Advertencias, a => a.Codigo == "inalcanzable" && a.RecursoId == huerfano.RecursoId);
    }

    [Fact]
    public void Validar_CicloSoloDeNodosAutomaticos_EsAdvertencia()
    {
        var (version, escena) = NuevaVersion();
        var a = Nodo(escena, RecursoTipos.Asigna, """{"Efectos":[{"var":"salud","op":"sumar","valor":1}]}""", primero: true);
        var b = Nodo(escena, RecursoTipos.Evalua, siguiente: a.RecursoId);
        Rama(b, """{"var":"salud","op":"<","valor":10}""", a.RecursoId);
        a.SiguienteRecursoId = b.RecursoId;

        var resultado = VersionValidator.Validar(version);

        Assert.True(resultado.Valida);
        Assert.Contains(resultado.Advertencias, w => w.Codigo == "ciclo_automatico");
    }

    [Fact]
    public void Validar_CicloQueIncluyeUnDialogo_NoAdvierte()
    {
        var (version, escena) = NuevaVersion();
        var habla = Nodo(escena, RecursoTipos.Conversacion, Conv("otra vuelta"), primero: true);
        var asigna = Nodo(escena, RecursoTipos.Asigna, """{"Efectos":[{"var":"salud","op":"sumar","valor":1}]}""", siguiente: habla.RecursoId);
        habla.SiguienteRecursoId = asigna.RecursoId;

        Assert.DoesNotContain(VersionValidator.Validar(version).Advertencias, w => w.Codigo == "ciclo_automatico");
    }

    [Fact]
    public async Task ValidarAsync_CargaOpcionesDeLaBaseDeDatos()
    {
        await using var context = TestDb.CreateContext();

        var novela = new Novela { Titulo = "T" };
        await context.Novelas.AddAsync(novela);
        var version = new NovelaVersion { NovelaId = novela.NovelaId, NumeroVersion = "1.0.0", EsBorrador = true, Definiciones = DefinicionesConSalud };
        await context.NovelaVersiones.AddAsync(version);
        var escena = new Escena { NovelaVersionId = version.NovelaVersionId, PrimerEscena = true };
        await context.Escenas.AddAsync(escena);
        var evalua = new Recurso { EscenaId = escena.EscenaId, TipoRecurso = RecursoTipos.Evalua, PrimerRecurso = true };
        await context.Recursos.AddAsync(evalua);
        await context.SaveChangesAsync();

        var resultado = await VersionValidator.ValidarAsync(context, version.NovelaVersionId, CancellationToken.None);

        Assert.Contains(resultado.Errores, e => e.Codigo == "evalua_sin_ramas");
    }

    [Fact]
    public async Task ValidarAsync_VersionInexistente_DevuelveError()
    {
        await using var context = TestDb.CreateContext();

        var resultado = await VersionValidator.ValidarAsync(context, Guid.NewGuid(), CancellationToken.None);

        Assert.False(resultado.Valida);
        Assert.Equal("version_no_encontrada", resultado.Errores.Single().Codigo);
    }
}
