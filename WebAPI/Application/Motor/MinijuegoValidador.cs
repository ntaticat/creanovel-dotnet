using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace Application.Motor
{
    public static class MinijuegoTipos
    {
        public const string Reflejo = "reflejo";
        public const string Precision = "precision";
        public const string Secuencia = "secuencia";
        public const string Pulsaciones = "pulsaciones";

        public static readonly string[] Todos = { Reflejo, Precision, Secuencia, Pulsaciones };
    }

    // Valida la configuración de un minijuego (nodo Juega). Cada tipo admite exactamente sus propiedades, con límites
    // que el servidor impone para que un autor no pueda crear algo injugable ni costoso de dibujar en el navegador.
    //
    //   { "tipo": "reflejo",     "objetivos": 1..20, "aciertosNecesarios": 1..objetivos, "duracionMs": 400..10000 }
    //   { "tipo": "precision",   "velocidad": 1..10, "anchoZona": 5..60, "intentos": 1..10 }
    //   { "tipo": "secuencia",   "longitud": 2..12, "tiempoMs": 1000..60000 }
    //   { "tipo": "pulsaciones", "objetivo": 3..200, "tiempoMs": 1000..30000 }
    public static class MinijuegoValidador
    {
        public static void Validar(JsonElement config, string ruta, List<string> errores)
        {
            if (config.ValueKind != JsonValueKind.Object)
            {
                errores.Add($"{ruta}: el minijuego debe ser un objeto");
                return;
            }

            var props = config.EnumerateObject().Select(p => p.Name).ToList();

            if (!props.Contains("tipo") || config.GetProperty("tipo").ValueKind != JsonValueKind.String)
            {
                errores.Add($"{ruta}: falta el tipo de minijuego");
                return;
            }

            var tipo = config.GetProperty("tipo").GetString();

            switch (tipo)
            {
                case MinijuegoTipos.Reflejo:
                    SoloPropiedades(props, new[] { "tipo", "objetivos", "aciertosNecesarios", "duracionMs" }, ruta, errores);
                    var objetivos = Entero(config, "objetivos", 1, 20, ruta, errores);
                    var necesarios = Entero(config, "aciertosNecesarios", 1, 20, ruta, errores);
                    Entero(config, "duracionMs", 400, 10000, ruta, errores);

                    if (objetivos.HasValue && necesarios.HasValue && necesarios > objetivos)
                    {
                        errores.Add($"{ruta}: los aciertos necesarios no pueden superar los objetivos");
                    }
                    break;

                case MinijuegoTipos.Precision:
                    SoloPropiedades(props, new[] { "tipo", "velocidad", "anchoZona", "intentos" }, ruta, errores);
                    Entero(config, "velocidad", 1, 10, ruta, errores);
                    Entero(config, "anchoZona", 5, 60, ruta, errores);
                    Entero(config, "intentos", 1, 10, ruta, errores);
                    break;

                case MinijuegoTipos.Secuencia:
                    SoloPropiedades(props, new[] { "tipo", "longitud", "tiempoMs" }, ruta, errores);
                    Entero(config, "longitud", 2, 12, ruta, errores);
                    Entero(config, "tiempoMs", 1000, 60000, ruta, errores);
                    break;

                case MinijuegoTipos.Pulsaciones:
                    SoloPropiedades(props, new[] { "tipo", "objetivo", "tiempoMs" }, ruta, errores);
                    Entero(config, "objetivo", 3, 200, ruta, errores);
                    Entero(config, "tiempoMs", 1000, 30000, ruta, errores);
                    break;

                default:
                    errores.Add($"{ruta}: tipo de minijuego desconocido '{tipo}'");
                    break;
            }
        }

        private static void SoloPropiedades(List<string> props, string[] permitidas, string ruta, List<string> errores)
        {
            var sobran = props.Where(p => !permitidas.Contains(p)).ToList();

            if (sobran.Count > 0)
            {
                errores.Add($"{ruta}: propiedades no admitidas: {string.Join(", ", sobran)}");
            }
        }

        private static int? Entero(JsonElement config, string nombre, int min, int max, string ruta, List<string> errores)
        {
            if (!config.TryGetProperty(nombre, out var valor)
                || valor.ValueKind != JsonValueKind.Number
                || !valor.TryGetInt32(out var n))
            {
                errores.Add($"{ruta}.{nombre}: debe ser un número entero");
                return null;
            }

            if (n < min || n > max)
            {
                errores.Add($"{ruta}.{nombre}: debe estar entre {min} y {max}");
                return null;
            }

            return n;
        }
    }
}
