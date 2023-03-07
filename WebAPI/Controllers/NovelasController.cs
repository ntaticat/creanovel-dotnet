using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Application.Commands.Novela;
using Application.Dtos.Novela;
using Application.Queries.Novela;
using AutoMapper;
using Persistence;
using Microsoft.AspNetCore.Mvc;
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
      return await _mediator.Send(new GetNovelasQuery.GetNovelasQueryDto());
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<NovelaNoEscenasDto>> GetNovela(Guid id)
    {
      return await _mediator.Send(new GetNovelaByIdQuery.GetNovelaByIdQueryDto{ NovelaId = id });
    }

    [HttpGet("{id}/escenas")]
    public async Task<ActionResult<NovelaWithEscenasDto>> GetNovelaWithEscenas(Guid id)
    {
      return await _mediator.Send(new GetNovelaByIdWithEscenasQuery.GetNovelaByIdWithEscenasQueryDto{ NovelaId =  id });
    }

    [HttpPost]
    public async Task<ActionResult<Unit>> PostNovela([FromBody] CreateNovelaCommand.CreateNovelaCommandDto data)
    {
      return await _mediator.Send(data);
    }

    [HttpPost("personajes")]
    public async Task<ActionResult<Unit>> PostNovelaPersonaje([FromBody] RegisterPersonajeToNovelaCommand.RegisterPersonajeToNovelaCommandDto data)
    {
      return await _mediator.Send(data);
    }

    [HttpPatch("{id}")]
    public async Task<ActionResult<Unit>> PatchNovela(Guid id, [FromBody] UpdateNovelaCommand.UpdateNovelaCommandDto data)
    {
      data.NovelaId = id;
      return await _mediator.Send(data);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<Unit>> DeleteNovela(Guid id)
    {
      return await _mediator.Send(new DeleteNovelaCommand.DeleteNovelaCommandDto{ NovelaId =  id });
    }
  }
}