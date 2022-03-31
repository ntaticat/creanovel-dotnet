using System;
using System.Collections.Generic;
using WebAPI.DTOs.LecturaRecursos;
using WebAPI.Models;

namespace WebAPI.DTOs.Lectura
{
    public class LecturaDto
    {
        public Guid LecturaId { get; set; }
        public Guid NovelaRegistrosId { get; set; }
        public Guid UsuarioPropietarioId { get; set; }
        public ICollection<LecturaRecursosDto> Recursos { get; set; }
    }
}