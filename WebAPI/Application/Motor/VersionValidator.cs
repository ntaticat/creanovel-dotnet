using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Domain.Models;
using Microsoft.EntityFrameworkCore;
using Persistence;
using WebAPI.Features.Recursos;

namespace Application.Motor
{
    public record ValidacionIssue(string Codigo, string Mensaje, Guid? RecursoId);

    public record ValidacionVersionDto(bool Valida, List<ValidacionIssue> Errores, List<ValidacionIssue> Advertencias);

    // Revisa una versión completa antes de publicarla. Los errores bloquean la publicación; las
    // advertencias no. Solo marca como error lo que rompería la partida o el motor.
    public static class VersionValidator
    {
        public static async Task<ValidacionVersionDto> ValidarAsync(CreanovelDbContext context, Guid novelaVersionId, CancellationToken ct)
        {
            var version = await context.NovelaVersiones
                .Include(nv => nv.Escenas)
                .ThenInclude(e => e.Recursos)
                .FirstOrDefaultAsync(nv => nv.NovelaVersionId == novelaVersionId, ct);

            if (version == null)
            {
                return new ValidacionVersionDto(false, new List<ValidacionIssue> { new("version_no_encontrada", "NovelaVersion no encontrada", null) }, new List<ValidacionIssue>());
            }

            await context.RecursoDecisionOpciones
                .Where(o => o.RecursoDecision.Escena.NovelaVersionId == novelaVersionId)
                .LoadAsync(ct);

            return Validar(version);
        }

        public static ValidacionVersionDto Validar(NovelaVersion version)
        {
            var errores = new List<ValidacionIssue>();
            var advertencias = new List<ValidacionIssue>();

            var definiciones = MotorJson.LeerDefiniciones(version.Definiciones);
            var (_, erroresDefiniciones) = MotorJson.NormalizarYValidar(definiciones);
            errores.AddRange(erroresDefiniciones.Select(e => new ValidacionIssue("definiciones_invalidas", e, null)));
            var ctx = ReglasContexto.Desde(definiciones);

            var escenas = version.Escenas.ToList();
            var recursos = escenas.SelectMany(e => e.Recursos).ToList();
            var porId = recursos.ToDictionary(r => r.RecursoId);

            foreach (var recurso in recursos)
            {
                ValidarEnlaces(recurso, porId, errores);

                switch (recurso.TipoRecurso)
                {
                    case RecursoTipos.Evalua:
                        ValidarEvalua(recurso, ctx, errores);
                        break;
                    case RecursoTipos.Asigna:
                        ValidarAsigna(recurso, ctx, errores);
                        break;
                    case RecursoTipos.Decision:
                        ValidarOpciones(recurso, ctx, errores);
                        break;
                    case RecursoTipos.Explora:
                        ValidarExplora(recurso, ctx, errores, advertencias);
                        break;
                    case RecursoTipos.Juega:
                        ValidarJuega(recurso, ctx, errores, advertencias);
                        break;
                    case RecursoTipos.Termina:
                        ValidarTermina(recurso, ctx, errores);
                        break;
                    case RecursoTipos.Entrada:
                        ValidarEntrada(recurso, ctx, advertencias);
                        break;
                }
            }

            foreach (var recurso in recursos)
            {
                AvisarTextos(recurso, ctx, advertencias);
            }

            ValidarDestinosDeObjetos(definiciones, porId, errores);
            AvisarFinalesSinNodo(recursos, definiciones, advertencias);
            ValidarAlcanzabilidad(escenas, recursos, porId, definiciones, advertencias);
            ValidarCiclosAutomaticos(recursos, porId, advertencias);

            return new ValidacionVersionDto(errores.Count == 0, errores, advertencias);
        }

        private static IEnumerable<Guid> Destinos(Recurso recurso)
        {
            if (recurso.SiguienteRecursoId.HasValue)
            {
                yield return recurso.SiguienteRecursoId.Value;
            }

            foreach (var opcion in recurso.Opciones ?? new List<RecursoDecisionOpcion>())
            {
                if (opcion.SiguienteRecursoId.HasValue)
                {
                    yield return opcion.SiguienteRecursoId.Value;
                }
            }
        }

