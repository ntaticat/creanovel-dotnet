using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Persistence;

namespace Application.Entities.Novela
{
    public class Editar
    {
        public class Execute : IRequest
        {
            public Guid NovelaId { get; set; }
            public string Titulo { get; set; }
            public string Descripcion { get; set; }
            public bool? Disponible { get; set; }
            public Guid? UsuarioCreadorId { get; set; }
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
                var novela = await _context.Novelas.FindAsync(request.NovelaId);

                if (novela == null)
                {
                    throw new Exception("Novela no encontrada");
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

                throw new Exception("No se actualizó la novela");
            }
        }
  }
}