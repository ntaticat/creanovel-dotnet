using System.Net;
using System.Threading.Tasks;
using Application.Handlers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using WebAPI.Infrastructure.Auth;

namespace WebAPI.Features.Usuarios
{
    public static class LoginUsuario
    {
        public record LoginUsuarioRequest(string Email, string Password);

        public record ResponseCredentials(string Token);

        public sealed class Handler
        {
            private readonly UserManager<Domain.Models.Usuario> _userManager;
            private readonly SignInManager<Domain.Models.Usuario> _signInManager;
            private readonly JwtTokenService _jwtTokenService;

            public Handler(UserManager<Domain.Models.Usuario> userManager, SignInManager<Domain.Models.Usuario> signInManager, JwtTokenService jwtTokenService)
            {
                _userManager = userManager;
                _signInManager = signInManager;
                _jwtTokenService = jwtTokenService;
            }

            public async Task<ResponseCredentials> HandleAsync(LoginUsuarioRequest request)
            {
                var user = await _userManager.FindByEmailAsync(request.Email);

                if (user == null)
                {
                    throw new ExceptionHandler(HttpStatusCode.Unauthorized, new { message = "Error al autenticarse" });
                }

                var passwordChecked = await _signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: false);

                if (!passwordChecked.Succeeded)
                {
                    throw new ExceptionHandler(HttpStatusCode.Unauthorized, new { message = "Error al autenticarse" });
                }

                return new ResponseCredentials(_jwtTokenService.CreateToken(user));
            }
        }

        public static class Endpoint
        {
            public static void Map(IEndpointRouteBuilder app) =>
                app.MapPost("api/usuarios/login", async ([FromBody] LoginUsuarioRequest request, Handler handler) =>
                        Results.Ok(await handler.HandleAsync(request)))
                    .AllowAnonymous()
                    .WithTags("Usuarios");
        }
    }
}
