using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace CreaNovelNETCore.Models
{
    public class Usuario : IdentityUser
    {
        public string Nombre { get; set; }
        public ICollection<Lectura> Lecturas { get; set; }
        public ICollection<Novela> NovelasCreadas { get; set; }
    }
}