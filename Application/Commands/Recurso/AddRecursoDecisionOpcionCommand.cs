using System;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Application.Handlers;
using Domain.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Commands.Recurso
{
    public class AddRecursoDecisionOpcionCommand
    {
        public class AddRecursoDecisionOpcionCommandDto : IRequest 
        {
            public string OpcionMensaje { get; set; }
            public Guid? SiguienteRecursoId { get; set; }
            public Guid RecursoDecisionId { get; set; }
        }

        public class Handler : IRequestHandler<AddRecursoDecisionOpcionCommandDto>
        {
            private readonly CreanovelDbContext _context;

            public Handler(CreanovelDbContext context)
            {
                _context = context;
            }

            public async Task<Unit> Handle(AddRecursoDecisionOpcionCommandDto request, CancellationToken cancellationToken)
            {
                var recurso = await _context.Recursos.OfType<RecursoDecision>().FirstOrDefaultAsync(r => r.RecursoId == request.RecursoDecisionId);
                
                if (recurso == null)
                {
                    throw new ExceptionHandler(HttpStatusCode.NotFound, new { message = "El recurso de la opción no fue encontrado" });
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

                throw new ExceptionHandler(HttpStatusCode.BadRequest, new { message = "No se pudo agregar la opción al recurso correspondiente" });
            }
        }
    }
}