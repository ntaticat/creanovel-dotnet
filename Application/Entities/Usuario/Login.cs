using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Application.Entities.Auth.Dtos;
using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Persistence;

namespace Application.Entities.Usuario
{
    public class Login
    {
        public class Execute : IRequest<ResponseCredentials>
        {
            public string Email { get; set; }
            public string Password { get; set; }
        }

        public class Handler : IRequestHandler<Execute, ResponseCredentials>
        {
            private readonly CreanovelDbContext _context;
            private readonly IMapper _mapper;
            private readonly SignInManager<Domain.Models.Usuario> _signInManager;
            private readonly UserManager<Domain.Models.Usuario> _userManager;
            private IConfiguration _configuration;

      public Handler(CreanovelDbContext context, IMapper mapper, UserManager<Domain.Models.Usuario> userManager, IConfiguration configuration, SignInManager<Domain.Models.Usuario> signInManager)
            {
                _context = context;
                _mapper = mapper;
                _userManager = userManager;
                _configuration = configuration;
                _signInManager = signInManager;
            }

            public async Task<ResponseCredentials> Handle(Execute request, CancellationToken cancellationToken)
            {
                var user = await _userManager.FindByEmailAsync(request.Email);

                if (user == null)
                {
                    throw new Exception("Error al autenticarse");
                }

                var passwordChecked = await _signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: false);
                // var signInSuccess = await _signInManager.PasswordSignInAsync(user, userCredentials.Password, isPersistent: false, lockoutOnFailure: false);

                if (!passwordChecked.Succeeded)
                {
                    throw new Exception("Error al autenticarse");
                }

                var token = CreateToken(user);
                return token;
            }

            private ResponseCredentials CreateToken(Domain.Models.Usuario usuario)
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
}