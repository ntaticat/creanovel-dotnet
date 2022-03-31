using System;
using System.Collections.Generic;

namespace WebAPI.DTOs.Novela
{
    public class NovelaNoEscenasDTO
    {
        public Guid NovelaId { get; set; }
        public string Titulo { get; set; }
        public string Descripcion { get; set; }
        public bool Disponible { get; set; }
        public Guid? UsuarioCreadorId { get; set; }
    }
}