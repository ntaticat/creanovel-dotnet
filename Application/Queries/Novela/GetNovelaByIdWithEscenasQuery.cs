using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Application.Dtos.Novela;
using Application.Handlers;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Queries.Novela
{
    public class GetNovelaByIdWithEscenasQuery
    {
        public class GetNovelaByIdWithEscenasQueryRequest : IRequest<NovelaWithEscenasDto> 
        {
            public Guid NovelaId { get; set; }
        }

        public class Handler : IRequestHandler<GetNovelaByIdWithEscenasQueryRequest, NovelaWithEscenasDto>
        {
            private readonly CreanovelDbContext _context;
            private readonly IMapper _mapper;

            public Handler(CreanovelDbContext context, IMapper mapper)
            {
                _context = context;
                _mapper = mapper;
            }

            public async Task<NovelaWithEscenasDto> Handle(GetNovelaByIdWithEscenasQueryRequest request, CancellationToken cancellationToken)
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
                    throw new ExceptionHandler(HttpStatusCode.NotFound, new { message = "Novela no encontrada" });
                }

                var dbNovelaDto = _mapper.Map<NovelaWithEscenasDto>(novela);
                return dbNovelaDto;
            }
        }
    }
}