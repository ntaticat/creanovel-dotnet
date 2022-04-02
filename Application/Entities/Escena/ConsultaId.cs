using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Application.Entities.Escena.Dtos;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Entities.Escena
{
    public class ConsultaId
    {
        public class EscenaUnica : IRequest<EscenaRecursosDto> {
            public Guid EscenaId { get; set; }
        }

        public class Handler : IRequestHandler<EscenaUnica, EscenaRecursosDto>
        {
            private readonly CreanovelDbContext _context;
            private readonly IMapper _mapper;

            public Handler(CreanovelDbContext context, IMapper mapper)
            {
                _context = context;
                _mapper = mapper;
            }

            public async Task<EscenaRecursosDto> Handle(EscenaUnica request, CancellationToken cancellationToken)
            {
                var escena = await _context.Escenas.Include(escena => escena.Recursos).FirstOrDefaultAsync(escena => escena.EscenaId == request.EscenaId);
                
                if (escena == null)
                {
                    throw new Exception("Escena no encontrada");
                }
                
                var escenaDto = _mapper.Map<EscenaRecursosDto>(escena);
                return escenaDto;
            }
        }
    }
}