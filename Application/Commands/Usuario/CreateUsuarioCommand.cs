using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Application.Handlers;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Persistence;

namespace Application.Commands.Usuario
{
    public class CreateUsuarioCommand
    {
        public class CreateUsuarioCommandDto : IRequest
        {
            public string Nombre { get; set; }
            public string Email { get; set; }
            public string UserName { get; set; }
            public string Password { get; set; }
        }

        public class Handler : IRequestHandler<CreateUsuarioCommandDto>
        {
            private readonly CreanovelDbContext _context;
            private readonly UserManager<Domain.Models.Usuario> _userManager;

            public Handler(CreanovelDbContext context, UserManager<Domain.Models.Usuario> userManager)
            {
                _context = context;
                _userManager = userManager;
            }

            public async Task<Unit> Handle(CreateUsuarioCommandDto request, CancellationToken cancellationToken)
            {
                var usuario = new Domain.Models.Usuario {
                    Nombre = request.Nombre,
                    UserName = request.UserName,
                    Email = request.Email,
                };

                var createdUsuario = await _userManager.CreateAsync(usuario, request.Password);

                if (!createdUsuario.Succeeded)
                {
                    throw new ExceptionHandler(HttpStatusCode.BadRequest, new { message = "No se pudo registrar el usuario" });
                }
                
                return Unit.Value;
            }
        }
    }
}