        private static void ValidarEnlaces(Recurso recurso, Dictionary<Guid, Recurso> porId, List<ValidacionIssue> errores)
        {
            if (Destinos(recurso).Any(d => !porId.ContainsKey(d)))
            {
                errores.Add(new ValidacionIssue("enlace_fuera_de_version", "Un recurso apunta a otro que no pertenece a esta versión", recurso.RecursoId));
            }
        }

        private static void ValidarEvalua(Recurso recurso, ReglasContexto ctx, List<ValidacionIssue> errores)
        {
            var ramas = (recurso.Opciones ?? new List<RecursoDecisionOpcion>()).Where(o => o.Tipo == RecursoSalidaTipos.Rama).ToList();

            if (ramas.Count == 0)
            {
                errores.Add(new ValidacionIssue("evalua_sin_ramas", "El nodo Evalua no tiene ninguna rama", recurso.RecursoId));
            }

            if (!recurso.SiguienteRecursoId.HasValue)
            {
                errores.Add(new ValidacionIssue("evalua_sin_sino", "El nodo Evalua necesita un destino 'sino' para cuando ninguna rama se cumple", recurso.RecursoId));
            }

            foreach (var rama in ramas)
            {
                if (string.IsNullOrWhiteSpace(rama.Condicion))
                {
                    errores.Add(new ValidacionIssue("rama_sin_condicion", "Una rama de Evalua no tiene condición", recurso.RecursoId));
                }
            }

            ValidarOpciones(recurso, ctx, errores);
        }

        private static void ValidarAsigna(Recurso recurso, ReglasContexto ctx, List<ValidacionIssue> errores)
        {
            var mensajes = new List<string>();
            JsonElement? contenido = null;

            try
            {
                using var doc = JsonDocument.Parse(recurso.Contenido);
                contenido = doc.RootElement.Clone();
            }
            catch (JsonException)
            {
            }

            var efectos = RecursoContenidoNormalizador.LeerEfectos(contenido);

            if (efectos == null)
            {
                mensajes.Add("El nodo Asigna no tiene una lista de efectos válida");
            }
            else
            {
                MotorValidador.ValidarEfectos(efectos.Value, ctx, "efectos", mensajes);
            }

            errores.AddRange(mensajes.Select(m => new ValidacionIssue("asigna_invalido", m, recurso.RecursoId)));
        }

        private static string LeerPropiedadTexto(string contenido, string propiedad)
        {
            try
            {
                using var doc = JsonDocument.Parse(contenido);

                if (doc.RootElement.ValueKind == JsonValueKind.Object)
                {
                    foreach (var p in doc.RootElement.EnumerateObject())
                    {
                        if (p.Name == propiedad && p.Value.ValueKind == JsonValueKind.String)
                        {
                            return p.Value.GetString();
                        }
                    }
                }
            }
            catch (JsonException)
            {
            }

            return null;
        }

        // Termina: debe citar un final del catálogo.
        private static void ValidarTermina(Recurso recurso, ReglasContexto ctx, List<ValidacionIssue> errores)
        {
            var final = LeerPropiedadTexto(recurso.Contenido, "Final");

            if (string.IsNullOrEmpty(final) || !ctx.Finales.Contains(final))
            {
                errores.Add(new ValidacionIssue("termina_invalido", $"El nodo Termina cita un final que no está definido ('{final}')", recurso.RecursoId));
            }
        }

        // Un final del catálogo que ningún nodo Termina usa es un final imposible de ver.
        private static void AvisarFinalesSinNodo(List<Recurso> recursos, DefinicionesJuego definiciones, List<ValidacionIssue> advertencias)
        {
            var usados = recursos
                .Where(r => r.TipoRecurso == RecursoTipos.Termina)
                .Select(r => LeerPropiedadTexto(r.Contenido, "Final"))
                .ToHashSet();

            foreach (var final in definiciones.Finales ?? new List<MetaDef>())
            {
                if (!usados.Contains(final.Id))
                {
                    advertencias.Add(new ValidacionIssue("final_sin_nodo", $"El final «{final.Nombre}» no lo usa ningún nodo Termina", null));
                }
            }
        }

