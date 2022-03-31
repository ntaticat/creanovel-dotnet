using System;
using System.Collections.Generic;
using WebAPI.DTOs.Lectura;
using WebAPI.DTOs.Novela;

namespace WebAPI.DTOs.Usuario
{
    public class UsuarioDto
    {
        public Guid Id { get; set; }
        public string Nombre { get; set; }
        public string Email { get; set; }
        public string UserName { get; set; }
        public ICollection<LecturaDto> Lecturas { get; set; }
        public ICollection<NovelaNoEscenasDTO> NovelasCreadas { get; set; }
    }
}