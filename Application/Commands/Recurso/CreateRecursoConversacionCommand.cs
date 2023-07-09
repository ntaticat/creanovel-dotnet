using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Application.Handlers;
using MediatR;
using Persistence;

namespace Application.Commands.Recurso;

public class CreateRecursoConversacionCommand
{
    public class CreateRecursoConversacionCommandRequest : IRequest
    {
        public Guid EscenaId { get; set; }
        public string TipoRecurso { get; set; }
        public bool PrimerRecurso { get; set; }
        public bool UltimoRecurso { get; set; }
        public string Mensaje { get; set; }
        public Guid? SiguienteRecursoId { get; set; }
    }
    
    public class Handler : IRequestHandler<CreateRecursoConversacionCommandRequest>
        {
            private readonly CreanovelDbContext _context;

            public Handler(CreanovelDbContext context)
            {
                _context = context;
            }

            public async Task<Unit> Handle(CreateRecursoConversacionCommandRequest request, CancellationToken cancellationToken)
            {
                var recurso = new Domain.Models.RecursoConversacion{
                    EscenaId = request.EscenaId,
                    Mensaje = request.Mensaje,
                    PrimerRecurso = request.PrimerRecurso,
                    UltimoRecurso = request.UltimoRecurso,
                    TipoRecurso = request.TipoRecurso,
                    SiguienteRecursoId = request.SiguienteRecursoId ?? null
                };

                await _context.Recursos.AddAsync(recurso);
                var result = await _context.SaveChangesAsync();

                if (result > 0)
                {
                    return Unit.Value;
                }

                throw new ExceptionHandler(HttpStatusCode.BadRequest, new { message = "No se pudo registrar el recurso conversacion" });
            }
        }
}