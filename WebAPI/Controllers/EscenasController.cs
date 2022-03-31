using System;
using System.Threading.Tasks;
using AutoMapper;
using WebAPI.DTOs.Escena;
using WebAPI.Models;
using WebAPI.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace WebAPI.Controllers
{
  [ApiController]
  [Route("api/[controller]")]
  public class EscenasController : ControllerBase
  {
    private readonly IMapper _mapper;
    private readonly CreanovelDbContext _context;

    public EscenasController(IMapper mapper, CreanovelDbContext context)
    {
      _mapper = mapper;
      _context = context;
    }

    // FIXME Dapper
    [HttpGet("{id}")]
    public async Task<ActionResult<EscenaRecursosDTO>> GetEscenaRecursos(Guid id)
    {
      var escena = await _context.Escenas.Include(t => t.Recursos).FirstOrDefaultAsync(e => e.EscenaId == id);
      var escenaDto = _mapper.Map<EscenaRecursosDTO>(escena);
      return escenaDto;
    }

    [HttpPost]
    public async Task<ActionResult<EscenaDTO>> PostEscena([FromBody] CreateEscenaDTO escenaDto)
    {
      var escena = _mapper.Map<Escena>(escenaDto);
      await _context.Escenas.AddAsync(escena);
      await _context.SaveChangesAsync();
      var dbEscenaDto = _mapper.Map<EscenaDTO>(escena);

      return dbEscenaDto;
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<EscenaDTO>> DeleteEscena(Guid id)
    {
      var dbEscena = await _context.Escenas.FindAsync(id);
      _context.Escenas.Remove(dbEscena);
      await _context.SaveChangesAsync();
      return _mapper.Map<EscenaDTO>(dbEscena);
    }
  }
}