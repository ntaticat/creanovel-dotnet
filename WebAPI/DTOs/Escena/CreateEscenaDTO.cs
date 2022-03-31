using System;

namespace WebAPI.DTOs.Escena
{
    public class CreateEscenaDTO
    {
        public string Identificador { get; set; }
        public Guid NovelaId { get; set; }
    }
}