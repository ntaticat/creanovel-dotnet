using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Persistence;

namespace Application.Entities.Escena
{
    public class Crear
    {
        public class Execute : IRequest
        {
            public string Identificador { get; set; }
            public Guid NovelaId { get; set; }
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
                var escena = new Domain.Models.Escena {
                    Identificador = request.Identificador,
                    NovelaId = request.NovelaId
                };
                await this._context.Escenas.AddAsync(escena);
                var result = await _context.SaveChangesAsync();

                if (result > 0)
                {
                    return Unit.Value;
                }

                throw new Exception("No se pudo registrar la escena");            
            }
        }
    }
}