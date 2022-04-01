using System;

namespace Application.Entities.Lectura.Dtos
{
    public class CreateLecturaDto
    {
        public Guid NovelaRegistrosId { get; set; }
        public Guid UsuarioPropietarioId { get; set; }
    }
}