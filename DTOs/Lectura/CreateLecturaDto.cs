using System;

namespace CreaNovelNETCore.DTOs.Lectura
{
    public class CreateLecturaDto
    {
        public Guid NovelaRegistrosId { get; set; }
        public Guid UsuarioPropietarioId { get; set; }
    }
}