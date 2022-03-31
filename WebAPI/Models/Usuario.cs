using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace WebAPI.Models
{
    public class Usuario : IdentityUser<Guid>
    {
        public string Nombre { get; set; }
        public ICollection<Lectura> Lecturas { get; set; }
        public ICollection<Novela> NovelasCreadas { get; set; }
    }
}