using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Application.Motor;

namespace UnitTests;

public class MotorValidadorTests
{
    private static readonly IReadOnlyDictionary<string, VariableDef> Vars = new List<VariableDef>
    {
        new("salud", "Salud", VariableTipos.Numero, null, 0, 100, new List<string>(), VariableHud.Barra),
        new("tiene_pareja", "Pareja", VariableTipos.Booleano, null, null, null, new List<string>(), VariableHud.Oculto),
        new("tiempo", "Tiempo", VariableTipos.Texto, null, null, null, new List<string> { "mañana", "tarde", "noche" }, VariableHud.Oculto),
    }.ToDictionary(v => v.Clave);

    private static List<string> Condicion(string json)
    {
        var errores = new List<string>();
        MotorValidador.ValidarCondicion(JsonDocument.Parse(json).RootElement, Vars, "c", errores);
        return errores;
    }

    private static List<string> Efectos(string json)
    {
        var errores = new List<string>();
        MotorValidador.ValidarEfectos(JsonDocument.Parse(json).RootElement, Vars, "e", errores);
        return errores;
    }

    [Fact]
    public void Condicion_ComparacionValida_NoTieneErrores()
    {
        Assert.Empty(Condicion("""{"var":"salud","op":">=","valor":50}"""));
        Assert.Empty(Condicion("""{"var":"tiempo","op":"==","valor":"tarde"}"""));
        Assert.Empty(Condicion("""{"var":"tiene_pareja","op":"==","valor":true}"""));
    }

    [Fact]
    public void Condicion_CombinadoresAnidados_SeValidanRecursivamente()
    {
        Assert.Empty(Condicion("""{"y":[{"var":"salud","op":">","valor":10},{"no":{"o":[{"var":"tiene_pareja","op":"==","valor":true}]}}]}"""));

        var errores = Condicion("""{"y":[{"var":"salud","op":">","valor":10},{"no":{"var":"fantasma","op":"==","valor":1}}]}""");
        Assert.Single(errores);
        Assert.Contains("fantasma", errores[0]);
    }

    [Theory]
    [InlineData("""{"var":"salud","op":">=","valor":"mucho"}""")]        // texto contra número
    [InlineData("""{"var":"salud","op":"~","valor":5}""")]               // operador desconocido
    [InlineData("""{"var":"tiene_pareja","op":">","valor":true}""")]      // orden sobre booleano
    [InlineData("""{"var":"tiempo","op":"==","valor":"madrugada"}""")]    // fuera de los valores permitidos
    [InlineData("""{"var":"salud","op":">="}""")]                        // falta valor
    [InlineData("""{"y":[]}""")]                                          // combinador vacío
    [InlineData("""{"y":[{"var":"salud","op":">","valor":1}],"o":[]}""")] // dos formas a la vez
    [InlineData("""[1,2]""")]                                            // no es objeto
    [InlineData("""{"eval":"alert(1)"}""")]                              // nada de código evaluable
    public void Condicion_Invalida_ReportaError(string json)
    {
        Assert.NotEmpty(Condicion(json));
    }

    [Fact]
    public void Condicion_AnidadadaDemasiadoProfundo_SeRechaza()
    {
        var json = """{"var":"salud","op":">","valor":1}""";
        for (var i = 0; i < MotorValidador.ProfundidadMaxima + 2; i++)
        {
            json = $$"""{"no":{{json}}}""";
        }

        Assert.NotEmpty(Condicion(json));
    }

    [Fact]
    public void Efectos_Validos_NoTienenErrores()
    {
        Assert.Empty(Efectos("""[{"var":"salud","op":"sumar","valor":1}]"""));
        Assert.Empty(Efectos("""[{"var":"tiene_pareja","op":"alternar"},{"var":"tiempo","op":"fijar","valor":"noche"},{"var":"salud","op":"restar","valor":5}]"""));
        Assert.Empty(Efectos("[]"));
    }

    [Theory]
    [InlineData("""[{"var":"salud","op":"alternar"}]""")]                      // alternar solo booleanos
    [InlineData("""[{"var":"tiene_pareja","op":"alternar","valor":true}]""")]   // alternar no lleva valor
    [InlineData("""[{"var":"tiempo","op":"sumar","valor":1}]""")]               // aritmética sobre texto
    [InlineData("""[{"var":"salud","op":"fijar","valor":true}]""")]             // tipo equivocado
    [InlineData("""[{"var":"salud","op":"sumar"}]""")]                          // falta valor
    [InlineData("""[{"var":"salud","op":"sumar","valor":1,"extra":2}]""")]       // propiedad desconocida
    [InlineData("""[{"var":"nadie","op":"fijar","valor":1}]""")]                // variable inexistente
    [InlineData("""{"var":"salud"}""")]                                        // no es lista
    public void Efectos_Invalidos_ReportanError(string json)
    {
        Assert.NotEmpty(Efectos(json));
    }

    [Fact]
    public void Efectos_MasDelMaximo_SeRechaza()
    {
        var uno = """{"var":"salud","op":"sumar","valor":1}""";
        var json = "[" + string.Join(",", Enumerable.Repeat(uno, MotorValidador.MaxEfectos + 1)) + "]";

        Assert.NotEmpty(Efectos(json));
    }
}
