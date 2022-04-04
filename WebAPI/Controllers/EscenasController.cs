using System;
using System.Threading.Tasks;
using Application.Entities.Escena.Dtos;
using Microsoft.AspNetCore.Mvc;
using MediatR;

namespace WebAPI.Controllers
{
  [ApiController]
  [Route("api/[controller]")]
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
      return await _mediator.Send(new Application.Entities.Escena.ConsultaId.EscenaUnica{ EscenaId = id });
    }

    [HttpPost]
    public async Task<ActionResult<Unit>> PostEscena([FromBody] Application.Entities.Escena.Crear.Execute data)
    {
      return await _mediator.Send(data);
    }

    [HttpPatch("{id}")]
    public async Task<ActionResult<Unit>> PatchEscena(Guid id, [FromBody] Application.Entities.Escena.Editar.Execute data)
    {
      data.EscenaId = id;
      return await _mediator.Send(data);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<Unit>> DeleteEscena(Guid id)
    {
      return await _mediator.Send(new Application.Entities.Escena.Eliminar.Execute{ EscenaId = id });
    }
  }
}