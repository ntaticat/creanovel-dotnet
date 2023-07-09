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
    public class UpdateNovelaCommand
    {
        public class UpdateNovelaCommandRequest : IRequest
        {
            public Guid NovelaId { get; set; }
            public string Titulo { get; set; }
            public string Descripcion { get; set; }
            public bool? Disponible { get; set; }
            public Guid? UsuarioCreadorId { get; set; }
        }

        public class Handler : IRequestHandler<UpdateNovelaCommandRequest>
        {
            private readonly CreanovelDbContext _context;

            public Handler(CreanovelDbContext context)
            {
                _context = context;
            }

            public async Task<Unit> Handle(UpdateNovelaCommandRequest request, CancellationToken cancellationToken)
            {
                var novela = await _context.Novelas.FindAsync(request.NovelaId);

                if (novela == null)
                {
                    throw new ExceptionHandler(HttpStatusCode.NotFound, new { message = "Novela no encontrada" });
                }

                novela.Titulo = request.Titulo ?? novela.Titulo;
                novela.Descripcion = request.Descripcion ?? novela.Descripcion;
                novela.Disponible = request.Disponible ?? novela.Disponible;
                novela.UsuarioCreadorId = request.UsuarioCreadorId ?? novela.UsuarioCreadorId;

                var result = await _context.SaveChangesAsync();

                if (result > 0)
                {
                    return Unit.Value;
                }
                
                throw new ExceptionHandler(HttpStatusCode.BadRequest, new { message = "No se actualizó la novela" });
            }
        }
  }
}