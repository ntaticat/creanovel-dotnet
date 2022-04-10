using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Application.Entities.Usuario.Dtos;
using Application.Handlers;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Entities.Usuario
{
    public class ConsultaId
    {
        public class UsuarioUnico : IRequest<UsuarioDto> 
        {
            public Guid UsuarioId { get; set; }
        }

        public class Handler : IRequestHandler<UsuarioUnico, UsuarioDto>
        {
            private readonly CreanovelDbContext _context;
            private readonly IMapper _mapper;

            public Handler(CreanovelDbContext context, IMapper mapper)
            {
                _context = context;
                _mapper = mapper;
            }

            public async Task<UsuarioDto> Handle(UsuarioUnico request, CancellationToken cancellationToken)
            {
                var usuario = await _context.Usuarios
                    .Include(u => u.Lecturas)
                    .ThenInclude(l => l.Recursos)
                    .AsSplitQuery()
                    .Include(u => u.NovelasCreadas)
                    .AsSplitQuery()
                    .FirstOrDefaultAsync(u => u.Id == request.UsuarioId);
                
                if (usuario == null)
                {
                    throw new ExceptionHandler(HttpStatusCode.NotFound, new { message = "Usuario no encontrado" });
                }
                
                var dbUsuarioDto = _mapper.Map<UsuarioDto>(usuario);
                
                return dbUsuarioDto;
            }
        }
    }
}