using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace WebAPI.Services
{
    public class FileStorageService
    {
        private static readonly string[] AllowedCategories = { "personajes", "backgrounds", "portadas", "objetos" };

        private readonly IWebHostEnvironment _env;

        public FileStorageService(IWebHostEnvironment env)
        {
            _env = env;
        }

        public async Task<string> SaveAsync(IFormFile file, string category)
        {
            if (file == null || file.Length == 0)
            {
                throw new ArgumentException("Archivo inválido");
            }

            if (Array.IndexOf(AllowedCategories, category) < 0)
            {
                throw new ArgumentException("Categoría de subida inválida");
            }

            var webRootPath = _env.WebRootPath ?? Path.Combine(_env.ContentRootPath, "wwwroot");
            var uploadsDir = Path.Combine(webRootPath, "uploads", category);
            Directory.CreateDirectory(uploadsDir);

            var extension = Path.GetExtension(file.FileName);
            var fileName = $"{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(uploadsDir, fileName);

            await using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return $"/uploads/{category}/{fileName}";
        }
    }
}
