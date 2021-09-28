using System;
using System.Collections.Generic;

namespace CreaNovelNETCore.Models
{
    public class Usuario
    {
        public Guid UsuarioId { get; set; }
        public string Nombre { get; set; }
        public string Correo { get; set; }
        public string Nickname { get; set; }
        public string Password { get; set; }
        public ICollection<Lectura> Lecturas { get; set; }
        public ICollection<Novela> NovelasCreadas { get; set; }
    }
}