using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using Domain;
using Persistence;
using System.Threading;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Application.Entities.Novela.Dtos;

namespace Application.Entities.Novela
{
    public class Consulta
    {
        public class ListaNovelas : IRequest<List<NovelaNoEscenasDto>> {}

        public class Handler : IRequestHandler<ListaNovelas, List<NovelaNoEscenasDto>>
        {
            private readonly CreanovelDbContext _context;
            private readonly IMapper _mapper;

            public Handler(CreanovelDbContext context, IMapper mapper)
            {
                _context = context;
                _mapper = mapper;
            }

            public async Task<List<NovelaNoEscenasDto>> Handle(ListaNovelas request, CancellationToken cancellationToken)
            {
                var dbNovelas = await _context.Novelas.ToListAsync();
                var novelasDto = _mapper.Map<List<NovelaNoEscenasDto>>(dbNovelas);
                return novelasDto;
            }
        }
  }
}