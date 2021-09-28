using System;

namespace CreaNovelNETCore.DTOs.Escena
{
    public class EscenaDTO
    {
        public Guid EscenaId { get; set; }
        public string Identificador { get; set; }
        public Guid NovelaId { get; set; }
    }
}