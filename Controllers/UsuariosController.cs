using System;
using System.Threading.Tasks;
using CreaNovelNETCore.DTOs.Usuario;
using CreaNovelNETCore.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace CreaNovelNETCore.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsuariosController: ControllerBase
    {
        private readonly UsuarioRepo _repo;

        public UsuariosController(UsuarioRepo repo)
        {
            _repo = repo;
        }
        
        [HttpGet("{id}")]
        public async Task<ActionResult<UsuarioDto>> GetUsuario(Guid id)
        {
            var usuario = await _repo.GetById(id);

            return usuario;
        }
        
        [HttpPost]
        public async Task<ActionResult<UsuarioDto>> PostUsuario([FromBody] CreateUsuarioDto usuarioDto)
        {
            return await this._repo.CreateUsuario(usuarioDto);
        }
    }
}