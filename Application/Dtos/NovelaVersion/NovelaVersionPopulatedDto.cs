using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.Dtos.Escena;

namespace Application.Dtos.NovelaVersion
{
    public class NovelaVersionPopulatedDto
    {
        public Guid NovelaVersionId { get; set; }
        public string NumeroVersion { get; set; }
        public bool Disponible { get; set; }
        public Guid NovelaId { get; set; }
        public ICollection<EscenaRecursosDto> Escenas { get; set; }
    }
}