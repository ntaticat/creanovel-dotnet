using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Application.Commands.Usuario;
using Application.Dtos.Auth;
using Application.Dtos.Usuario;
using Application.Queries.Usuario;
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
      return await _mediator.Send(new GetUsuarioByIdQuery.GetUsuarioByIdQueryDto{ UsuarioId = id });
    }

    [HttpPost]
    public async Task<ActionResult<Unit>> PostUsuario([FromBody] CreateUsuarioCommand.CreateUsuarioCommandDto data)
    {
      return await _mediator.Send(data);
    }

    [HttpPost("login")]
    public async Task<ActionResult<ResponseCredentials>> Login([FromBody] LogInUsuarioCommand.LogInUsuarioCommandDto data)
    {
      return await _mediator.Send(data);
    }
  }
}