        // Los textos pueden citar variables entre llaves ({salud}, {objeto:llave}, {ubicacion}). Una que no existe se muestra tal cual
        // al jugador: se avisa (no se bloquea, para no romper novelas antiguas con llaves en la prosa). Los textos condicionales
        // ({si ...: a | b}) los valida el editor al escribirlos.
        private static readonly Regex CitaVariable = new(@"(?<!\{)\{([a-z][a-z0-9_]*)\}(?!\})", RegexOptions.Compiled);
        private static readonly Regex CitaObjeto = new(@"(?<!\{)\{objeto:([a-z][a-z0-9_]*)\}(?!\})", RegexOptions.Compiled);

        private static void AvisarTextos(Recurso recurso, ReglasContexto ctx, List<ValidacionIssue> advertencias)
        {
            var textos = new List<string>();

            foreach (var propiedad in new[] { "Mensaje", "DecisionMensaje", "Etiqueta" })
            {
                var texto = LeerPropiedadTexto(recurso.Contenido, propiedad);
                if (!string.IsNullOrEmpty(texto)) textos.Add(texto);
            }

            foreach (var opcion in recurso.Opciones ?? new List<RecursoDecisionOpcion>())
            {
                if (!string.IsNullOrEmpty(opcion.OpcionMensaje)) textos.Add(opcion.OpcionMensaje);
            }

            var avisados = new HashSet<string>();

            foreach (var texto in textos)
            {
                foreach (Match m in CitaVariable.Matches(texto))
                {
                    var clave = m.Groups[1].Value;
                    if (clave != "ubicacion" && !ctx.Variables.ContainsKey(clave) && avisados.Add("v:" + clave))
                    {
                        advertencias.Add(new ValidacionIssue("texto_variable_desconocida", $"El texto cita {{{clave}}}, que no es una variable definida", recurso.RecursoId));
                    }
                }

                foreach (Match m in CitaObjeto.Matches(texto))
                {
                    var id = m.Groups[1].Value;
                    if (!ctx.Objetos.ContainsKey(id) && avisados.Add("o:" + id))
                    {
                        advertencias.Add(new ValidacionIssue("texto_variable_desconocida", $"El texto cita {{objeto:{id}}}, que no es un objeto definido", recurso.RecursoId));
                    }
                }
            }
        }

        // Juega: configuración válida, una salida de éxito y una de fallo, y una variable de puntaje numérica si se usa.
        private static void ValidarJuega(Recurso recurso, ReglasContexto ctx, List<ValidacionIssue> errores, List<ValidacionIssue> advertencias)
        {
            var mensajes = new List<string>();
            JsonElement? contenido = null;

            try
            {
                using var doc = JsonDocument.Parse(recurso.Contenido);
                contenido = doc.RootElement.Clone();
            }
            catch (JsonException)
            {
            }

            JsonElement? minijuego = null;
            string resultado = null;

            if (contenido != null && contenido.Value.ValueKind == JsonValueKind.Object)
            {
                foreach (var p in contenido.Value.EnumerateObject())
                {
                    if (p.Name == "Minijuego") minijuego = p.Value;
                    if (p.Name == "VariableResultado" && p.Value.ValueKind == JsonValueKind.String) resultado = p.Value.GetString();
                }
            }

            if (minijuego == null)
            {
                mensajes.Add("El nodo Juega no tiene un minijuego configurado");
            }
            else
            {
                MinijuegoValidador.Validar(minijuego.Value, "minijuego", mensajes);
            }

            if (!string.IsNullOrEmpty(resultado) && (!ctx.Variables.TryGetValue(resultado, out var variable) || variable.Tipo != VariableTipos.Numero))
            {
                mensajes.Add($"La variable de puntaje '{resultado}' debe ser una variable numérica definida");
            }

            errores.AddRange(mensajes.Select(m => new ValidacionIssue("juega_invalido", m, recurso.RecursoId)));

            var salidas = recurso.Opciones ?? new List<RecursoDecisionOpcion>();

            foreach (var (tipo, codigo, nombre) in new[] { (RecursoSalidaTipos.Exito, "juega_sin_exito", "éxito"), (RecursoSalidaTipos.Fallo, "juega_sin_fallo", "fallo") })
            {
                var coinciden = salidas.Where(o => o.Tipo == tipo).ToList();

                if (coinciden.Count != 1)
                {
                    errores.Add(new ValidacionIssue(codigo, $"El nodo Juega necesita exactamente una salida de {nombre}", recurso.RecursoId));
                }
                else if (!coinciden[0].SiguienteRecursoId.HasValue && string.IsNullOrWhiteSpace(coinciden[0].Efectos))
                {
                    advertencias.Add(new ValidacionIssue("salida_sin_destino", $"La salida de {nombre} no lleva a ningún sitio: la historia terminará ahí", recurso.RecursoId));
                }
            }

            ValidarOpciones(recurso, ctx, errores);
        }

