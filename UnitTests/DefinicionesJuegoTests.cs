using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Application.Motor;

namespace UnitTests;

public class DefinicionesJuegoTests
{
    private static VariableDef Var(string clave, string tipo, JsonElement? inicial = null, double? min = null, double? max = null, string? hud = null, params string[] valores) =>
        new(clave, null!, tipo, inicial, min, max, valores.ToList(), hud!);

    private static (DefinicionesJuego, List<string>) Normalizar(params VariableDef[] variables) =>
        MotorJson.NormalizarYValidar(new DefinicionesJuego(variables.ToList()));

    [Fact]
    public void Normalizar_RellenaValoresPorDefecto()
    {
        var (defs, errores) = Normalizar(
            Var("salud", VariableTipos.Numero, min: 0, max: 100),
            Var("pareja", VariableTipos.Booleano),
            Var("tiempo", VariableTipos.Texto, valores: new[] { "mañana", "tarde" }));

        Assert.Empty(errores);
        Assert.Equal("salud", defs.Variables[0].Etiqueta);
        Assert.Equal(VariableHud.Oculto, defs.Variables[0].Hud);
        Assert.Equal(0d, defs.Variables[0].Inicial!.Value.GetDouble());
        Assert.False(defs.Variables[1].Inicial!.Value.GetBoolean());
        Assert.Equal("mañana", defs.Variables[2].Inicial!.Value.GetString());
    }

    [Fact]
    public void Normalizar_ClaveRepetidaOInvalida_ReportaError()
    {
        var (_, errores) = Normalizar(
            Var("salud", VariableTipos.Numero),
            Var("salud", VariableTipos.Numero),
            Var("Mala Clave", VariableTipos.Numero));

        Assert.Equal(2, errores.Count);
    }

    [Fact]
    public void Normalizar_RangoInicialYBarra_SeValidan()
    {
        var (_, errores) = Normalizar(
            Var("a", VariableTipos.Numero, JsonSerializer.SerializeToElement(500), 0, 100),
            Var("b", VariableTipos.Numero, min: 10, max: 5),
            Var("c", VariableTipos.Numero, hud: VariableHud.Barra),
            Var("d", VariableTipos.Booleano, hud: VariableHud.Barra));

        Assert.Equal(4, errores.Count);
    }

    [Fact]
    public void Normalizar_InicialConTipoEquivocado_ReportaError()
    {
        var (_, errores) = Normalizar(
            Var("a", VariableTipos.Numero, JsonSerializer.SerializeToElement("hola")),
            Var("b", VariableTipos.Booleano, JsonSerializer.SerializeToElement(1)),
            Var("c", VariableTipos.Texto, JsonSerializer.SerializeToElement("noche"), valores: new[] { "dia" }));

        Assert.Equal(3, errores.Count);
    }

    [Fact]
    public void LeerDefiniciones_JsonCorruptoOVacio_DevuelveVacias()
    {
        Assert.Empty(MotorJson.LeerDefiniciones("no es json").Variables);
        Assert.Empty(MotorJson.LeerDefiniciones("").Variables);
        Assert.Empty(MotorJson.LeerDefiniciones("{}").Variables);
    }

    [Fact]
    public void SerializarYLeer_HaceRoundTripEnCamelCase()
    {
        var (defs, _) = Normalizar(Var("salud", VariableTipos.Numero, min: 0, max: 100, hud: VariableHud.Barra));

        var json = MotorJson.Serializar(defs);

        Assert.Contains("\"variables\"", json);
        Assert.Contains("\"clave\":\"salud\"", json);
        Assert.Equal("salud", MotorJson.LeerDefiniciones(json).Variables.Single().Clave);
    }
}
