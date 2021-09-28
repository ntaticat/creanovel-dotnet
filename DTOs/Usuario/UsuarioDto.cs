using System;
using System.Collections.Generic;
using CreaNovelNETCore.DTOs.Lectura;

namespace CreaNovelNETCore.DTOs.Usuario
{
    public class UsuarioDto
    {
        public Guid UsuarioId { get; set; }
        public string Nombre { get; set; }
        public string Correo { get; set; }
        public string Nickname { get; set; }
        public ICollection<LecturaDto> Lecturas { get; set; }
        public ICollection<NovelaNoEscenasDTO> NovelasCreadas { get; set; }
    }
}