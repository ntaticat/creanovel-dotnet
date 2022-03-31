using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using CreaNovelNETCore.DTOs.Auth;
using CreaNovelNETCore.DTOs.Usuario;
using CreaNovelNETCore.Models;
using CreaNovelNETCore.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace CreaNovelNETCore.Controllers
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

    public UsuariosController(IConfiguration configuration, IMapper mapper, CreanovelDbContext context, UserManager<Usuario> userManager, SignInManager<Usuario> signInManager)
    {
      _mapper = mapper;
      _context = context;

      _configuration = configuration;
      _userManager = userManager;
      _signInManager = signInManager;
    }

    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [HttpGet("{id}")]
    public async Task<ActionResult<UsuarioDto>> GetUsuario(Guid id)
    {
      var dbUsuario = await _context.Usuarios
        .Include(u => u.Lecturas)
        .ThenInclude(l => l.Recursos)
        .AsSplitQuery()
        .Include(u => u.NovelasCreadas)
        .AsSplitQuery()
        .FirstOrDefaultAsync(u => u.Id == id);
      var dbUsuarioDto = _mapper.Map<UsuarioDto>(dbUsuario);
      return dbUsuarioDto;
    }

    [HttpPost]
    public async Task<ActionResult<ResponseCredentials>> PostUsuario([FromBody] CreateUsuarioDto createUsuarioDto)
    {
      var usuario = new Usuario
      {
        Nombre = createUsuarioDto.Nombre,
        UserName = createUsuarioDto.UserName,
        Email = createUsuarioDto.Email,

      };

      var createdUsuario = await _userManager.CreateAsync(usuario, createUsuarioDto.Password);

      if (!createdUsuario.Succeeded)
      {
        return BadRequest("Failed");
      }

      var token = CreateToken(usuario);
      return token;
    }

    [HttpPost("login")]
    public async Task<ActionResult<ResponseCredentials>> Login([FromBody] UserCredentials userCredentials)
    {

      var user = await _userManager.FindByEmailAsync(userCredentials.Email);

      if (user == null)
      {
        return Unauthorized(false);
      }

      var passwordChecked = await _signInManager.CheckPasswordSignInAsync(user, userCredentials.Password, lockoutOnFailure: false);
      // var signInSuccess = await _signInManager.PasswordSignInAsync(user, userCredentials.Password, isPersistent: false, lockoutOnFailure: false);

      if (!passwordChecked.Succeeded)
      {
        return Unauthorized(false);
      }

      var token = CreateToken(user);
      return token;
    }

    private ResponseCredentials CreateToken(Usuario usuario)
    {
      var claims = new List<Claim>()
            {
                new Claim("userId", usuario.Id.ToString())
            };

      var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWTKey"]));
      var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
      var expiration = DateTime.UtcNow.AddYears(1);
      var securityToken = new JwtSecurityToken(issuer: null, audience: null, claims: claims, expires: expiration, signingCredentials: creds);

      return new ResponseCredentials()
      {
        Token = new JwtSecurityTokenHandler().WriteToken(securityToken)
      };
    }
  }
}