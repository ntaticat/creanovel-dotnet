using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Application.Handlers;
using MediatR;
using Persistence;

namespace Application.Entities.Escena
{
    public class Editar
    {
        public class Execute : IRequest
        {
            public Guid EscenaId { get; set; }
            public string Identificador { get; set; }
            public Guid? NovelaId { get; set; }
        }

        public class Handler : IRequestHandler<Execute>
        {
            private readonly CreanovelDbContext _context;

            public Handler(CreanovelDbContext context)
            {
                _context = context;
            }

            public async Task<Unit> Handle(Execute request, CancellationToken cancellationToken)
            {
                var escena = await _context.Escenas.FindAsync(request.EscenaId);

                if (escena == null)
                {
                    throw new ExceptionHandler(HttpStatusCode.NotFound, new { message = "Escena no encontrada" });
                }

                escena.Identificador = request.Identificador ?? escena.Identificador;
                escena.NovelaId = request.NovelaId ?? escena.NovelaId;

                var result = await _context.SaveChangesAsync();

                if (result > 0)
                {
                    return Unit.Value;
                }

                throw new ExceptionHandler(HttpStatusCode.BadRequest, new { message = "No se actualizó la escena" });
            }
        }
    }
}