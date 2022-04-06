using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Entities.Recurso
{
    public class AgregarOpcionDecision
    {
        public class Execute : IRequest 
        {
            public string OpcionMensaje { get; set; }
            public Guid? SiguienteRecursoId { get; set; }
            public Guid RecursoDecisionId { get; set; }
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
                var recurso = await _context.Recursos.OfType<RecursoDecision>().FirstOrDefaultAsync(r => r.RecursoId == request.RecursoDecisionId);
                
                if (recurso == null)
                {
                    throw new Exception("El recurso de la opción no fue encontrado");
                }

                var opcion = new RecursoDecisionOpcion{
                    RecursoDecisionId = request.RecursoDecisionId,
                    SiguienteRecursoId = request.SiguienteRecursoId ?? null,
                    OpcionMensaje = request.OpcionMensaje
                };

                await _context.RecursoDecisionOpciones.AddAsync(opcion);

                var secondResult = await _context.SaveChangesAsync();

                if (secondResult > 0)
                {
                    return Unit.Value;
                }

                throw new Exception("No se pudo agregar la opción al recurso correspondiente");
            }
        }
    }
}