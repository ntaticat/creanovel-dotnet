using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Application.Handlers;
using AutoMapper;
using FluentValidation;
using MediatR;
using Persistence;

namespace Application.Commands.Novela
{
    public class CreateNovelaCommand
    {
        public class CreateNovelaCommandRequest : IRequest<Guid>
        {
            public string Titulo { get; set; }
            public string Descripcion { get; set; }
            public bool Disponible { get; set; }
            public Guid? UsuarioCreadorId { get; set; }
        }

        public class ExecuteValidation : AbstractValidator<CreateNovelaCommandRequest>
        {
            public ExecuteValidation()
            {
                RuleFor(x => x.Titulo).NotEmpty();
                RuleFor(x => x.Disponible).Must(x => x == false || x == true);
            }
        }

        public class Handler : IRequestHandler<CreateNovelaCommandRequest, Guid>
        {
            private readonly CreanovelDbContext _context;

            public Handler(CreanovelDbContext context)
            {
                _context = context;
            }

            public async Task<Guid> Handle(CreateNovelaCommandRequest request, CancellationToken cancellationToken)
            {
                await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

                try
                {
                    var novela = new Domain.Models.Novela
                    {
                        Titulo = request.Titulo,
                        Descripcion = request.Descripcion,
                        Disponible = request.Disponible,
                        UsuarioCreadorId = request.UsuarioCreadorId,
                    };

                    await this._context.Novelas.AddAsync(novela, cancellationToken);
                    var resultNovela = await _context.SaveChangesAsync(cancellationToken);

                    if (resultNovela <= 0)
                    {
                        throw new ExceptionHandler(HttpStatusCode.BadRequest, new { message = "No se pudo registrar la novela" });
                    }

                    var novelaVersion = new Domain.Models.NovelaVersion
                    {
                        NovelaId = novela.NovelaId,
                        NumeroVersion = "1.0.0",
                        Disponible = false
                    };

                    await _context.NovelaVersiones.AddAsync(novelaVersion, cancellationToken);
                    var resultNovelaVersion = await _context.SaveChangesAsync(cancellationToken);

                    if (resultNovelaVersion <= 0)
                    {
                        throw new ExceptionHandler(HttpStatusCode.BadRequest, new { message = "No se pudo registrar la versión por defecto" });
                    }

                    // Confirmar ambas operaciones
                    await transaction.CommitAsync(cancellationToken);

                    return novela.NovelaId;

                }
                catch (System.Exception)
                {

                    await transaction.RollbackAsync(cancellationToken);
                    throw new ExceptionHandler(HttpStatusCode.BadRequest, new { message = "No se pudo crear la novela y su versión por defecto" });
                }
            }
        }
    }
}