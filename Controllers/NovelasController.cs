using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using CreaNovelNETCore.DTOs;
using CreaNovelNETCore.Models;
using CreaNovelNETCore.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace CreaNovelNETCore.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NovelasController : ControllerBase
    {
        private readonly NovelaRepo _repo;

        public NovelasController(NovelaRepo repo, IMapper mapper)
        {
            this._repo = repo;
        }

        [HttpPost]
        public async Task<ActionResult<NovelaNoEscenasDTO>> PostNovela([FromBody] CreateNovelaDTO novelaDto)
        {
            return await this._repo.Create(novelaDto);
        }

        [HttpGet]
        public async Task<ActionResult<List<NovelaNoEscenasDTO>>> GetNovelas()
        {
            var novelas = await _repo.GetAll();

            return novelas;
        }
        
        [HttpGet("{id}")]
        public async Task<ActionResult<NovelaNoEscenasDTO>> GetNovela(Guid id)
        {
            var novela = await _repo.GetById(id);

            return novela;
        }
        
        [HttpGet("{id}/escenas")]
        public async Task<ActionResult<NovelaWithEscenasDTO>> GetNovelaWithEscenas(Guid id)
        {
            var novela = await _repo.GetByIdWithEscenas(id);

            return novela;
        }
        
        [HttpDelete("{id}")]
        public async Task<ActionResult<NovelaNoEscenasDTO>> DeleteNovela(Guid id)
        {
            var dbNovela = await _repo.Delete(id);

            return dbNovela;
        }
    }
}