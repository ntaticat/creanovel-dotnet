using System;
using System.Threading.Tasks;
using CreaNovelNETCore.DTOs.Lectura;
using CreaNovelNETCore.DTOs.LecturaRecursos;
using CreaNovelNETCore.Models;
using CreaNovelNETCore.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace CreaNovelNETCore.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LecturasController: ControllerBase
    {
        private readonly LecturaRepo _repo;

        public LecturasController(LecturaRepo repo)
        {
            _repo = repo;
        }
        
        [HttpPost]
        public async Task<ActionResult<LecturaDto>> PostLectura([FromBody] CreateLecturaDto lecturaDto)
        {
            return await this._repo.AddLectura(lecturaDto);
        }
        
        [HttpPost("recursos")]
        public async Task<ActionResult<LecturaDto>> PostLecturaRecurso([FromBody] LecturaRecursosDto lecturaRecursos)
        {
            return await this._repo.AddRecurso(lecturaRecursos);
        }
        
        [HttpDelete("{id}")]
        public async Task<ActionResult<LecturaDto>> DeleteLectura(Guid id)
        {
            var dbNovela = await _repo.DeleteLectura(id);
            return dbNovela;
        }
    }
}