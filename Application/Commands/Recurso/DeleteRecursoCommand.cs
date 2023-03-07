using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Application.Handlers;
using MediatR;
using Persistence;

namespace Application.Commands.Recurso
{
    public class DeleteRecursoCommand
    {
        public class DeleteRecursoCommandDto : IRequest
        {
            public Guid RecursoId { get; set; }
        }

        public class Handler : IRequestHandler<DeleteRecursoCommandDto>
        {
            private readonly CreanovelDbContext _context;

            public Handler(CreanovelDbContext context)
            {
                _context = context;
            }

            public async Task<Unit> Handle(DeleteRecursoCommandDto request, CancellationToken cancellationToken)
            {
                var recurso = await _context.Recursos.FindAsync(request.RecursoId);

                if (recurso == null)
                {
                    throw new ExceptionHandler(HttpStatusCode.NotFound, new { message = "Recurso no encontrado" });
                }
                
                _context.Recursos.Remove(recurso);

                var result = await _context.SaveChangesAsync();

                if (result > 0)
                {
                    return Unit.Value;
                }

                throw new ExceptionHandler(HttpStatusCode.NotFound, new { message = "No se eliminó el recurso" });
            }
        }
    }
}