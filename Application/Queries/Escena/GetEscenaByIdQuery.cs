using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Application.Dtos.Escena;
using Application.Handlers;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Queries.Escena
{
    public class GetEscenaByIdQuery
    {
        public class GetEscenaByIdQueryRequest : IRequest<EscenaRecursosDto> {
            public Guid EscenaId { get; set; }
        }

        public class Handler : IRequestHandler<GetEscenaByIdQueryRequest, EscenaRecursosDto>
        {
            private readonly CreanovelDbContext _context;
            private readonly IMapper _mapper;

            public Handler(CreanovelDbContext context, IMapper mapper)
            {
                _context = context;
                _mapper = mapper;
            }

            public async Task<EscenaRecursosDto> Handle(GetEscenaByIdQueryRequest request, CancellationToken cancellationToken)
            {
                var escena = await _context.Escenas.Include(escena => escena.Recursos).FirstOrDefaultAsync(escena => escena.EscenaId == request.EscenaId);
                
                if (escena == null)
                {
                    throw new ExceptionHandler(HttpStatusCode.NotFound, new { message = "Escena no encontrada" });
                }
                
                var escenaDto = _mapper.Map<EscenaRecursosDto>(escena);
                return escenaDto;
            }
        }
    }
}