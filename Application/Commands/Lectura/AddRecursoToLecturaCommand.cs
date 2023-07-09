using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Application.Handlers;
using MediatR;
using Persistence;

namespace Application.Commands.Lectura
{
    public class AddRecursoToLecturaCommand
    {
        public class AddRecursoToLecturaCommandRequest : IRequest
        {
            public Guid LecturaId { get; set; }
            public Guid RecursoId { get; set; }
            public int RecursoOrder { get; set; }
        }

        public class Handler : IRequestHandler<AddRecursoToLecturaCommandRequest>
        {
            private readonly CreanovelDbContext _context;

            public Handler(CreanovelDbContext context)
            {
                _context = context;
            }

            public async Task<Unit> Handle(AddRecursoToLecturaCommandRequest request, CancellationToken cancellationToken)
            {
                var lecturaRecurso = new Domain.Models.LecturaRecursos {
                    LecturaId = request.LecturaId,
                    RecursoId = request.RecursoId,
                    RecursoOrder = request.RecursoOrder
                };
                
                await _context.LecturaRecurso.AddAsync(lecturaRecurso);

                var result = await _context.SaveChangesAsync();

                if (result > 0)
                {
                    return Unit.Value;
                }
                
                throw new ExceptionHandler(HttpStatusCode.BadRequest, new { message = "No se pudo registrar la relación de Lectura y Recurso" });
            }
        }
    }
}