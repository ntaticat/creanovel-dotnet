using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.Entities.Recurso.Dtos;
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
      return await _mediator.Send(new Application.Entities.Recurso.ConsultaId.RecursoUnico{ RecursoId = id });
    }

    [HttpPost]
    public async Task<ActionResult<Unit>> PostRecurso([FromBody] Application.Entities.Recurso.Crear.Execute data)
    {
      return await _mediator.Send(data);
    }

    [HttpPost("{recursoId}/next/{recursoSiguienteId}")]
    public async Task<ActionResult<Unit>> PostSetRecursoSiguiente(Guid recursoId, Guid recursoSiguienteId)
    {
      return await _mediator.Send(new Application.Entities.Recurso.AgregarSiguienteRecursoConversacion.Execute{ RecursoId = recursoId, RecursoSiguienteId = recursoSiguienteId });
    }

    [HttpPost("opciones")]
    public async Task<ActionResult<Unit>> PostOpcion([FromBody] Application.Entities.Recurso.AgregarOpcionDecision.Execute data)
    {
      return await _mediator.Send(data);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<Unit>> DeleteRecurso(Guid id)
    {
      return await _mediator.Send(new Application.Entities.Recurso.Eliminar.Execute{ RecursoId = id });
    }
  }
}