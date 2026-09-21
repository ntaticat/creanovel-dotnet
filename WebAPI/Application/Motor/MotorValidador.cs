using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace Application.Motor
{
    // Valida la forma de condiciones y efectos contra lo definido en la versión (variables, objetos, ubicaciones).
    //
    // Condición (objeto JSON, exactamente una de estas formas):
    //   { "y": [cond, ...] }   { "o": [cond, ...] }   { "no": cond }
    //   { "var": "clave", "op": "==|!=|>|>=|<|<=", "valor": <número|booleano|texto> }
    //   { "objeto": "id", "op": "==|!=|>|>=|<|<=", "valor": <cantidad entera> }
    //   { "en": "ubicacion_id" }                       (el jugador está en esa ubicación)
    //   { "logro": "id" }  { "final": "id" }           (ya desbloqueó ese logro / ya vio ese final, en esta o en partidas anteriores)
    //
    // Efectos (arreglo JSON de):
    //   { "var": "clave", "op": "fijar|sumar|restar|multiplicar|alternar", "valor": ... }
    //   { "objeto": "id", "op": "dar|quitar", "cantidad": <entero, opcional, 1 por defecto> }
    //   { "ir": "ubicacion_id" }
    //   { "logro": "id" }                              (desbloquea un logro)
    //   "avanzar" (en un efecto de variable): pasa a la siguiente entre los valores de una variable de texto, dando la vuelta (mañana → tarde → noche → mañana)
    //
    // Es un AST cerrado: el cliente lo interpreta sin evaluar texto, así que un autor no puede
    // ejecutar código en el navegador de quien juega su novela.
    public static class MotorValidador
    {
        public static readonly string[] OpsComparacion = { "==", "!=", ">", ">=", "<", "<=" };
        public static readonly string[] OpsIgualdad = { "==", "!=" };
        public static readonly string[] OpsEfecto = { "fijar", "sumar", "restar", "multiplicar", "alternar", "avanzar" };
        public static readonly string[] OpsEfectoNumerico = { "sumar", "restar", "multiplicar" };
        public static readonly string[] OpsEfectoObjeto = { "dar", "quitar" };

        public const int ProfundidadMaxima = 8;
        public const int MaxEfectos = 50;
        public const int MaxHijos = 20;

        public static void ValidarCondicion(JsonElement cond, IReadOnlyDictionary<string, VariableDef> vars, string ruta, List<string> errores, int profundidad = 0) =>
            ValidarCondicion(cond, ReglasContexto.SoloVariables(vars), ruta, errores, profundidad);

        public static void ValidarEfectos(JsonElement efectos, IReadOnlyDictionary<string, VariableDef> vars, string ruta, List<string> errores) =>
            ValidarEfectos(efectos, ReglasContexto.SoloVariables(vars), ruta, errores);

        public static void ValidarCondicion(JsonElement cond, ReglasContexto ctx, string ruta, List<string> errores, int profundidad = 0)
        {
            if (profundidad > ProfundidadMaxima)
            {
                errores.Add($"{ruta}: la condición está anidada demasiado profundo");
                return;
            }

            if (cond.ValueKind != JsonValueKind.Object)
            {
                errores.Add($"{ruta}: la condición debe ser un objeto");
                return;
            }

            var props = cond.EnumerateObject().Select(p => p.Name).ToList();

            if (props.Count == 1 && (props[0] == "y" || props[0] == "o"))
            {
                var hijos = cond.GetProperty(props[0]);

                if (hijos.ValueKind != JsonValueKind.Array || hijos.GetArrayLength() == 0 || hijos.GetArrayLength() > MaxHijos)
                {
                    errores.Add($"{ruta}: '{props[0]}' requiere entre 1 y {MaxHijos} condiciones");
                    return;
                }

                var i = 0;
                foreach (var hijo in hijos.EnumerateArray())
                {
                    ValidarCondicion(hijo, ctx, $"{ruta}.{props[0]}[{i++}]", errores, profundidad + 1);
                }
                return;
            }

            if (props.Count == 1 && props[0] == "no")
            {
                ValidarCondicion(cond.GetProperty("no"), ctx, $"{ruta}.no", errores, profundidad + 1);
                return;
            }

            if (props.Count == 1 && props[0] == "en")
            {
                ValidarUbicacion(cond.GetProperty("en"), ctx, ruta, errores);
                return;
            }

            if (props.Count == 1 && props[0] == "logro")
            {
                ValidarReferencia(cond.GetProperty("logro"), ctx.Logros, "logro", ruta, errores);
                return;
            }

            if (props.Count == 1 && props[0] == "final")
            {
                ValidarReferencia(cond.GetProperty("final"), ctx.Finales, "final", ruta, errores);
                return;
            }

            if (props.Count == 3 && props.Contains("op") && props.Contains("valor"))
            {
                if (props.Contains("var"))
                {
                    ValidarComparacion(cond, ctx, ruta, errores);
                    return;
                }

                if (props.Contains("objeto"))
                {
                    ValidarComparacionObjeto(cond, ctx, ruta, errores);
                    return;
                }
            }

            errores.Add($"{ruta}: condición no reconocida");
        }

        private static void ValidarComparacion(JsonElement cond, ReglasContexto ctx, string ruta, List<string> errores)
        {
            if (!TryVariable(cond, ctx, ruta, errores, out var def))
            {
                return;
            }

            var op = cond.GetProperty("op").ValueKind == JsonValueKind.String ? cond.GetProperty("op").GetString() : null;

            if (op == null || !OpsComparacion.Contains(op))
            {
                errores.Add($"{ruta}: operador inválido");
                return;
            }

            var valor = cond.GetProperty("valor");

            switch (def.Tipo)
            {
                case VariableTipos.Numero:
                    if (valor.ValueKind != JsonValueKind.Number) errores.Add($"{ruta}: '{def.Clave}' se compara con un número");
                    break;
                case VariableTipos.Booleano:
                    if (valor.ValueKind != JsonValueKind.True && valor.ValueKind != JsonValueKind.False) errores.Add($"{ruta}: '{def.Clave}' se compara con verdadero o falso");
                    if (!OpsIgualdad.Contains(op)) errores.Add($"{ruta}: '{def.Clave}' solo admite == y !=");
                    break;
                default:
                    if (valor.ValueKind != JsonValueKind.String) errores.Add($"{ruta}: '{def.Clave}' se compara con texto");
                    else if (def.Valores != null && def.Valores.Count > 0 && !def.Valores.Contains(valor.GetString())) errores.Add($"{ruta}: '{valor.GetString()}' no es un valor válido de '{def.Clave}'");
                    if (!OpsIgualdad.Contains(op)) errores.Add($"{ruta}: '{def.Clave}' solo admite == y !=");
                    break;
            }
        }

        // La cantidad de un objeto se compara con un entero: `{ objeto: "llave", op: ">=", valor: 1 }`.
        private static void ValidarComparacionObjeto(JsonElement cond, ReglasContexto ctx, string ruta, List<string> errores)
        {
            if (!TryObjeto(cond, ctx, ruta, errores, out _))
            {
                return;
            }

            var op = cond.GetProperty("op").ValueKind == JsonValueKind.String ? cond.GetProperty("op").GetString() : null;

            if (op == null || !OpsComparacion.Contains(op))
            {
                errores.Add($"{ruta}: operador inválido");
            }

            if (!EsEntero(cond.GetProperty("valor"), 0))
            {
                errores.Add($"{ruta}: la cantidad de un objeto se compara con un número entero desde 0");
            }
        }

        public static void ValidarEfectos(JsonElement efectos, ReglasContexto ctx, string ruta, List<string> errores)
        {
            if (efectos.ValueKind != JsonValueKind.Array)
            {
                errores.Add($"{ruta}: los efectos deben ser una lista");
                return;
            }

            if (efectos.GetArrayLength() > MaxEfectos)
            {
                errores.Add($"{ruta}: se permiten como máximo {MaxEfectos} efectos");
                return;
            }

            var i = 0;
            foreach (var efecto in efectos.EnumerateArray())
            {
                ValidarEfecto(efecto, ctx, $"{ruta}[{i++}]", errores);
            }
        }

        private static void ValidarEfecto(JsonElement efecto, ReglasContexto ctx, string ruta, List<string> errores)
        {
            if (efecto.ValueKind != JsonValueKind.Object)
            {
                errores.Add($"{ruta}: el efecto debe ser un objeto");
                return;
            }

            var props = efecto.EnumerateObject().Select(p => p.Name).ToList();

            if (props.Count == 1 && props[0] == "ir")
            {
                ValidarUbicacion(efecto.GetProperty("ir"), ctx, ruta, errores);
                return;
            }

            if (props.Count == 1 && props[0] == "logro")
            {
                ValidarReferencia(efecto.GetProperty("logro"), ctx.Logros, "logro", ruta, errores);
                return;
            }

            if (props.Contains("objeto"))
            {
                ValidarEfectoObjeto(efecto, props, ctx, ruta, errores);
                return;
            }

            if (!props.Contains("var") || !props.Contains("op") || props.Any(p => p != "var" && p != "op" && p != "valor"))
            {
                errores.Add($"{ruta}: efecto no reconocido");
                return;
            }

            if (!TryVariable(efecto, ctx, ruta, errores, out var def))
            {
                return;
            }

            var op = efecto.GetProperty("op").ValueKind == JsonValueKind.String ? efecto.GetProperty("op").GetString() : null;

            if (op == null || !OpsEfecto.Contains(op))
            {
                errores.Add($"{ruta}: operación inválida");
                return;
            }

            var tieneValor = props.Contains("valor");
            var valor = tieneValor ? efecto.GetProperty("valor") : default;

            if (op == "alternar")
            {
                if (def.Tipo != VariableTipos.Booleano) errores.Add($"{ruta}: 'alternar' solo aplica a variables booleanas");
                if (tieneValor) errores.Add($"{ruta}: 'alternar' no lleva valor");
                return;
            }

            if (op == "avanzar")
            {
                if (def.Tipo != VariableTipos.Texto || def.Valores == null || def.Valores.Count < 2)
                {
                    errores.Add($"{ruta}: 'avanzar' solo aplica a variables de texto con al menos dos valores permitidos");
                }
                if (tieneValor) errores.Add($"{ruta}: 'avanzar' no lleva valor");
                return;
            }

            if (!tieneValor)
            {
                errores.Add($"{ruta}: falta el valor");
                return;
            }

            if (OpsEfectoNumerico.Contains(op))
            {
                if (def.Tipo != VariableTipos.Numero) errores.Add($"{ruta}: '{op}' solo aplica a variables numéricas");
                else if (valor.ValueKind != JsonValueKind.Number) errores.Add($"{ruta}: '{op}' requiere un número");
                return;
            }

            // fijar
            switch (def.Tipo)
            {
                case VariableTipos.Numero:
                    if (valor.ValueKind != JsonValueKind.Number) errores.Add($"{ruta}: '{def.Clave}' se fija con un número");
                    break;
                case VariableTipos.Booleano:
                    if (valor.ValueKind != JsonValueKind.True && valor.ValueKind != JsonValueKind.False) errores.Add($"{ruta}: '{def.Clave}' se fija con verdadero o falso");
                    break;
                default:
                    if (valor.ValueKind != JsonValueKind.String) errores.Add($"{ruta}: '{def.Clave}' se fija con texto");
                    else if (def.Valores != null && def.Valores.Count > 0 && !def.Valores.Contains(valor.GetString())) errores.Add($"{ruta}: '{valor.GetString()}' no es un valor válido de '{def.Clave}'");
                    break;
            }
        }

        private static void ValidarEfectoObjeto(JsonElement efecto, List<string> props, ReglasContexto ctx, string ruta, List<string> errores)
        {
            if (!props.Contains("op") || props.Any(p => p != "objeto" && p != "op" && p != "cantidad"))
            {
                errores.Add($"{ruta}: efecto no reconocido");
                return;
            }

            if (!TryObjeto(efecto, ctx, ruta, errores, out _))
            {
                return;
            }

            var op = efecto.GetProperty("op").ValueKind == JsonValueKind.String ? efecto.GetProperty("op").GetString() : null;

            if (op == null || !OpsEfectoObjeto.Contains(op))
            {
                errores.Add($"{ruta}: un objeto solo se puede dar o quitar");
                return;
            }

            if (props.Contains("cantidad") && !EsEntero(efecto.GetProperty("cantidad"), 1))
            {
                errores.Add($"{ruta}: la cantidad debe ser un entero desde 1");
            }
        }

        private static void ValidarUbicacion(JsonElement id, ReglasContexto ctx, string ruta, List<string> errores) =>
            ValidarReferencia(id, ctx.Ubicaciones, "ubicación", ruta, errores);

        private static void ValidarReferencia(JsonElement id, IReadOnlySet<string> definidos, string tipo, string ruta, List<string> errores)
        {
            if (id.ValueKind != JsonValueKind.String || !definidos.Contains(id.GetString()))
            {
                errores.Add($"{ruta}: {(tipo == "ubicación" ? "la" : "el")} {tipo} '{(id.ValueKind == JsonValueKind.String ? id.GetString() : id.ToString())}' no está definid{(tipo == "ubicación" ? "a" : "o")}");
            }
        }

        private static bool TryVariable(JsonElement nodo, ReglasContexto ctx, string ruta, List<string> errores, out VariableDef def)
        {
            def = null;
            var clave = nodo.GetProperty("var");

            if (clave.ValueKind != JsonValueKind.String || !ctx.Variables.TryGetValue(clave.GetString(), out def))
            {
                errores.Add($"{ruta}: la variable '{(clave.ValueKind == JsonValueKind.String ? clave.GetString() : clave.ToString())}' no está definida");
                return false;
            }

            return true;
        }

        private static bool TryObjeto(JsonElement nodo, ReglasContexto ctx, string ruta, List<string> errores, out ObjetoDef def)
        {
            def = null;
            var id = nodo.GetProperty("objeto");

            if (id.ValueKind != JsonValueKind.String || !ctx.Objetos.TryGetValue(id.GetString(), out def))
            {
                errores.Add($"{ruta}: el objeto '{(id.ValueKind == JsonValueKind.String ? id.GetString() : id.ToString())}' no está definido");
                return false;
            }

            return true;
        }

        private static bool EsEntero(JsonElement valor, int minimo) =>
            valor.ValueKind == JsonValueKind.Number
            && valor.TryGetInt32(out var n)
            && n >= minimo
            && n <= MotorJson.MaxCantidad;

        // Zona clicable de un Explora: rectángulo en % del escenario, `{ x, y, ancho, alto }`.
        public static void ValidarRegion(JsonElement region, string ruta, List<string> errores)
        {
            if (region.ValueKind != JsonValueKind.Object)
            {
                errores.Add($"{ruta}: la región debe ser un objeto {{ x, y, ancho, alto }}");
                return;
            }

            var props = region.EnumerateObject().Select(p => p.Name).ToList();
            var esperadas = new[] { "x", "y", "ancho", "alto" };

            if (props.Count != 4 || esperadas.Any(e => !props.Contains(e)))
            {
                errores.Add($"{ruta}: la región debe tener exactamente x, y, ancho y alto");
                return;
            }

            double Leer(string nombre) =>
                region.GetProperty(nombre).ValueKind == JsonValueKind.Number ? region.GetProperty(nombre).GetDouble() : double.NaN;

            var (x, y, ancho, alto) = (Leer("x"), Leer("y"), Leer("ancho"), Leer("alto"));

            if (double.IsNaN(x) || double.IsNaN(y) || double.IsNaN(ancho) || double.IsNaN(alto))
            {
                errores.Add($"{ruta}: x, y, ancho y alto deben ser números");
                return;
            }

            const double tolerancia = 0.0001;

            if (x < 0 || y < 0 || ancho <= 0 || alto <= 0 || x + ancho > 100 + tolerancia || y + alto > 100 + tolerancia)
            {
                errores.Add($"{ruta}: la región debe quedar dentro del escenario (0 a 100 %) y tener tamaño");
            }
        }
    }
}
