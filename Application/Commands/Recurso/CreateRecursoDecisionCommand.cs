using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Application.Handlers;
using MediatR;
using Persistence;

namespace Application.Commands.Recurso;

public class CreateRecursoDecisionCommand
{
    public class CreateRecursoDecisionCommandDto : IRequest
    {
        public Guid EscenaId { get; set; }
        public string TipoRecurso { get; set; }
        public bool PrimerRecurso { get; set; }
        public bool UltimoRecurso { get; set; }
        public string DecisionMensaje { get; set; }
    }
    
    public class Handler : IRequestHandler<CreateRecursoDecisionCommandDto>
    {
        private readonly CreanovelDbContext _context;

        public Handler(CreanovelDbContext context)
        {
            _context = context;
        }

        public async Task<Unit> Handle(CreateRecursoDecisionCommandDto request, CancellationToken cancellationToken)
        {
            var recurso = new Domain.Models.RecursoDecision{
                EscenaId = request.EscenaId,
                DecisionMensaje = request.DecisionMensaje,
                PrimerRecurso = request.PrimerRecurso,
                UltimoRecurso = request.UltimoRecurso,
                TipoRecurso = request.TipoRecurso
            };

            await _context.Recursos.AddAsync(recurso);
            var result = await _context.SaveChangesAsync();

            if (result > 0)
            {
                return Unit.Value;
            }

            throw new ExceptionHandler(HttpStatusCode.BadRequest, new { message = "No se pudo registrar el recurso decision" });
        }
    }
}