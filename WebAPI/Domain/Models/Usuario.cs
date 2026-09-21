using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace Domain.Models
{
    public class Usuario : IdentityUser<Guid>
    {
        public string Nombre { get; set; }
        public ICollection<Lectura> Lecturas { get; set; }
        public ICollection<Novela> NovelasCreadas { get; set; }
    }
}