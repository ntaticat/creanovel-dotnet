using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Application.Handlers;
using MediatR;
using Persistence;

namespace Application.Commands.NovelaVersion
{
    public class CreateNovelaVersionCommand
    {
        public class CreateNovelaVersionCommandRequest : IRequest
        {
            public string NumeroVersion { get; set; }
            public Guid NovelaId { get; set; }
            public bool? Disponible { get; set; }
        }

        public class Handler : IRequestHandler<CreateNovelaVersionCommandRequest>
        {
            private readonly CreanovelDbContext _context;

            public Handler(CreanovelDbContext context)
            {
                _context = context;
            }
            
            public async Task<Unit> Handle(CreateNovelaVersionCommandRequest request, CancellationToken cancellationToken)
            {
                var novelaVersion = new Domain.Models.NovelaVersion {
                    NovelaId = request.NovelaId,
                    NumeroVersion = request.NumeroVersion,
                    Disponible = request.Disponible ?? false
                };

                await _context.NovelaVersiones.AddAsync(novelaVersion);

                var result = await _context.SaveChangesAsync();

                if (result > 0)
                {
                    return Unit.Value;
                }
                
                throw new ExceptionHandler(HttpStatusCode.BadRequest, new { message = "No se pudo registrar la version de la novela" });
            }
        }
    }
}