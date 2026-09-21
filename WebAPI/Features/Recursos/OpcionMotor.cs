using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.Json;
using Application.Handlers;
using Application.Motor;

namespace WebAPI.Features.Recursos
{
    // Validación compartida de condición/efectos/región de una opción, rama o zona (Add/UpdateRecursoDecisionOpcion).
    internal static class OpcionMotor
    {
        public const int MaxEtiquetaZona = 60;

        public static (string Condicion, string Efectos, string Region) Validar(
            string tipo, string etiqueta, JsonElement? condicion, JsonElement? efectos, JsonElement? region, string condicionModo, ReglasContexto ctx)
        {
            var errores = new List<string>();

            if (condicionModo != null && !CondicionModos.Todos.Contains(condicionModo))
            {
                errores.Add($"condicionModo inválido '{condicionModo}'");
            }

            var condicionJson = EsNulo(condicion) ? (JsonElement?)null : condicion.Value;
            var efectosJson = EsNulo(efectos) ? (JsonElement?)null : efectos.Value;
            var regionJson = EsNulo(region) ? (JsonElement?)null : region.Value;

            if (condicionJson != null)
            {
                MotorValidador.ValidarCondicion(condicionJson.Value, ctx, "condicion", errores);
            }

            if (efectosJson != null)
            {
                MotorValidador.ValidarEfectos(efectosJson.Value, ctx, "efectos", errores);
            }

            // Las salidas de un minijuego se toman por el resultado, no por una condición.
            if ((tipo == RecursoSalidaTipos.Exito || tipo == RecursoSalidaTipos.Fallo) && condicionJson != null)
            {
                errores.Add("condicion: la salida de un minijuego no lleva condición");
            }

            if (tipo == RecursoSalidaTipos.Zona)
            {
                // El nombre de la zona es su texto accesible (lector de pantalla / tooltip): sin él nadie sabría qué es.
                if (string.IsNullOrWhiteSpace(etiqueta) || etiqueta.Length > MaxEtiquetaZona)
                {
                    errores.Add($"mensaje: la zona necesita un nombre de hasta {MaxEtiquetaZona} caracteres");
                }

                if (regionJson == null)
                {
                    errores.Add("region: la zona necesita una región");
                }
                else
                {
                    MotorValidador.ValidarRegion(regionJson.Value, "region", errores);
                }
            }

            if (errores.Count > 0)
            {
                throw new ExceptionHandler(HttpStatusCode.BadRequest, new { message = "La condición, los efectos o la región no son válidos", errores });
            }

            // Solo las zonas llevan región; en otras salidas se descarta.
            return (condicionJson?.GetRawText(), efectosJson?.GetRawText(), tipo == RecursoSalidaTipos.Zona ? regionJson?.GetRawText() : null);
        }

        private static bool EsNulo(JsonElement? e) =>
            e == null || e.Value.ValueKind == JsonValueKind.Null || e.Value.ValueKind == JsonValueKind.Undefined;
    }
}
