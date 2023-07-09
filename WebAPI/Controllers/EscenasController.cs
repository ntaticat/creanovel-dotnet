using System;
using System.Threading.Tasks;
using Application.Commands.Escena;
using Application.Dtos.Escena;
using Application.Queries.Escena;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;

namespace WebAPI.Controllers
{
  [ApiController]
  [Route("api/[controller]")]
  [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
  public class EscenasController : ControllerBase
  {
    private readonly IMediator _mediator;

    public EscenasController(IMediator mediator)
    {
      _mediator = mediator;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<EscenaRecursosDto>> GetEscenaRecursos(Guid id)
    {
      return await _mediator.Send(new GetEscenaByIdQuery.GetEscenaByIdQueryRequest{ EscenaId = id });
    }

    [HttpPost]
    public async Task<ActionResult<Unit>> PostEscena([FromBody] CreateEscenaCommand.CreateEscenaCommandRequest data)
    {
      return await _mediator.Send(data);
    }

    [HttpPatch("{id}")]
    public async Task<ActionResult<Unit>> PatchEscena(Guid id, [FromBody] UpdateEscenaCommand.UpdateEscenaCommandRequest data)
    {
      data.EscenaId = id;
      return await _mediator.Send(data);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<Unit>> DeleteEscena(Guid id)
    {
      return await _mediator.Send(new DeleteEscenaCommand.DeleteEscenaCommandRequest{ EscenaId = id });
    }
  }
}