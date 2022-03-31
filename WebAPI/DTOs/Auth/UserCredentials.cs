using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebAPI.DTOs.Auth
{
    public class UserCredentials
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }
}