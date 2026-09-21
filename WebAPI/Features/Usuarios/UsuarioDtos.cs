using System;
using System.Collections.Generic;
using System.Text.Json;

namespace WebAPI.Features.Usuarios
{
    public record UsuarioDto(
        Guid Id,
        string Nombre,
        string Email,
        string UserName,
        ICollection<LecturaDto> Lecturas,
        ICollection<NovelaResumenDto> NovelasCreadas
    );

    public record LecturaDto(
        Guid LecturaId, Guid NovelaRegistrosId, Guid UsuarioPropietarioId, ICollection<LecturaRecursoDto> Recursos,
        JsonElement? Estado, Guid? RecursoActualId, Guid? NovelaVersionId
    );

    public record LecturaRecursoDto(Guid LecturaId, Guid RecursoId, int RecursoOrder);

    public record NovelaResumenDto(Guid NovelaId, string Titulo, string Descripcion, bool Disponible, string PortadaImagenUrl, Guid? UsuarioCreadorId, int VersionesCount);
}
