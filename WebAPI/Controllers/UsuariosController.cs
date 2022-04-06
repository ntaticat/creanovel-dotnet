using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Application.Entities.Auth.Dtos;
using Application.Entities.Usuario.Dtos;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;

namespace WebAPI.Controllers
{
  [ApiController]
  [Route("api/[controller]")]
  public class UsuariosController : ControllerBase
  {
    private readonly IMediator _mediator;
    
    public UsuariosController(IMediator mediator)
    {
      _mediator = mediator;
    }

    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [HttpGet("{id}")]
    public async Task<ActionResult<UsuarioDto>> GetUsuario(Guid id)
    {
      return await _mediator.Send(new Application.Entities.Usuario.ConsultaId.UsuarioUnico{ UsuarioId = id });
    }

    [HttpPost]
    public async Task<ActionResult<Unit>> PostUsuario([FromBody] Application.Entities.Usuario.Crear.Execute data)
    {
      return await _mediator.Send(data);
    }

    [HttpPost("login")]
    public async Task<ActionResult<ResponseCredentials>> Login([FromBody] Application.Entities.Usuario.Login.Execute data)
    {
      return await _mediator.Send(data);
    }
  }
}