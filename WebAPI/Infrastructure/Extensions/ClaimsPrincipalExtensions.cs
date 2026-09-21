using System;
using System.Security.Claims;

namespace WebAPI.Infrastructure.Extensions
{
    public static class ClaimsPrincipalExtensions
    {
        public static Guid? GetActingUsuarioId(this ClaimsPrincipal user)
        {
            var claim = user?.FindFirst("userId");

            if (claim == null || !Guid.TryParse(claim.Value, out var usuarioId))
            {
                return null;
            }

            return usuarioId;
        }
    }
}
