using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Application.Dtos.Usuario;
using Application.Handlers;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Queries.Usuario
{
    public class GetUsuarioByIdQuery
    {
        public class GetUsuarioByIdQueryRequest : IRequest<UsuarioDto> 
        {
            public Guid UsuarioId { get; set; }
        }

        public class Handler : IRequestHandler<GetUsuarioByIdQueryRequest, UsuarioDto>
        {
            private readonly CreanovelDbContext _context;
            private readonly IMapper _mapper;

            public Handler(CreanovelDbContext context, IMapper mapper)
            {
                _context = context;
                _mapper = mapper;
            }

            public async Task<UsuarioDto> Handle(GetUsuarioByIdQueryRequest request, CancellationToken cancellationToken)
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