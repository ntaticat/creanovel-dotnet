using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Application.Handlers;
using MediatR;
using Persistence;

namespace Application.Commands.Lectura
{
    public class DeleteLecturaCommand
    {
        public class DeleteLecturaCommandRequest : IRequest
        {
            public Guid LecturaId { get; set; }
        }

        public class Handler : IRequestHandler<DeleteLecturaCommandRequest>
        {
            private readonly CreanovelDbContext _context;

            public Handler(CreanovelDbContext context)
            {
                _context = context;
            }

            public async Task<Unit> Handle(DeleteLecturaCommandRequest request, CancellationToken cancellationToken)
            {

                var lectura = await _context.Lecturas.FindAsync(request.LecturaId);

                if (lectura == null)
                {
                    throw new ExceptionHandler(HttpStatusCode.NotFound, new { message = "Lectura no encontrada" });
                }

                _context.Lecturas.Remove(lectura);

                var result = await _context.SaveChangesAsync();

                if (result > 0)
                {
                    return Unit.Value;
                }

                throw new ExceptionHandler(HttpStatusCode.BadRequest, new { message = "No se pudo eliminar la lectura" });
            }
        }
    }
}