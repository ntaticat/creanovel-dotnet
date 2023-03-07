using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Application.Dtos.Recurso;
using Application.Handlers;
using AutoMapper;
using Domain.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Queries.Recurso
{
    public class GetRecursoByIdQuery
    {
        public class GetRecursoByIdQueryDto : IRequest<RecursoDto> {
            public Guid RecursoId { get; set; }
        }

        public class Handler : IRequestHandler<GetRecursoByIdQueryDto, RecursoDto>
        {
            private readonly CreanovelDbContext _context;
            private readonly IMapper _mapper;

            public Handler(CreanovelDbContext context, IMapper mapper)
            {
                _context = context;
                _mapper = mapper;
            }

            public async Task<RecursoDto> Handle(GetRecursoByIdQueryDto request, CancellationToken cancellationToken)
            {
                
                var recurso = await _context.Recursos.FindAsync(request.RecursoId);

                if (recurso == null)
                {
                    throw new ExceptionHandler(HttpStatusCode.NotFound, new { message = "Recurso no encontrado" });
                }

                var type = recurso.GetType();

                var isRecursoConversacion = type == typeof(RecursoConversacion);
                var isRecursoDecision = type == typeof(RecursoDecision);

                if(isRecursoDecision)
                {
                    var nuevo = await _context.RecursosDecision.Include(r => r.Opciones).FirstOrDefaultAsync(r => r.RecursoId == request.RecursoId);
                    var opciones = nuevo.Opciones;
                }
                
                if (isRecursoConversacion)
                {
                    return _mapper.Map<RecursoConversacionDto>(recurso);
                }
                if (isRecursoDecision)
                {
                    return _mapper.Map<RecursoDecisionDto>(recurso);
                }

                return _mapper.Map<RecursoDto>(recurso);
            }
        }
    }
}