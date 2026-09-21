using System;
using System.Collections.Generic;

namespace Domain.Models
{
    public class Lectura
    {
        public Guid LecturaId { get; set; }

        public Guid NovelaRegistrosId { get; set; }
        public Novela NovelaRegistros { get; set; }

        public ICollection<LecturaRecursos> Recursos { get; set; }

        // Partida guardada: estado del motor (variables, ...) serializado, y dónde quedó el jugador.
        // Son columnas planas sin FK: es una foto de la partida, no una relación.
        public string Estado { get; set; } = "{}";
        public Guid? RecursoActualId { get; set; }
        public Guid? NovelaVersionId { get; set; }

        public Guid UsuarioPropietarioId { get; set; }
        public Usuario UsuarioPropietario { get; set; }
    }
}