using System;
using System.Collections.Generic;
// using System.ComponentModel.DataAnnotations;

namespace Domain.Models
{
    public class Escena
    {
        public Guid EscenaId { get; set; }
        public string Identificador { get; set; }

        public Guid NovelaId { get; set; }
        public Novela Novela { get; set; }

        public ICollection<Recurso> Recursos { get; set; }

        public bool PrimerEscena { get; set; }
        public bool UltimaEscena { get; set; }
    }
}