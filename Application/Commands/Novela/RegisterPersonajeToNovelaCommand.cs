using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Application.Handlers;
using MediatR;
using Persistence;

namespace Application.Commands.Novela
{
    public class RegisterPersonajeToNovelaCommand
    {
        public class RegisterPersonajeToNovelaCommandDto : IRequest
        {
            public Guid NovelaId { get; set; }
            public Guid PersonajeId { get; set; }
        }

        public class Handler : IRequestHandler<RegisterPersonajeToNovelaCommandDto>
        {
            private readonly CreanovelDbContext _context;

            public Handler(CreanovelDbContext context)
            {
                _context = context;
            }

            public async Task<Unit> Handle(RegisterPersonajeToNovelaCommandDto request, CancellationToken cancellationToken)
            {
                var novelaPersonaje = new Domain.Models.NovelaPersonaje {
                    NovelaId = request.NovelaId,
                    PersonajeId = request.PersonajeId
                };
                await _context.NovelaPersonaje.AddAsync(novelaPersonaje);

                var result = await _context.SaveChangesAsync();

                if (result > 0)
                {
                    return Unit.Value;
                }

                throw new ExceptionHandler(HttpStatusCode.BadRequest, new { message = "No se pudo registrar la relación de Novela y Personaje" });
            }
        }
    }
}