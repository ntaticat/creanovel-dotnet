using System;
using System.Collections.Generic;
using System.Text.Json;
using WebAPI.Features.Escenas;

namespace WebAPI.Features.NovelaVersiones
{
    public record NovelaVersionDto(Guid NovelaVersionId, string NumeroVersion, bool Disponible, bool EsBorrador, Guid NovelaId);

    public record NovelaVersionPopulatedDto(
        Guid NovelaVersionId,
        string NumeroVersion,
        bool Disponible,
        bool EsBorrador,
        Guid NovelaId,
        ICollection<EscenaRecursosDto> Escenas,
        JsonElement? Definiciones
    );
}
