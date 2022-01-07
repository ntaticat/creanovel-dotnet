using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using CreaNovelNETCore.DTOs.Auth;
using CreaNovelNETCore.DTOs.Usuario;
using CreaNovelNETCore.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace CreaNovelNETCore.Repositories
{
    public class UsuarioRepo
    {
        private readonly IMapper _mapper;
        private readonly CreanovelDbContext _context;
        private IConfiguration _configuration;
        private readonly UserManager<Usuario> _userManager;
        private readonly SignInManager<Usuario> _signInManager;

    public UsuarioRepo(IMapper mapper, CreanovelDbContext context, IConfiguration configuration, UserManager<Usuario> userManager, SignInManager<Usuario> signInManager)
        {
            _mapper = mapper;
            _context = context;
            _configuration = configuration;
            _userManager = userManager;
            _signInManager = signInManager;
    }
        
        public async Task<UsuarioDto> GetById(string usuarioId)
        {
            var dbUsuario = await _context.Usuarios
                .Include(u => u.Lecturas)
                .ThenInclude(l => l.Recursos)
                .AsSplitQuery()
                .Include(u => u.NovelasCreadas)
                .AsSplitQuery()
                .FirstOrDefaultAsync(u => u.Id == usuarioId);
            var dbUsuarioDto = _mapper.Map<UsuarioDto>(dbUsuario);
            return dbUsuarioDto;
        }
        
        public async Task<Usuario> CreateUsuario(CreateUsuarioDto createUsuarioDto)
        {
            var usuario = new Usuario {
                Nombre = createUsuarioDto.Nombre,
                UserName = createUsuarioDto.UserName, 
                Email = createUsuarioDto.Email,

            };

            var createdUsuario = await _userManager.CreateAsync(usuario, createUsuarioDto.Password);
            
            if(!createdUsuario.Succeeded)
            {
                return null;
            }

            return usuario;
        }


        public ResponseCredentials CreateToken(Usuario usuario)
        {
            var claims = new List<Claim>()
            {
                new Claim("userId", usuario.Id)
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

        public async Task<Usuario> Login(UserCredentials userCredentials)
        {
            var user = await _userManager.FindByEmailAsync(userCredentials.Email);

            if(user == null)
            {
                return null;
            }

            var passwordChecked = await _signInManager.CheckPasswordSignInAsync(user, userCredentials.Password, lockoutOnFailure: false);            
            // var signInSuccess = await _signInManager.PasswordSignInAsync(user, userCredentials.Password, isPersistent: false, lockoutOnFailure: false);

            if(!passwordChecked.Succeeded)
            {
                return null;
            }

            return user;
        }
    }
}