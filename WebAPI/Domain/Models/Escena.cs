using System;
using System.Collections.Generic;
// using System.ComponentModel.DataAnnotations;

namespace Domain.Models
{
    public class Escena
    {
        public Guid EscenaId { get; set; }
        public string Identificador { get; set; }

        public Guid NovelaVersionId { get; set; }
        public NovelaVersion NovelaVersion { get; set; }

        public ICollection<Recurso> Recursos { get; set; }

        // Etiquetas para agrupar/filtrar escenas en el editor (jsonb: lista de strings). Ver `EscenaEtiquetas`.
        public string Etiquetas { get; set; } = "[]";

        public bool PrimerEscena { get; set; }
        public bool UltimaEscena { get; set; }
    }
}