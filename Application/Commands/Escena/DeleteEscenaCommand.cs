using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Application.Handlers;
using MediatR;
using Persistence;

namespace Application.Commands.Escena
{
    public class DeleteEscenaCommand
    {
        public class DeleteEscenaCommandDto : IRequest
        {
            public Guid EscenaId { get; set; }
        }

        public class Handler : IRequestHandler<DeleteEscenaCommandDto>
        {
            private readonly CreanovelDbContext _context;

            public Handler(CreanovelDbContext context)
            {
                _context = context;
            }

            public async Task<Unit> Handle(DeleteEscenaCommandDto request, CancellationToken cancellationToken)
            {
                var escena = await _context.Escenas.FindAsync(request.EscenaId);

                if (escena == null)
                {
                    throw new ExceptionHandler(HttpStatusCode.NotFound, new { message = "Escena no encontrada" });
                }
                
                _context.Escenas.Remove(escena);

                var result = await _context.SaveChangesAsync();

                if (result > 0)
                {
                    return Unit.Value;
                }

                throw new ExceptionHandler(HttpStatusCode.BadRequest, new { message = "No se eliminó la escena" });
            }
        }
    }
}