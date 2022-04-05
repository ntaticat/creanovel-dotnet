using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Persistence;

namespace Application.Entities.Novela
{
    public class RegistrarPersonaje
    {
        public class Execute : IRequest
        {
            public Guid NovelaId { get; set; }
            public Guid PersonajeId { get; set; }
        }

        public class Handler : IRequestHandler<Execute>
        {
            private readonly CreanovelDbContext _context;
            private readonly IMapper _mapper;

            public Handler(CreanovelDbContext context, IMapper mapper)
            {
                _context = context;
                _mapper = mapper;
            }

            public async Task<Unit> Handle(Execute request, CancellationToken cancellationToken)
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

                throw new Exception("No se pudo registrar la relación de Novela y Personaje");            
            }
        }
    }
}