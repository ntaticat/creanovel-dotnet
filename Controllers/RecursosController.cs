using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CreaNovelNETCore.DTOs.Recursos;
using CreaNovelNETCore.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CreaNovelNETCore.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class RecursosController : ControllerBase
    {
        private readonly RecursoRepo _repo;
        
        public RecursosController(RecursoRepo repo)
        {
            _repo = repo;
        }
        
        [HttpPost]
        public async Task<ActionResult<RecursoDTO>> PostRecurso([FromBody] CreateRecursoDTO recursoDto)
        {
            return await _repo.Create(recursoDto);
        }
        
        [HttpPost("{recursoId}/next/{recursoSiguienteId}")]
        public async Task<ActionResult<RecursoDTO>> PostSetRecursoSiguiente(Guid recursoId, Guid recursoSiguienteId)
        {
            return await _repo.SetSiguienteRecursoToConversacion(recursoId, recursoSiguienteId);
        }
        
        [HttpPost("opciones")]
        public async Task<ActionResult<RecursoDecisionOpcionDTO>> PostOpcion([FromBody] CreateRecursoDecisionOpcionDTO opcionDto)
        {
            return await _repo.SetOpcionToDecision(opcionDto);
        }
        
        [HttpDelete("{id}")]
        public async Task<ActionResult<RecursoDTO>> DeleteRecurso(Guid id)
        {
            return await _repo.Delete(id);
        }
        
        [HttpGet("{id}")]
        public async Task<ActionResult<RecursoDTO>> GetRecurso(Guid id)
        {
            return await _repo.GetById(id);
        }
        
        [HttpGet]
        public async Task<ActionResult<List<RecursoDTO>>> GetRecursos()
        {
            return await _repo.GetAll();
        }
    }
}