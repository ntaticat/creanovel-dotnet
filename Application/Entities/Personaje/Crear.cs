using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Application.Handlers;
using MediatR;
using Persistence;

namespace Application.Entities.Personaje
{
    public class Crear
    {
        public class Execute : IRequest 
        {
            public string Nombre { get; set; }
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
                var personaje = new Domain.Models.Personaje {
                    Nombre = request.Nombre
                };
                await this._context.Personajes.AddAsync(personaje);
                var result = await _context.SaveChangesAsync();

                if (result > 0)
                {
                    return Unit.Value;
                }

                throw new ExceptionHandler(HttpStatusCode.BadRequest, new { message = "No se pudo registrar el personaje" });
            }
        }
    }
}