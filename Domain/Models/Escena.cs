using System;
using System.Collections.Generic;
// using System.ComponentModel.DataAnnotations;

namespace Domain.Models
{
    public class Escena
    {
        // [Key]
        public Guid EscenaId { get; set; }
        public string Identificador { get; set; }

        public ICollection<Recurso> Recursos { get; set; }

        public Guid NovelaId { get; set; }
        public Novela Novela { get; set; }
    }
}