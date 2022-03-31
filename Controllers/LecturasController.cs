using System;
using System.Threading.Tasks;
using AutoMapper;
using CreaNovelNETCore.DTOs.Lectura;
using CreaNovelNETCore.DTOs.LecturaRecursos;
using CreaNovelNETCore.Models;
using CreaNovelNETCore.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CreaNovelNETCore.Controllers
{
  [ApiController]
  [Route("api/[controller]")]
  public class LecturasController : ControllerBase
  {
    private readonly IMapper _mapper;
    private readonly CreanovelDbContext _context;

    public LecturasController(IMapper mapper, CreanovelDbContext context)
    {
      _mapper = mapper;
      _context = context;
    }

    [HttpPost]
    public async Task<ActionResult<LecturaDto>> PostLectura([FromBody] CreateLecturaDto lecturaDto)
    {
      var lectura = _mapper.Map<Lectura>(lecturaDto);
      await _context.Lecturas.AddAsync(lectura);
      await _context.SaveChangesAsync();
      return _mapper.Map<LecturaDto>(lectura);
    }

    [HttpPost("recursos")]
    public async Task<ActionResult<LecturaDto>> PostLecturaRecurso([FromBody] LecturaRecursosDto lecturaRecursosDto)
    {
      var lecturaRecursos = _mapper.Map<LecturaRecursos>(lecturaRecursosDto);
      lecturaRecursos.Lectura = await _context.Lecturas.FindAsync(lecturaRecursos.LecturaId);
      lecturaRecursos.Recurso = await _context.Recursos.FindAsync(lecturaRecursos.RecursoId);
      await _context.LecturaRecurso.AddAsync(lecturaRecursos);
      await _context.SaveChangesAsync();

      var dbLectura = await _context.Lecturas.Include(l => l.Recursos).FirstOrDefaultAsync(l => l.LecturaId == lecturaRecursos.LecturaId);

      return _mapper.Map<LecturaDto>(dbLectura);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<LecturaDto>> DeleteLectura(Guid id)
    {
      var dbLectura = await _context.Lecturas.FindAsync(id);
      _context.Lecturas.Remove(dbLectura);
      await _context.SaveChangesAsync();
      return _mapper.Map<LecturaDto>(dbLectura);
    }
  }
}