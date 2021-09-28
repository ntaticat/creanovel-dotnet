using System;
using System.Collections.Generic;
using CreaNovelNETCore.DTOs.Recursos;
using CreaNovelNETCore.Models;

namespace CreaNovelNETCore.DTOs.Escena
{
    public class EscenaRecursosDTO
    {
        public Guid EscenaId { get; set; }
        public string Identificador { get; set; }
        public ICollection<RecursoDTO> Recursos { get; set; }
        public Guid NovelaId { get; set; }
    }
}