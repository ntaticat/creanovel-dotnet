using System;

namespace Application.Entities.Escena.Dtos
{
    public class EscenaDto
    {
        public Guid EscenaId { get; set; }
        public string Identificador { get; set; }
        public Guid NovelaId { get; set; }
    }
}