using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Persistence;

namespace Application.Entities.Usuario
{
    public class Crear
    {
        public class Execute : IRequest
        {
            public string Nombre { get; set; }
            public string Email { get; set; }
            public string UserName { get; set; }
            public string Password { get; set; }
        }

        public class Handler : IRequestHandler<Execute>
        {
            private readonly CreanovelDbContext _context;
            private readonly IMapper _mapper;
            private readonly UserManager<Domain.Models.Usuario> _userManager;

            public Handler(CreanovelDbContext context, IMapper mapper, UserManager<Domain.Models.Usuario> userManager)
            {
                _context = context;
                _mapper = mapper;
                _userManager = userManager;
            }

            public async Task<Unit> Handle(Execute request, CancellationToken cancellationToken)
            {
                var usuario = new Domain.Models.Usuario {
                    Nombre = request.Nombre,
                    UserName = request.UserName,
                    Email = request.Email,
                };

                var createdUsuario = await _userManager.CreateAsync(usuario, request.Password);

                if (!createdUsuario.Succeeded)
                {
                    throw new Exception("No se pudo registrar el usuario");
                }
                
                return Unit.Value;
            }
        }
    }
}