using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Application.Handlers;
using MediatR;
using Persistence;

namespace Application.Commands.Lectura
{
    public class CreateLecturaCommand
    {
        public class CreateLecturaCommandRequest : IRequest 
        {
            public Guid NovelaRegistrosId { get; set; }
            public Guid UsuarioPropietarioId { get; set; }
        }

        public class Handler : IRequestHandler<CreateLecturaCommandRequest>
        {
            private readonly CreanovelDbContext _context;

            public Handler(CreanovelDbContext context)
            {
                _context = context;
            }

            public async Task<Unit> Handle(CreateLecturaCommandRequest request, CancellationToken cancellationToken)
            {
                var lectura = new Domain.Models.Lectura {
                    NovelaRegistrosId = request.NovelaRegistrosId,
                    UsuarioPropietarioId = request.UsuarioPropietarioId
                };
                await this._context.Lecturas.AddAsync(lectura);

                var result = await _context.SaveChangesAsync();

                if (result > 0)
                {
                    return Unit.Value;
                }
                
                throw new ExceptionHandler(HttpStatusCode.BadRequest, new { message = "No se pudo registrar la lectura" });
            }
        }
    }
}