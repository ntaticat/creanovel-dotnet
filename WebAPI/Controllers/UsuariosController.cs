using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Application.Entities.Auth.Dtos;
using Application.Entities.Usuario.Dtos;
using Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Domain.Models;
using MediatR;

namespace WebAPI.Controllers
{
  [ApiController]
  [Route("api/[controller]")]
  public class UsuariosController : ControllerBase
  {
    private readonly IMapper _mapper;
    private readonly CreanovelDbContext _context;
    private IConfiguration _configuration;
    private readonly UserManager<Usuario> _userManager;
    private readonly SignInManager<Usuario> _signInManager;
    private readonly IMediator _mediator;
    
    public UsuariosController(IConfiguration configuration, IMapper mapper, CreanovelDbContext context, UserManager<Usuario> userManager, SignInManager<Usuario> signInManager, IMediator mediator)
    {
      _mapper = mapper;
      _context = context;
      _mediator = mediator;

      _configuration = configuration;
      _userManager = userManager;
      _signInManager = signInManager;
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