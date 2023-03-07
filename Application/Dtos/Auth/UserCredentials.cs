using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Dtos.Auth
{
    public record UserCredentials
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }
}