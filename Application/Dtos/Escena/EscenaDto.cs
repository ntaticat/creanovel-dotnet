using System;

namespace Application.Dtos.Escena
{
    public record EscenaDto
    {
        public Guid EscenaId { get; set; }
        public string Identificador { get; set; }
        public Guid NovelaVersionId { get; set; }
        public bool PrimerEscena { get; set; }
        public bool UltimaEscena { get; set; }
    }
}