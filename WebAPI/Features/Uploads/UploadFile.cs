using System;
using System.Net;
using System.Threading.Tasks;
using Application.Handlers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using WebAPI.Services;

namespace WebAPI.Features.Uploads
{
    public static class UploadFile
    {
        public sealed class Handler
        {
            private readonly FileStorageService _fileStorageService;

            public Handler(FileStorageService fileStorageService)
            {
                _fileStorageService = fileStorageService;
            }

            public async Task<string> HandleAsync(IFormFile file, string category)
            {
                try
                {
                    return await _fileStorageService.SaveAsync(file, category);
                }
                catch (ArgumentException ex)
                {
                    throw new ExceptionHandler(HttpStatusCode.BadRequest, new { message = ex.Message });
                }
            }
        }

        public static class Endpoint
        {
            public static void Map(IEndpointRouteBuilder app) =>
                app.MapPost("api/uploads", async (IFormFile file, string category, Handler handler) =>
                    {
                        var url = await handler.HandleAsync(file, category);
                        return Results.Ok(new { url });
                    })
                    .DisableAntiforgery()
                    .RequireAuthorization()
                    .WithTags("Uploads");
        }
    }
}
