using System;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Extensions
{
    public static class ControllerBaseExtensions
    {
        public static Guid? GetActingUsuarioId(this ControllerBase controller)
        {
            var claim = controller.User?.FindFirst("userId");

            if (claim == null || !Guid.TryParse(claim.Value, out var usuarioId))
            {
                return null;
            }

            return usuarioId;
        }
    }
}
