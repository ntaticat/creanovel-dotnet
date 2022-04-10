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

namespace Application.Entities.Recurso
{
    public class AgregarSiguienteRecursoConversacion
    {
        public class Execute : IRequest 
        {
            public Guid RecursoId { get; set; }
            public Guid RecursoSiguienteId { get; set; }
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
                var recurso = await _context.Recursos.OfType<RecursoConversacion>().FirstOrDefaultAsync(r => r.RecursoId == request.RecursoId);
                
                if (recurso == null)
                {
                    throw new ExceptionHandler(HttpStatusCode.NotFound, new { message = "Recurso no encontrado" });
                }

                recurso.SiguienteRecursoId = request.RecursoSiguienteId;
                var result = await _context.SaveChangesAsync();

                if (result > 0)
                {
                    return Unit.Value;
                }

                throw new ExceptionHandler(HttpStatusCode.BadRequest, new { message = "No se pudo asignar el siguiente recurso al recurso correspondiente" });
            }
        }
    }
}