using System;
using System.Collections.Generic;
using CreaNovelNETCore.DTOs.Escena;
using CreaNovelNETCore.Models;

namespace CreaNovelNETCore.DTOs
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