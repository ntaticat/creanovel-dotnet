using System;
using System.Collections.Generic;
using Application.Dtos.Lectura;
using Application.Dtos.Novela;

namespace Application.Dtos.Usuario
{
    public record UsuarioDto
    {
        public Guid Id { get; set; }
        public string Nombre { get; set; }
        public string Email { get; set; }
        public string UserName { get; set; }
        public ICollection<LecturaDto> Lecturas { get; set; }
        public ICollection<NovelaNoEscenasDto> NovelasCreadas { get; set; }
    }
}