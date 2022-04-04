using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Entities.Lectura
{
    public class RegistrarRecurso
    {
        public class Execute : IRequest
        {
            public Guid LecturaId { get; set; }
            public Guid RecursoId { get; set; }
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
                var lecturaRecurso = new Domain.Models.LecturaRecursos {
                    LecturaId = request.LecturaId,
                    RecursoId = request.RecursoId
                };
                
                // lecturaRecursos.Lectura = await _context.Lecturas.FindAsync(lecturaRecursos.LecturaId);
                // lecturaRecursos.Recurso = await _context.Recursos.FindAsync(lecturaRecursos.RecursoId);
                await _context.LecturaRecurso.AddAsync(lecturaRecurso);

                var result = await _context.SaveChangesAsync();

                if (result > 0)
                {
                    return Unit.Value;
                }

                throw new Exception("No se pudo registrar la relación de Lectura y Recurso");            
            }
        }
    }
}