using System;
using System.Collections.Generic;

namespace Domain.Models
{
    public class Novela
    {
        public Guid NovelaId { get; set; }
        public string Titulo { get; set; }
        public string Descripcion { get; set; }
        public bool Disponible { get; set; }
        
        public ICollection<NovelaPersonaje> Personajes { get; set; }
        public ICollection<NovelaBackground> Backgrounds { get; set; }
        
        public Guid? UsuarioCreadorId { get; set; }
        public Usuario UsuarioCreador { get; set; }

        public ICollection<NovelaVersion> NovelaVersiones { get; set; }
    }
}