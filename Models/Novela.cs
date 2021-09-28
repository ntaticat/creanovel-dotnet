using System;
using System.Collections.Generic;

namespace CreaNovelNETCore.Models
{
    public class Novela
    {
        public Guid NovelaId { get; set; }
        public string Titulo { get; set; }
        public string Descripcion { get; set; }
        public bool Disponible { get; set; }
        
        public ICollection<Escena> Escenas { get; set; }
        
        public Guid? UsuarioCreadorId { get; set; }
        public Usuario UsuarioCreador { get; set; }
    }
}