using System;
using System.Threading.Tasks;
using Application.Commands.Lectura;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;

namespace WebAPI.Controllers
{
  [ApiController]
  [Route("api/[controller]")]
  [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
  public class LecturasController : ControllerBase
  {
    private readonly IMediator _mediator;

    public LecturasController(IMediator mediator)
    {
      _mediator = mediator;
    }

    [HttpPost]
    public async Task<ActionResult<Unit>> PostLectura([FromBody] CreateLecturaCommand.CreateLecturaCommandRequest data)
    {
      return await _mediator.Send(data);
    }

    [HttpPost("recursos")]
    public async Task<ActionResult<Unit>> PostLecturaRecurso([FromBody] AddRecursoToLecturaCommand.AddRecursoToLecturaCommandRequest data)
    {
      return await _mediator.Send(data);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<Unit>> DeleteLectura(Guid id)
    {
      return await _mediator.Send(new DeleteLecturaCommand.DeleteLecturaCommandRequest{ LecturaId = id });
    }

    [HttpDelete("recursos")]
    public async Task<ActionResult<Unit>> DeleteLecturaRecurso(RemoveRecursoFromLecturaCommand.RemoveRecursoFromLecturaCommandRequest data)
    {
      return await _mediator.Send(data);
    }
  }
}