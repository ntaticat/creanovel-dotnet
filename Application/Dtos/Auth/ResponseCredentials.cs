using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Dtos.Auth
{
    public record ResponseCredentials
    {
        public string Token { get; set; }
    }
}