using System.Net;
using System.Threading.Tasks;
using Application.Handlers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace WebAPI.Features.Usuarios
{
    public static class RegisterUsuario
    {
        public record RegisterUsuarioRequest(string Nombre, string Email, string UserName, string Password);

        public sealed class Handler
        {
            private readonly UserManager<Domain.Models.Usuario> _userManager;

            public Handler(UserManager<Domain.Models.Usuario> userManager)
            {
                _userManager = userManager;
            }

            public async Task HandleAsync(RegisterUsuarioRequest request)
            {
                var usuario = new Domain.Models.Usuario
                {
                    Nombre = request.Nombre,
                    UserName = request.UserName,
                    Email = request.Email
                };

                var created = await _userManager.CreateAsync(usuario, request.Password);

                if (!created.Succeeded)
                {
                    throw new ExceptionHandler(HttpStatusCode.BadRequest, new { message = "No se pudo registrar el usuario" });
                }
            }
        }

        public static class Endpoint
        {
            public static void Map(IEndpointRouteBuilder app) =>
                app.MapPost("api/usuarios", async ([FromBody] RegisterUsuarioRequest request, Handler handler) =>
                    {
                        await handler.HandleAsync(request);
                        return Results.NoContent();
                    })
                    .AllowAnonymous()
                    .WithTags("Usuarios");
        }
    }
}
