using System;
using System.Collections.Generic;
using WebAPI.DTOs.Recursos;
using WebAPI.Models;

namespace WebAPI.DTOs.Escena
{
    public class EscenaRecursosDTO
    {
        public Guid EscenaId { get; set; }
        public string Identificador { get; set; }
        public ICollection<RecursoDTO> Recursos { get; set; }
        public Guid NovelaId { get; set; }
    }
}