        // Explora: cada zona necesita nombre y una región dentro del escenario; sin zonas el jugador quedaría atrapado.
        private static void ValidarExplora(Recurso recurso, ReglasContexto ctx, List<ValidacionIssue> errores, List<ValidacionIssue> advertencias)
        {
            var zonas = (recurso.Opciones ?? new List<RecursoDecisionOpcion>()).Where(o => o.Tipo == RecursoSalidaTipos.Zona).ToList();

            if (zonas.Count == 0)
            {
                errores.Add(new ValidacionIssue("explora_sin_zonas", "El nodo Explora no tiene ninguna zona clicable", recurso.RecursoId));
            }

            foreach (var zona in zonas)
            {
                var mensajes = new List<string>();

                if (string.IsNullOrWhiteSpace(zona.OpcionMensaje))
                {
                    mensajes.Add("Una zona no tiene nombre");
                }

                var region = MotorJson.ParseNullable(zona.Region);
                if (region == null)
                {
                    mensajes.Add("Una zona no tiene región");
                }
                else
                {
                    MotorValidador.ValidarRegion(region.Value, "region", mensajes);
                }

                errores.AddRange(mensajes.Select(m => new ValidacionIssue("zona_invalida", m, recurso.RecursoId)));

                if (!zona.SiguienteRecursoId.HasValue && string.IsNullOrWhiteSpace(zona.Efectos))
                {
                    advertencias.Add(new ValidacionIssue("zona_sin_efecto", $"La zona «{zona.OpcionMensaje}» no lleva a ningún sitio ni cambia nada", recurso.RecursoId));
                }
            }

            ValidarOpciones(recurso, ctx, errores);
        }

        // Aviso y no error: los Pide anteriores al motor guardaban `Clave` sin variable asociada.
        private static void ValidarEntrada(Recurso recurso, ReglasContexto ctx, List<ValidacionIssue> advertencias)
        {
            string clave = null;

            try
            {
                using var doc = JsonDocument.Parse(recurso.Contenido);
                foreach (var propiedad in doc.RootElement.EnumerateObject())
                {
                    if (propiedad.Name == "Clave" && propiedad.Value.ValueKind == JsonValueKind.String)
                    {
                        clave = propiedad.Value.GetString();
                    }
                }
            }
            catch (JsonException)
            {
            }

            if (string.IsNullOrEmpty(clave) || !ctx.Variables.TryGetValue(clave, out var def) || def.Tipo == VariableTipos.Booleano)
            {
                advertencias.Add(new ValidacionIssue("entrada_sin_variable", "Este nodo Pide no guarda lo escrito en una variable de texto o número definida", recurso.RecursoId));
            }
        }

        private static void ValidarOpciones(Recurso recurso, ReglasContexto ctx, List<ValidacionIssue> errores)
        {
            foreach (var opcion in recurso.Opciones ?? new List<RecursoDecisionOpcion>())
            {
                var mensajes = new List<string>();

                if (!string.IsNullOrWhiteSpace(opcion.Condicion))
                {
                    MotorValidador.ValidarCondicion(MotorJson.ParseNullable(opcion.Condicion).Value, ctx, "condicion", mensajes);
                }

                if (!string.IsNullOrWhiteSpace(opcion.Efectos))
                {
                    MotorValidador.ValidarEfectos(MotorJson.ParseNullable(opcion.Efectos).Value, ctx, "efectos", mensajes);
                }

                errores.AddRange(mensajes.Select(m => new ValidacionIssue("condicion_o_efecto_invalido", m, recurso.RecursoId)));
            }
        }

