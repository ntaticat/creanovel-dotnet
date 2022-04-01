using System;

namespace Application.Entities.Escena.Dtos
{
    public class CreateEscenaDto
    {
        public string Identificador { get; set; }
        public Guid NovelaId { get; set; }
    }
}