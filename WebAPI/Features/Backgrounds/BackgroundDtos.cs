using System;
using System.Collections.Generic;

namespace WebAPI.Features.Backgrounds
{
    public record BackgroundDto(Guid BackgroundId, string Descripcion, ICollection<BackgroundSpriteDto> Sprites);

    public record BackgroundSpriteDto(Guid BackgroundSpriteId, string Nombre, string DireccionImagen);
}
