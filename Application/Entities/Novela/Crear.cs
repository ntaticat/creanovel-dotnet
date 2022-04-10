using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Application.Entities.Novela.Dtos;
using Application.Handlers;
using AutoMapper;
using FluentValidation;
using MediatR;
using Persistence;

namespace Application.Entities.Novela
{
    public class Crear
    {
        public class Execute : IRequest 
        {
            public string Titulo { get; set; }
            public string Descripcion { get; set; }
            public bool Disponible { get; set; }
            public Guid? UsuarioCreadorId { get; set; }
        }

        public class ExecuteValidation : AbstractValidator<Execute>
        {
            public ExecuteValidation()
            {
                RuleFor( x => x.Titulo ).NotEmpty();
                RuleFor( x => x.Disponible ).NotEmpty();
            }
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
                var novela = new Domain.Models.Novela {
                    Titulo = request.Titulo, 
                    Descripcion = request.Descripcion, 
                    Disponible = request.Disponible, 
                    UsuarioCreadorId = request.UsuarioCreadorId, 
                };
                await this._context.Novelas.AddAsync(novela);
                var result = await _context.SaveChangesAsync();

                if (result > 0)
                {
                    return Unit.Value;
                }
                
                throw new ExceptionHandler(HttpStatusCode.BadRequest, new { message = "No se pudo registrar la novela" });
            }
        }
  }
}