using System;
using System.Collections.Generic;

namespace WebAPI.Features.Personajes
{
    public record PersonajeDto(Guid PersonajeId, string Nombre, ICollection<PersonajeSpriteDto> Sprites);

    public record PersonajeSpriteDto(
        Guid PersonajeSpriteId,
        string Nombre,
        string DireccionImagen
    );
}
