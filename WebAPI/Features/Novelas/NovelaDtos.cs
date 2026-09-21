using System;
using System.Collections.Generic;
using WebAPI.Features.Backgrounds;
using WebAPI.Features.Personajes;

namespace WebAPI.Features.Novelas
{
    public record NovelaListItemDto(Guid NovelaId, string Titulo, string Descripcion, bool Disponible, string PortadaImagenUrl, Guid? UsuarioCreadorId);

    public record NovelaVersionResumenDto(Guid NovelaVersionId, string NumeroVersion, bool Disponible, bool EsBorrador, Guid NovelaId);

    public record NovelaPopulatedDto(
        Guid NovelaId,
        string Titulo,
        string Descripcion,
        bool Disponible,
        string PortadaImagenUrl,
        Guid? UsuarioCreadorId,
        string UsuarioCreadorNombre,
        ICollection<NovelaVersionResumenDto> Versiones,
        ICollection<PersonajeDto> Personajes,
        ICollection<BackgroundDto> Backgrounds
    );
}
