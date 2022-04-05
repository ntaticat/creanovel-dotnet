using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Application.Entities.Novela.Dtos;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Entities.Novela
{
    public class ConsultaIdEscenas
    {
        public class NovelaUnica : IRequest<NovelaWithEscenasDto> 
        {
            public Guid NovelaId { get; set; }
        }

        public class Handler : IRequestHandler<NovelaUnica, NovelaWithEscenasDto>
        {
            private readonly CreanovelDbContext _context;
            private readonly IMapper _mapper;

            public Handler(CreanovelDbContext context, IMapper mapper)
            {
                _context = context;
                _mapper = mapper;
            }

            public async Task<NovelaWithEscenasDto> Handle(NovelaUnica request, CancellationToken cancellationToken)
            {
                var novela = await _context.Novelas
                    .Include(n => n.Escenas)
                    .AsSplitQuery()
                    .Include(n => n.Personajes)
                    .ThenInclude(n => n.Personaje)
                    .AsSplitQuery()
                    .FirstOrDefaultAsync(novela => novela.NovelaId == request.NovelaId);

                if (novela == null)
                {
                    throw new Exception("Novela no encontrada");
                }

                var dbNovelaDto = _mapper.Map<NovelaWithEscenasDto>(novela);
                return dbNovelaDto;
            }
        }
    }
}