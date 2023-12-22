using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Application.Dtos.NovelaVersion;
using Application.Handlers;
using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Persistence;

namespace Application.Queries.NovelaVersion
{
    public class GetNovelaVersionByIdQuery
    {
        public record GetNovelaVersionByIdQueryRequest : IRequest<NovelaVersionPopulatedDto>  
        {
            public Guid NovelaVersionId { get; set; }
        }

        public class Handler : IRequestHandler<GetNovelaVersionByIdQueryRequest, NovelaVersionPopulatedDto>
        {
            private readonly CreanovelDbContext _context;
            private IMapper _mapper;

            public Handler(CreanovelDbContext context, IMapper mapper)
            {
                _context = context;
                _mapper = mapper;
            }
            
            public async Task<NovelaVersionPopulatedDto> Handle(GetNovelaVersionByIdQueryRequest request, CancellationToken cancellationToken)
            {
                var novelaVersion = await _context.NovelaVersiones.FindAsync(request.NovelaVersionId);

                if (novelaVersion == null)
                {
                    throw new ExceptionHandler(HttpStatusCode.NotFound, new { message = "NovelaVersion no encontrada" });
                }

                await _context.Entry(novelaVersion)
                .Collection(nv => nv.Escenas)
                .LoadAsync();

                var novelaVersionDto = _mapper.Map<NovelaVersionPopulatedDto>(novelaVersion);
                return novelaVersionDto;
            }
        }
    }
}