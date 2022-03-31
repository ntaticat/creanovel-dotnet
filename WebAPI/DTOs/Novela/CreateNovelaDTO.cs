using System;

namespace WebAPI.DTOs.Novela
{
    public class CreateNovelaDTO
    {
        public string Titulo { get; set; }
        public string Descripcion { get; set; }
        public bool Disponible { get; set; }
        public Guid? UsuarioCreadorId { get; set; }
    }
}