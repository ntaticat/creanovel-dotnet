using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Application.Entities.Novela.Dtos;
using Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Domain;
using MediatR;
using Application.Entities.Novela;

namespace WebAPI.Controllers
{
  [ApiController]
  [Route("api/[controller]")]
  public class NovelasController : ControllerBase
  {
    private readonly IMapper _mapper;
    private readonly CreanovelDbContext _context;
    private readonly IMediator _mediator;

    public NovelasController(IMapper mapper, CreanovelDbContext context, IMediator mediator)
    {
      _mapper = mapper;
      _context = context;
      _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<List<NovelaNoEscenasDto>>> GetNovelas()
    {
      return await _mediator.Send(new Consulta.ListaNovelas());
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<NovelaNoEscenasDto>> GetNovela(Guid id)
    {
      var dbNovela = await _context.Novelas.FindAsync(id);
      var dbNovelaDto = _mapper.Map<NovelaNoEscenasDto>(dbNovela);
      return dbNovelaDto;
    }

    [HttpGet("{id}/escenas")]
    public async Task<ActionResult<NovelaWithEscenasDto>> GetNovelaWithEscenas(Guid id)
    {
      var dbNovela = await _context.Novelas.Include(n => n.Escenas)
        .FirstOrDefaultAsync(n => n.NovelaId == id);
      var dbNovelaDto = _mapper.Map<NovelaWithEscenasDto>(dbNovela);
      return dbNovelaDto;
    }

    [HttpPost]
    public async Task<ActionResult<NovelaNoEscenasDto>> PostNovela([FromBody] CreateNovelaDto novelaDto)
    {
      var novela = _mapper.Map<Novela>(novelaDto);
      await this._context.Novelas.AddAsync(novela);
      await _context.SaveChangesAsync();
      var dbNovelaDto = _mapper.Map<NovelaNoEscenasDto>(novela);
      return dbNovelaDto;
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<NovelaNoEscenasDto>> DeleteNovela(Guid id)
    {
      var dbNovela = await _context.Novelas.FindAsync(id);
      _context.Novelas.Remove(dbNovela);
      await _context.SaveChangesAsync();
      return _mapper.Map<NovelaNoEscenasDto>(dbNovela);
    }
  }
}