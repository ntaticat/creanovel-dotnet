using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Application.Dtos.NovelaVersion;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Queries.NovelaVersion
{
    public class GetNovelaVersionesQuery
    {
        public class GetNovelaVersionesQueryRequest : IRequest<List<NovelaVersionDto>> {}

        public class Handler : IRequestHandler<GetNovelaVersionesQueryRequest, List<NovelaVersionDto>>
        {
            private readonly CreanovelDbContext _context;
            private readonly IMapper _mapper;

            public Handler(CreanovelDbContext context, IMapper mapper)
            {
                _context = context;
                _mapper = mapper;
            }

            public async Task<List<NovelaVersionDto>> Handle(GetNovelaVersionesQueryRequest request, CancellationToken cancellationToken)
            {
                var dbNovelaVersiones = await _context.NovelaVersiones.ToListAsync();
                var novelaVersionesDto = _mapper.Map<List<NovelaVersionDto>>(dbNovelaVersiones);

                return novelaVersionesDto;
            }
        }

    }
}