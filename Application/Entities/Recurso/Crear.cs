using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Application.Entities.Recurso.Dtos;
using AutoMapper;
using Domain.Models;
using MediatR;
using Persistence;

namespace Application.Entities.Recurso
{
    public class Crear
    {
        public class Execute : IRequest 
        {
            public Guid EscenaId { get; set; }
            public string TipoRecurso { get; set; }
            public string Mensaje { get; set; }
            public Guid? SiguienteRecursoId { get; set; }
            public bool PrimerRecurso { get; set; }
            public bool UltimoRecurso { get; set; }
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
                Domain.Models.Recurso recurso;

                switch (request.TipoRecurso)
                {
                    case "recurso_conversacion":
                    recurso = new Domain.Models.RecursoConversacion{
                        EscenaId = request.EscenaId,
                        Mensaje = request.Mensaje,
                        PrimerRecurso = request.PrimerRecurso,
                        UltimoRecurso = request.UltimoRecurso,
                        SiguienteRecursoId = request.SiguienteRecursoId ?? null
                    };
                    break;
                    case "recurso_decision":
                    recurso = new Domain.Models.RecursoDecision{
                        EscenaId = request.EscenaId,
                        DecisionMensaje = request.Mensaje,
                        PrimerRecurso = request.PrimerRecurso,
                        UltimoRecurso = request.UltimoRecurso
                    };
                    break;
                    default:
                    throw new Exception("No existe ese tipo de recurso");
                }

                await _context.Recursos.AddAsync(recurso);
                var result = await _context.SaveChangesAsync();

                if (result > 0)
                {
                    return Unit.Value;
                }

                throw new Exception("No se pudo registrar el recurso");
            }
        }
    }
}