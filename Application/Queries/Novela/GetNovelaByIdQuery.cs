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
    public class GetNovelaByIdQuery
    {
        public class GetNovelaByIdQueryRequest : IRequest<NovelaPopulatedDto> 
        {
            public Guid NovelaId { get; set; }
            public bool HasVersiones { get; set; }
            public bool HasPersonajes { get; set; }
            public bool HasBackgrounds { get; set; }
        }

        public class Handler : IRequestHandler<GetNovelaByIdQueryRequest, NovelaPopulatedDto>
        {
            private readonly CreanovelDbContext _context;
            private readonly IMapper _mapper;

            public Handler(CreanovelDbContext context, IMapper mapper)
            {
                _context = context;
                _mapper = mapper;
            }

            public async Task<NovelaPopulatedDto> Handle(GetNovelaByIdQueryRequest request, CancellationToken cancellationToken)
            {
                var novela = await _context.Novelas.FindAsync(request.NovelaId);

                if (novela == null)
                {
                    throw new ExceptionHandler(HttpStatusCode.NotFound, new { message = "Novela no encontrada" });
                }

                if (request.HasVersiones)
                {
                    await _context.Entry(novela)
                    .Collection(n => n.NovelaVersiones)
                    .LoadAsync();
                }

                if (request.HasPersonajes)
                {
                    await _context.Entry(novela)
                    .Collection(n => n.Personajes)
                    .Query()
                    .Include(np => np.Personaje)
                    .LoadAsync();
                }

                if (request.HasBackgrounds)
                {
                    await _context.Entry(novela)
                    .Collection(n => n.Backgrounds)
                    .LoadAsync();
                }

                var dbNovelaDto = _mapper.Map<NovelaPopulatedDto>(novela);
                return dbNovelaDto;
            }
        }
    }
}