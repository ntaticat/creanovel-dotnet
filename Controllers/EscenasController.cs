using System;
using System.Threading.Tasks;
using CreaNovelNETCore.DTOs.Escena;
using CreaNovelNETCore.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace CreaNovelNETCore.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EscenasController : ControllerBase
    {
        private readonly EscenaRepo _repo;
        
        public EscenasController(EscenaRepo repo)
        {
            _repo = repo;
        }
        
        [HttpGet("{id}")]
        public async Task<ActionResult<EscenaRecursosDTO>> GetEscenaRecursos(Guid id)
        {
            var escenaRecursosDto = await _repo.GetByIdWithRecursos(id);
            return escenaRecursosDto;
        }

        [HttpPost]
        public async Task<ActionResult<EscenaDTO>> PostEscena([FromBody] CreateEscenaDTO escenaDto)
        {
            return await _repo.Create(escenaDto);
        }
        
        [HttpDelete("{id}")]
        public async Task<ActionResult<EscenaDTO>> DeleteEscena(Guid id)
        {
            return await _repo.Delete(id);
        }
    }
}