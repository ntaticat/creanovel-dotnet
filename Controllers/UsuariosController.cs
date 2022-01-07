using System;
using System.Threading.Tasks;
using CreaNovelNETCore.DTOs.Auth;
using CreaNovelNETCore.DTOs.Usuario;
using CreaNovelNETCore.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace CreaNovelNETCore.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsuariosController: ControllerBase
    {
        private readonly UsuarioRepo _repo;

    public UsuariosController(UsuarioRepo repo, IConfiguration configuration)
        {
            _repo = repo;
        }
        
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpGet("{id}")]
        public async Task<ActionResult<UsuarioDto>> GetUsuario(string id)
        {
            var usuario = await _repo.GetById(id);
            return usuario;
        }
        
        [HttpPost]
        public async Task<ActionResult<ResponseCredentials>> PostUsuario([FromBody] CreateUsuarioDto usuarioDto)
        {
            var usuario = await this._repo.CreateUsuario(usuarioDto);
            if(usuario == null)
            {
                return BadRequest("Failed");
            }
            var token = this._repo.CreateToken(usuario);
            return token;
        }

        [HttpPost("login")]
        public async Task<ActionResult<ResponseCredentials>> Login([FromBody] UserCredentials userCredentials)
        {
            var usuario = await _repo.Login(userCredentials);
            if(usuario == null)
            {
                return Unauthorized(false);
            }
            var token = this._repo.CreateToken(usuario);
            return token;
        }
    }
}