using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Application.Handlers;
using AutoMapper;
using MediatR;
using Persistence;

namespace Application.Commands.Novela
{
    public class DeleteNovelaCommand
    {
        public class DeleteNovelaCommandRequest : IRequest
        {
            public Guid NovelaId { get; set; }
        }

        public class Handler : IRequestHandler<DeleteNovelaCommandRequest>
        {
            private readonly CreanovelDbContext _context;

            public Handler(CreanovelDbContext context)
            {
                _context = context;
            }

            public async Task<Unit> Handle(DeleteNovelaCommandRequest request, CancellationToken cancellationToken)
            {
                var novela = await _context.Novelas.FindAsync(request.NovelaId);

                if (novela == null)
                {
                    throw new ExceptionHandler(HttpStatusCode.NotFound, new { message = "Novela no encontrada" });
                }
                
                _context.Novelas.Remove(novela);

                var result = await _context.SaveChangesAsync();

                if (result > 0)
                {
                    return Unit.Value;
                }

                throw new ExceptionHandler(HttpStatusCode.BadRequest, new { message = "No se eliminó la novela" });
            }
        }
    }
}