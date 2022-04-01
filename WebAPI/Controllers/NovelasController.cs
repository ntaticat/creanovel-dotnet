using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Application.Entities.Novela.Dtos;
using Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MediatR;

namespace WebAPI.Controllers
{
  [ApiController]
  [Route("api/[controller]")]
  public class NovelasController : ControllerBase
  {
    private readonly IMediator _mediator;

    public NovelasController(IMapper mapper, CreanovelDbContext context, IMediator mediator)
    {
      _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<List<NovelaNoEscenasDto>>> GetNovelas()
    {
      return await _mediator.Send(new Application.Entities.Novela.Consulta.ListaNovelas());
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<NovelaNoEscenasDto>> GetNovela(Guid id)
    {
      return await _mediator.Send(new Application.Entities.Novela.ConsultaId.NovelaUnica{ NovelaId = id });
    }

    [HttpGet("{id}/escenas")]
    public async Task<ActionResult<NovelaWithEscenasDto>> GetNovelaWithEscenas(Guid id)
    {
      return await _mediator.Send(new Application.Entities.Novela.ConsultaIdEscenas.NovelaUnica{ NovelaId =  id });
    }

    [HttpPost]
    public async Task<ActionResult<Unit>> PostNovela([FromBody] Application.Entities.Novela.Crear.Execute data)
    {
      return await _mediator.Send(data);
    }

    [HttpPatch("{id}")]
    public async Task<ActionResult<Unit>> PatchNovela(Guid id, [FromBody] Application.Entities.Novela.Editar.Execute data)
    {
      data.NovelaId = id;
      return await _mediator.Send(data);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<Unit>> DeleteNovela(Guid id)
    {
      return await _mediator.Send(new Application.Entities.Novela.Eliminar.Execute{ NovelaId =  id });
    }
  }
}