using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Application.Handlers;
using MediatR;
using Persistence;

namespace Application.Commands.Escena
{
    public class UpdateEscenaCommand
    {
        public class UpdateEscenaCommandRequest : IRequest
        {
            public Guid EscenaId { get; set; }
            public string Identificador { get; set; }
            public Guid? NovelaVersionId { get; set; }
            public bool? PrimerEscena { get; set; }
            public bool? UltimaEscena { get; set; }
        }

        public class Handler : IRequestHandler<UpdateEscenaCommandRequest>
        {
            private readonly CreanovelDbContext _context;

            public Handler(CreanovelDbContext context)
            {
                _context = context;
            }

            public async Task<Unit> Handle(UpdateEscenaCommandRequest request, CancellationToken cancellationToken)
            {
                var escena = await _context.Escenas.FindAsync(request.EscenaId);

                if (escena == null)
                {
                    throw new ExceptionHandler(HttpStatusCode.NotFound, new { message = "Escena no encontrada" });
                }

                escena.Identificador = request.Identificador ?? escena.Identificador;
                escena.NovelaVersionId = request.NovelaVersionId ?? escena.NovelaVersionId;
                escena.PrimerEscena = request.PrimerEscena ?? escena.PrimerEscena;
                escena.UltimaEscena = request.UltimaEscena ?? escena.UltimaEscena;

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