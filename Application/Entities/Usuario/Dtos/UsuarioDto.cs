using System;
using System.Collections.Generic;
using Application.Entities.Lectura.Dtos;
using Application.Entities.Novela.Dtos;

namespace Application.Entities.Usuario.Dtos
{
    public class UsuarioDto
    {
        public Guid Id { get; set; }
        public string Nombre { get; set; }
        public string Email { get; set; }
        public string UserName { get; set; }
        public ICollection<LecturaDto> Lecturas { get; set; }
        public ICollection<NovelaNoEscenasDto> NovelasCreadas { get; set; }
    }
}