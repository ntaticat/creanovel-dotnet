using System;
using System.Collections.Generic;
using WebAPI.DTOs.Escena;
using WebAPI.Models;

namespace WebAPI.DTOs.Novela
{
    public class NovelaWithEscenasDTO
    {
        public Guid NovelaId { get; set; }
        public string Titulo { get; set; }
        public string Descripcion { get; set; }
        public bool Disponible { get; set; }
        public Guid? UsuarioCreadorId { get; set; }
        public ICollection<EscenaDTO> Escenas { get; set; }
    }
}