using System;
using System.Collections.Generic;
using System.Linq;
using Domain.Models;
using WebAPI.Features.Recursos;

namespace WebAPI.Features.Escenas
{
    public record EscenaRecursosDto(
        Guid EscenaId,
        string Identificador,
        Guid NovelaVersionId,
        bool PrimerEscena,
        bool UltimaEscena,
        IReadOnlyList<string> Etiquetas,
        ICollection<object> Recursos
    );

    public static class EscenaMapping
    {
        public static EscenaRecursosDto ToDto(Escena escena) => new(
            escena.EscenaId,
            escena.Identificador,
            escena.NovelaVersionId,
            escena.PrimerEscena,
            escena.UltimaEscena,
            EscenaEtiquetas.Leer(escena.Etiquetas),
            escena.Recursos.Select(RecursoMapping.ToDto).ToList()
        );
    }
}
