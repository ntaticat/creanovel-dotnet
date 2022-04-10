using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Application.Handlers;
using MediatR;
using Persistence;

namespace Application.Entities.Lectura
{
    public class Crear
    {
        public class Execute : IRequest 
        {
            public Guid NovelaRegistrosId { get; set; }
            public Guid UsuarioPropietarioId { get; set; }
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