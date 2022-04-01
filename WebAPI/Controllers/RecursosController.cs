using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Application.Entities.Recurso.Dtos;
using Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Domain;

namespace WebAPI.Controllers
{
  [ApiController]
  [Route("api/[controller]")]
  [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
  public class RecursosController : ControllerBase
  {
    private readonly IMapper _mapper;
    private readonly CreanovelDbContext _context;

    public RecursosController(IMapper mapper, CreanovelDbContext context)
    {
      _mapper = mapper;
      _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<List<RecursoDto>>> GetRecursos()
    {
      var dbRecursos = await _context.Recursos.ToListAsync();
      var dbRecursosDto = _mapper.Map<List<RecursoDto>>(dbRecursos);
      return dbRecursosDto;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<RecursoDto>> GetRecurso(Guid id)
    {
      var dbRecurso = await _context.Recursos.FindAsync(id);

      var type = dbRecurso.GetType() == typeof(RecursoConversacion);
      if (type)
      {
        return _mapper.Map<RecursoConversacionDto>(dbRecurso);
      }
      else
      {
        return _mapper.Map<RecursoDecisionDto>(dbRecurso);
      }
    }

    [HttpPost]
    public async Task<ActionResult<RecursoDto>> PostRecurso([FromBody] CreateRecursoDto createRecursoDto)
    {
      Recurso recurso;
      RecursoDto recursoDto;
      switch (createRecursoDto.TipoRecurso)
      {
        case "recurso_conversacion":
          recurso = _mapper.Map<RecursoConversacion>(createRecursoDto);
          break;
        case "recurso_decision":
          recurso = _mapper.Map<RecursoDecision>(createRecursoDto);
          break;
        default:
          recurso = _mapper.Map<RecursoConversacion>(createRecursoDto);
          break;
      }

      await _context.Recursos.AddAsync(recurso);
      await _context.SaveChangesAsync();

      switch (createRecursoDto.TipoRecurso)
      {
        case "recurso_conversacion":
          recursoDto = _mapper.Map<RecursoConversacionDto>(recurso);
          break;
        case "recurso_decision":
          recursoDto = _mapper.Map<RecursoDecisionDto>(recurso);
          break;
        default:
          recursoDto = _mapper.Map<RecursoConversacionDto>(recurso);
          break;
      }

      return recursoDto;
    }

    [HttpPost("{recursoId}/next/{recursoSiguienteId}")]
    public async Task<ActionResult<RecursoDto>> PostSetRecursoSiguiente(Guid recursoId, Guid recursoSiguienteId)
    {
      var dbRecurso = await _context.Recursos.OfType<RecursoConversacion>().FirstOrDefaultAsync(r => r.RecursoId == recursoId);
      var siguienteRecurso = await _context.Recursos.FindAsync(recursoSiguienteId);
      dbRecurso.SiguienteRecurso = siguienteRecurso;
      dbRecurso.SiguienteRecursoId = recursoSiguienteId;

      await _context.SaveChangesAsync();
      return _mapper.Map<RecursoDto>(dbRecurso);
    }

    [HttpPost("opciones")]
    public async Task<ActionResult<RecursoDecisionOpcionDto>> PostOpcion([FromBody] CreateRecursoDecisionOpcionDto opcionDto)
    {
      var opcion = _mapper.Map<RecursoDecisionOpcion>(opcionDto);
      _context.RecursoDecisionOpciones.Add(opcion);
      await _context.SaveChangesAsync();

      var dbRecursoDecision = await _context.RecursosDecision.FindAsync(opcion.RecursoDecisionId);
      dbRecursoDecision.Opciones.Add(opcion);
      await _context.SaveChangesAsync();

      return _mapper.Map<RecursoDecisionOpcionDto>(opcion);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<RecursoDto>> DeleteRecurso(Guid id)
    {
      var dbRecurso = await _context.Recursos.FindAsync(id);
      _context.Recursos.Remove(dbRecurso);
      await _context.SaveChangesAsync();
      return _mapper.Map<RecursoDto>(dbRecurso);
    }

    // FIXME Dapper
    // public async Task<List<RecursoDTO>> GetAllConversaciones()
    // {
    //   var dbRecursos = await _context.Recursos.OfType<RecursoConversacion>().ToListAsync();
    //   var dbRecursosDto = _mapper.Map<List<RecursoDTO>>(dbRecursos);
    //   return dbRecursosDto;
    // }

    // public async Task<List<RecursoDTO>> GetAllDecisiones()
    // {
    //   var dbRecursos = await _context.Recursos.OfType<RecursoDecision>().ToListAsync();
    //   var dbRecursosDto = _mapper.Map<List<RecursoDTO>>(dbRecursos);
    //   return dbRecursosDto;
    // }
  }
}