using System;

namespace Application.Entities.Novela.Dtos
{
    public class CreateNovelaDto
    {
        public string Titulo { get; set; }
        public string Descripcion { get; set; }
        public bool Disponible { get; set; }
        public Guid? UsuarioCreadorId { get; set; }
    }
}