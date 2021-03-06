using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Application.Entities.Novela.Dtos;
using Application.Handlers;
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
                    throw new ExceptionHandler(HttpStatusCode.NotFound, new { message = "Novela no encontrada" });
                }

                var dbNovelaDto = _mapper.Map<NovelaNoEscenasDto>(novela);
                return dbNovelaDto;
            }
        }
    }
}