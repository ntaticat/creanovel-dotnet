using System;
using System.Collections.Generic;

namespace Application.Entities.Usuario.Dtos
{
    public class CreateUsuarioDto
    {
        public string Nombre { get; set; }
        public string Email { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
    }
}