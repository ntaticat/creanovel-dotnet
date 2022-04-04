using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Persistence;

namespace Application.Entities.Lectura
{
    public class Eliminar
    {
        public class Execute : IRequest
        {
            public Guid LecturaId { get; set; }
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

                var lectura = await _context.Lecturas.FindAsync(request.LecturaId);

                if (lectura == null)
                {
                    throw new Exception("Lectura no encontrada");
                }

                _context.Lecturas.Remove(lectura);

                var result = await _context.SaveChangesAsync();

                if (result > 0)
                {
                    return Unit.Value;
                }

                throw new Exception("No se pudo eliminar la lectura");            
            }
        }
    }
}