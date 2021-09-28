using System;
using System.Collections.Generic;

namespace CreaNovelNETCore.Models
{
    public class Lectura
    {
        public Guid LecturaId { get; set; }

        public Guid NovelaRegistrosId { get; set; }
        public Novela NovelaRegistros { get; set; }

        public ICollection<LecturaRecursos> Recursos { get; set; }
        
        
        public Guid UsuarioPropietarioId { get; set; }
        public Usuario UsuarioPropietario { get; set; }
    }
}