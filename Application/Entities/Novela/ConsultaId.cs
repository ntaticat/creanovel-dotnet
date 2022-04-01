using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Application.Entities.Novela.Dtos;
using AutoMapper;
using MediatR;
using Persistence;

namespace Application.Entities.Novela
{
    public class ConsultaId
    {
        public class NovelaUnica : IRequest<NovelaNoEscenasDto> 
        {
            public Guid NovelaId { get; set; }
        }

        public class Handler : IRequestHandler<NovelaUnica, NovelaNoEscenasDto>
        {
            private readonly CreanovelDbContext _context;
            private readonly IMapper _mapper;

            public Handler(CreanovelDbContext context, IMapper mapper)
            {
                _context = context;
                _mapper = mapper;
            }

            public async Task<NovelaNoEscenasDto> Handle(NovelaUnica request, CancellationToken cancellationToken)
            {
                var novela = await _context.Novelas.FindAsync(request.NovelaId);

                if (novela == null)
                {
                    throw new Exception("Novela no encontrada");
                }

                var dbNovelaDto = _mapper.Map<NovelaNoEscenasDto>(novela);
                return dbNovelaDto;
            }
        }
    }
}