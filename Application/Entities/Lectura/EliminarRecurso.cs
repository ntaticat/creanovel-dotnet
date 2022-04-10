using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Application.Handlers;
using AutoMapper;
using MediatR;
using Persistence;

namespace Application.Entities.Lectura
{
    public class EliminarRecurso
    {
        public class Execute : IRequest
        {
            public Guid LecturaId { get; set; }
            public Guid RecursoId { get; set; }
        }

        public class Handler : IRequestHandler<Execute>
        {
            private readonly CreanovelDbContext _context;
            private readonly IMapper _mapper;

            public Handler(CreanovelDbContext context, IMapper mapper)
            {
                _context = context;
                _mapper = mapper;
            }

            public async Task<Unit> Handle(Execute request, CancellationToken cancellationToken)
            {

                var lecturaRecurso = await _context.LecturaRecurso.FindAsync(request.LecturaId, request.RecursoId);

                if (lecturaRecurso == null)
                {
                    throw new ExceptionHandler(HttpStatusCode.NotFound, new { message = "Lectura no encontrada" });
                }

                _context.LecturaRecurso.Remove(lecturaRecurso);

                var result = await _context.SaveChangesAsync();

                if (result > 0)
                {
                    return Unit.Value;
                }

                throw new ExceptionHandler(HttpStatusCode.BadRequest, new { message = "No se pudo eliminar la relación de Lectura y Recurso" });
            }
        }
    }
}