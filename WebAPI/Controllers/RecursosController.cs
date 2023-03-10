using System;
using System.Threading.Tasks;
using Application.Commands.Recurso;
using Application.Dtos.Recurso;
using Application.Queries.Recurso;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;

namespace WebAPI.Controllers
{
  [ApiController]
  [Route("api/[controller]")]
  [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
  public class RecursosController : ControllerBase
  {
    private readonly IMediator _mediator;

    public RecursosController(IMediator mediator)
    {
      _mediator = mediator;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<RecursoDto>> GetRecurso(Guid id)
    {
      return await _mediator.Send(new GetRecursoByIdQuery.GetRecursoByIdQueryDto{ RecursoId = id });
    }

    [HttpGet("first/{id}")]
    public async Task<ActionResult<RecursoDto>> GetPrimerRecurso(Guid id)
    {
      return await _mediator.Send(new GetFirstRecursoOfNovelaQuery.GetFirstRecursoOfNovelaQueryDto{ NovelaId = id });
    }

    [HttpPost]
    public async Task<ActionResult<Unit>> PostRecurso([FromBody] CreateRecursoCommand.CreateRecursoCommandDto data)
    {
      return await _mediator.Send(data);
    }
    
    [HttpPost("{recursoId}/next/{recursoSiguienteId}")]
    public async Task<ActionResult<Unit>> PostSetRecursoSiguiente(Guid recursoId, Guid recursoSiguienteId)
    {
      return await _mediator.Send(new SetNextRecursoToRecursoCommand.SetNextRecursoToRecursoCommandDto{ RecursoId = recursoId, RecursoSiguienteId = recursoSiguienteId });
    }

    [HttpPost("opciones")]
    public async Task<ActionResult<Unit>> PostOpcion([FromBody] AddRecursoDecisionOpcionCommand.AddRecursoDecisionOpcionCommandDto data)
    {
      return await _mediator.Send(data);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<Unit>> DeleteRecurso(Guid id)
    {
      return await _mediator.Send(new DeleteRecursoCommand.DeleteRecursoCommandDto{ RecursoId = id });
    }
  }
}