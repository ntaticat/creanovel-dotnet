using System;
using System.Collections.Generic;
using CreaNovelNETCore.DTOs.LecturaRecursos;
using CreaNovelNETCore.Models;

namespace CreaNovelNETCore.DTOs.Lectura
{
    public class LecturaDto
    {
        public Guid LecturaId { get; set; }
        public Guid NovelaRegistrosId { get; set; }
        public Guid UsuarioPropietarioId { get; set; }
        public ICollection<LecturaRecursosDto> Recursos { get; set; }
    }
}