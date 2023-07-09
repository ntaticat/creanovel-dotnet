using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Application.Handlers;
using MediatR;
using Persistence;

namespace Application.Commands.Escena
{
    public class CreateEscenaCommand
    {
        public class CreateEscenaCommandRequest : IRequest
        {
            public string Identificador { get; set; }
            public Guid NovelaId { get; set; }
            public bool PrimerEscena { get; set; }
            public bool UltimaEscena { get; set; }
        }

        public class Handler : IRequestHandler<CreateEscenaCommandRequest>
        {
            private readonly CreanovelDbContext _context;

            public Handler(CreanovelDbContext context)
            {
                _context = context;
            }

            public async Task<Unit> Handle(CreateEscenaCommandRequest request, CancellationToken cancellationToken)
            {
                var escena = new Domain.Models.Escena {
                    Identificador = request.Identificador,
                    NovelaId = request.NovelaId,
                    PrimerEscena = request.PrimerEscena,
                    UltimaEscena = request.UltimaEscena
                };
                await this._context.Escenas.AddAsync(escena);
                var result = await _context.SaveChangesAsync();

                if (result > 0)
                {
                    return Unit.Value;
                }

                throw new ExceptionHandler(HttpStatusCode.BadRequest, new { message = "No se pudo registrar la escena" });
            }
        }
    }
}