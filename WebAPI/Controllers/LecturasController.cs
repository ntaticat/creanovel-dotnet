using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MediatR;

namespace WebAPI.Controllers
{
  [ApiController]
  [Route("api/[controller]")]
  public class LecturasController : ControllerBase
  {
    private readonly IMediator _mediator;

    public LecturasController(IMediator mediator)
    {
      _mediator = mediator;
    }

    [HttpPost]
    public async Task<ActionResult<Unit>> PostLectura([FromBody] Application.Entities.Lectura.Crear.Execute data)
    {
      return await _mediator.Send(data);
    }

    [HttpPost("recursos")]
    public async Task<ActionResult<Unit>> PostLecturaRecurso([FromBody] Application.Entities.Lectura.RegistrarRecurso.Execute data)
    {
      return await _mediator.Send(data);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<Unit>> DeleteLectura(Guid id)
    {
      return await _mediator.Send(new Application.Entities.Lectura.Eliminar.Execute{ LecturaId = id });
    }

    [HttpDelete("recursos")]
    public async Task<ActionResult<Unit>> DeleteLecturaRecurso(Application.Entities.Lectura.EliminarRecurso.Execute data)
    {
      return await _mediator.Send(data);
    }
  }
}