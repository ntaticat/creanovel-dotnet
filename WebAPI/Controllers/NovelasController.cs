using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using WebAPI.DTOs;
using WebAPI.DTOs.Novela;
using WebAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace WebAPI.Controllers
{
  [ApiController]
  [Route("api/[controller]")]
  public class NovelasController : ControllerBase
  {
    private readonly IMapper _mapper;
    private readonly CreanovelDbContext _context;

    public NovelasController(IMapper mapper, CreanovelDbContext context)
    {
      _mapper = mapper;
      _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<List<NovelaNoEscenasDTO>>> GetNovelas()
    {
      var dbNovelas = await _context.Novelas.ToListAsync();
      var novelasDto = _mapper.Map<List<NovelaNoEscenasDTO>>(dbNovelas);
      return novelasDto;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<NovelaNoEscenasDTO>> GetNovela(Guid id)
    {
      var dbNovela = await _context.Novelas.FindAsync(id);
      var dbNovelaDto = _mapper.Map<NovelaNoEscenasDTO>(dbNovela);
      return dbNovelaDto;
    }

    [HttpGet("{id}/escenas")]
    public async Task<ActionResult<NovelaWithEscenasDTO>> GetNovelaWithEscenas(Guid id)
    {
      var dbNovela = await _context.Novelas.Include(n => n.Escenas)
        .FirstOrDefaultAsync(n => n.NovelaId == id);
      var dbNovelaDto = _mapper.Map<NovelaWithEscenasDTO>(dbNovela);
      return dbNovelaDto;
    }

    [HttpPost]
    public async Task<ActionResult<NovelaNoEscenasDTO>> PostNovela([FromBody] CreateNovelaDTO novelaDto)
    {
      var novela = _mapper.Map<Novela>(novelaDto);
      await this._context.Novelas.AddAsync(novela);
      await _context.SaveChangesAsync();
      var dbNovelaDto = _mapper.Map<NovelaNoEscenasDTO>(novela);
      return dbNovelaDto;
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<NovelaNoEscenasDTO>> DeleteNovela(Guid id)
    {
      var dbNovela = await _context.Novelas.FindAsync(id);
      _context.Novelas.Remove(dbNovela);
      await _context.SaveChangesAsync();
      return _mapper.Map<NovelaNoEscenasDTO>(dbNovela);
    }
  }
}