        private static Recurso ResolverInicio(List<Escena> escenas)
        {
            var escena = escenas.FirstOrDefault(e => e.PrimerEscena) ?? escenas.FirstOrDefault();
            return escena?.Recursos.FirstOrDefault(r => r.PrimerRecurso) ?? escena?.Recursos.FirstOrDefault();
        }

        private static void ValidarDestinosDeObjetos(DefinicionesJuego definiciones, Dictionary<Guid, Recurso> porId, List<ValidacionIssue> errores)
        {
            foreach (var (objetoId, recursoId) in UsoObjetoDestinos.De(definiciones).Where(d => !porId.ContainsKey(d.RecursoId)))
            {
                errores.Add(new ValidacionIssue("uso_objeto_destino_invalido", $"El objeto '{objetoId}' lleva a un nodo que no existe en esta versión", null));
            }
        }

        private static void ValidarAlcanzabilidad(List<Escena> escenas, List<Recurso> recursos, Dictionary<Guid, Recurso> porId, DefinicionesJuego definiciones, List<ValidacionIssue> advertencias)
        {
            var inicio = ResolverInicio(escenas);

            if (inicio == null)
            {
                advertencias.Add(new ValidacionIssue("sin_inicio", "La versión no tiene ningún recurso por donde empezar", null));
                return;
            }

            var visitados = new HashSet<Guid>();
            var pendientes = new Stack<Recurso>();
            pendientes.Push(inicio);

            // Un nodo al que lleva el uso de un objeto también se alcanza (aunque solo si el jugador tiene el objeto).
            foreach (var (_, destino) in UsoObjetoDestinos.De(definiciones))
            {
                if (porId.TryGetValue(destino, out var deObjeto))
                {
                    pendientes.Push(deObjeto);
                }
            }

            while (pendientes.Count > 0)
            {
                var actual = pendientes.Pop();

                if (!visitados.Add(actual.RecursoId))
                {
                    continue;
                }

                foreach (var destino in Destinos(actual))
                {
                    if (porId.TryGetValue(destino, out var siguiente))
                    {
                        pendientes.Push(siguiente);
                    }
                }
            }

            foreach (var recurso in recursos.Where(r => !visitados.Contains(r.RecursoId)))
            {
                advertencias.Add(new ValidacionIssue("inalcanzable", "Este recurso no se puede alcanzar desde el inicio", recurso.RecursoId));
            }
        }

        // Un ciclo formado solo por nodos automáticos (Evalua/Asigna) es válido si una condición lo corta
        // (p. ej. un contador), pero el motor lo interrumpe tras un máximo de pasos. Se avisa al autor.
        private static void ValidarCiclosAutomaticos(List<Recurso> recursos, Dictionary<Guid, Recurso> porId, List<ValidacionIssue> advertencias)
        {
            static bool EsAutomatico(Recurso r) => r.TipoRecurso == RecursoTipos.Evalua || r.TipoRecurso == RecursoTipos.Asigna;

            var automaticos = recursos.Where(EsAutomatico).ToList();
            var estado = new Dictionary<Guid, int>(); // 1 = en curso, 2 = terminado
            var reportados = new HashSet<Guid>();

            void Visitar(Recurso recurso)
            {
                estado[recurso.RecursoId] = 1;

                foreach (var destino in Destinos(recurso))
                {
                    if (!porId.TryGetValue(destino, out var siguiente) || !EsAutomatico(siguiente))
                    {
                        continue;
                    }

                    if (!estado.TryGetValue(siguiente.RecursoId, out var e))
                    {
                        Visitar(siguiente);
                    }
                    else if (e == 1 && reportados.Add(siguiente.RecursoId))
                    {
                        advertencias.Add(new ValidacionIssue("ciclo_automatico", "Hay un ciclo de nodos automáticos (Evalua/Asigna) sin ningún diálogo dentro; el motor lo cortará tras un máximo de pasos", siguiente.RecursoId));
                    }
                }

                estado[recurso.RecursoId] = 2;
            }

            foreach (var recurso in automaticos.Where(r => !estado.ContainsKey(r.RecursoId)))
            {
                Visitar(recurso);
            }
        }
    }
}
