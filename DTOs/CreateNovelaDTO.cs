using System;

namespace CreaNovelNETCore.DTOs
{
    public class CreateNovelaDTO
    {
        public string Titulo { get; set; }
        public string Descripcion { get; set; }
        public bool Disponible { get; set; }
        public string? UsuarioCreadorId { get; set; }
    }
}