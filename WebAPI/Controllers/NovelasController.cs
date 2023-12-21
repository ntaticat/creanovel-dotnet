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
      return await _mediator.Send(new GetNovelasQuery.GetNovelasQueryRequest());
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<NovelaPopulatedDto>> GetNovela(Guid id, [FromQuery(Name = "versiones")] string hasVersiones, [FromQuery(Name = "personajes")] string hasPersonajes, [FromQuery(Name = "backgrounds")] string hasBackgrounds)
    {
      bool includeVersiones =  !String.IsNullOrEmpty(hasVersiones) && hasVersiones == "True";
      bool includePersonajes =  !String.IsNullOrEmpty(hasPersonajes) && hasPersonajes == "True";
      bool includeBackgrounds =  !String.IsNullOrEmpty(hasBackgrounds) && hasBackgrounds == "True";

      return await _mediator.Send(new GetNovelaByIdQuery.GetNovelaByIdQueryRequest{ NovelaId = id, HasVersiones = includeVersiones, HasBackgrounds = includeBackgrounds, HasPersonajes = includePersonajes });
    }

    [HttpPost]
    public async Task<ActionResult<Unit>> PostNovela([FromBody] CreateNovelaCommand.CreateNovelaCommandRequest data)
    {
      return await _mediator.Send(data);
    }

    [HttpPost("personajes")]
    public async Task<ActionResult<Unit>> PostNovelaPersonaje([FromBody] RegisterPersonajeToNovelaCommand.RegisterPersonajeToNovelaCommandRequest data)
    {
      return await _mediator.Send(data);
    }

    [HttpPatch("{id}")]
    public async Task<ActionResult<Unit>> PatchNovela(Guid id, [FromBody] UpdateNovelaCommand.UpdateNovelaCommandRequest data)
    {
      data.NovelaId = id;
      return await _mediator.Send(data);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<Unit>> DeleteNovela(Guid id)
    {
      return await _mediator.Send(new DeleteNovelaCommand.DeleteNovelaCommandRequest{ NovelaId =  id });
    }
  }
}