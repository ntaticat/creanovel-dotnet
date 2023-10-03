// using System;
// using System.Collections.Generic;
// using System.Linq;
// using System.Net;
// using System.Threading;
// using System.Threading.Tasks;
// using Application.Dtos.Recurso;
// using Application.Handlers;
// using AutoMapper;
// using Domain.Models;
// using MediatR;
// using Microsoft.EntityFrameworkCore;
// using Persistence;
//
// namespace Application.Queries.Recurso
// {
//     public class GetFirstRecursoOfNovelaQuery
//     {
//         public class GetFirstRecursoOfNovelaQueryRequest : IRequest<RecursoDto> {
//             public Guid NovelaId { get; set; }
//         }
//
//         public class Handler : IRequestHandler<GetFirstRecursoOfNovelaQueryRequest, RecursoDto>
//         {
//             private readonly CreanovelDbContext _context;
//             private readonly IMapper _mapper;
//
//             public Handler(CreanovelDbContext context, IMapper mapper)
//             {
//                 _context = context;
//                 _mapper = mapper;
//             }
//
//             public async Task<RecursoDto> Handle(GetFirstRecursoOfNovelaQueryRequest request, CancellationToken cancellationToken)
//             {
//                 var novela = await _context.Novelas.Include(n => n.Escenas).FirstOrDefaultAsync(n => n.NovelaId == request.NovelaId);
//
//                 if (novela == null)
//                 {
//                     throw new ExceptionHandler(HttpStatusCode.NotFound, new { message = "Novela del recurso no encontrada" });
//                 }
//
//                 var primerEscena = novela.Escenas.ToList().Find(e => e.PrimerEscena == true);
//                 
//                 if(primerEscena == null)
//                 {
//                     throw new ExceptionHandler(HttpStatusCode.NotFound, new { message = "primera escena de la novela no fue encontrada" });
//                 }
//
//                 var escena = await _context.Escenas.Include(e => e.Recursos).FirstOrDefaultAsync(e => e.EscenaId == primerEscena.EscenaId);
//
//                 if(escena == null)
//                 {
//                     throw new ExceptionHandler(HttpStatusCode.NotFound, new { message = "data de la primera escena no fue encontrada" });
//                 }
//
//                 var primerRecurso = escena.Recursos.ToList().Find(r => r.PrimerRecurso == true);
//
//                 if(primerRecurso == null)
//                 {
//                     throw new ExceptionHandler(HttpStatusCode.NotFound, new { message = "primer recurso de la escena no fue encontrado" });
//                 }
//
//
//                 var recurso = await _context.Recursos.FindAsync(primerRecurso.RecursoId);
//
//                 if (recurso == null)
//                 {
//                     throw new ExceptionHandler(HttpStatusCode.NotFound, new { message = "data del primer recurso no fue encontrado" });
//                 }
//
//                 var type = recurso.GetType();
//
//                 var isRecursoConversacion = type == typeof(RecursoConversacion);
//                 var isRecursoDecision = type == typeof(RecursoDecision);
//
//                 if(isRecursoDecision)
//                 {
//                     var nuevo = await _context.RecursosDecision.Include(r => r.Opciones).FirstOrDefaultAsync(r => r.RecursoId == primerRecurso.RecursoId);
//                     var opciones = nuevo.Opciones;
//                 }
//                 
//                 if (isRecursoConversacion)
//                 {
//                     return _mapper.Map<RecursoConversacionDto>(recurso);
//                 }
//                 if (isRecursoDecision)
//                 {
//                     return _mapper.Map<RecursoDecisionDto>(recurso);
//                 }
//
//                 return _mapper.Map<RecursoDto>(recurso);
//
//             }
//         }
//     }
// }