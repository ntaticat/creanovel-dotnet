using System;
using System.Net;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Application.Handlers;
using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Persistence;
using WebAPI.Infrastructure.Extensions;

namespace WebAPI.Features.Novelas
{
    public static class CreateNovela
    {
        public record CreateNovelaRequest(string Titulo, string Descripcion, bool Disponible, Guid? UsuarioCreadorId);

        public class Validator : AbstractValidator<CreateNovelaRequest>
        {
            public Validator()
            {
                RuleFor(x => x.Titulo).NotEmpty();
            }
        }

        public sealed class Handler
        {
            private readonly CreanovelDbContext _context;
            private readonly Validator _validator;

            public Handler(CreanovelDbContext context, Validator validator)
            {
                _context = context;
                _validator = validator;
            }

            public async Task<Guid> HandleAsync(CreateNovelaRequest request, Guid? actingUsuarioId, CancellationToken ct)
            {
                await _validator.ValidateAndThrowAsync(request, ct);

                await using var transaction = await _context.Database.BeginTransactionAsync(ct);

                try
                {
                    var novela = new Domain.Models.Novela
                    {
                        Titulo = request.Titulo,
                        Descripcion = request.Descripcion,
                        Disponible = request.Disponible,
                        UsuarioCreadorId = request.UsuarioCreadorId ?? actingUsuarioId
                    };

                    await _context.Novelas.AddAsync(novela, ct);
                    var resultNovela = await _context.SaveChangesAsync(ct);

                    if (resultNovela <= 0)
                    {
                        throw new ExceptionHandler(HttpStatusCode.BadRequest, new { message = "No se pudo registrar la novela" });
                    }

                    var novelaVersion = new Domain.Models.NovelaVersion
                    {
                        NovelaId = novela.NovelaId,
                        NumeroVersion = "1.0.0",
                        Disponible = false,
                        EsBorrador = true
                    };

                    await _context.NovelaVersiones.AddAsync(novelaVersion, ct);
                    var resultNovelaVersion = await _context.SaveChangesAsync(ct);

                    if (resultNovelaVersion <= 0)
                    {
                        throw new ExceptionHandler(HttpStatusCode.BadRequest, new { message = "No se pudo registrar la versión por defecto" });
                    }

                    await transaction.CommitAsync(ct);

                    return novela.NovelaId;
                }
                catch (Exception)
                {
                    await transaction.RollbackAsync(ct);
                    throw new ExceptionHandler(HttpStatusCode.BadRequest, new { message = "No se pudo crear la novela y su versión por defecto" });
                }
            }
        }

        public static class Endpoint
        {
            public static void Map(IEndpointRouteBuilder app) =>
                app.MapPost("api/novelas", async ([FromBody] CreateNovelaRequest request, Handler handler, ClaimsPrincipal user, CancellationToken ct) =>
                    {
                        var novelaId = await handler.HandleAsync(request, user.GetActingUsuarioId(), ct);
                        return Results.Created($"/api/novelas/{novelaId}", novelaId);
                    })
                    .RequireAuthorization()
                    .WithTags("Novelas");
        }
    }
}
